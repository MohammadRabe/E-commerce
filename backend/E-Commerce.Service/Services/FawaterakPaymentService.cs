using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using E_commerce.Data.Dtos.Payment.Fawaterak;
using E_commerce.Data.Entities;
using E_commerce.Data.Options;
using E_commerce.Infrastructure.Abstraction;
using E_commerce.Service.Abstraction;
using E_commerce.Service.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Logging;

namespace E_commerce.Service.Services;

public sealed class FawaterakPaymentService(
    IUOW uow,
    IHttpClientFactory clients,
    IOptions<FawaterakSettings> settings,
    ILogger<FawaterakPaymentService> logger) : IPaymentService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly SemaphoreSlim _tokenLock = new(1, 1);
    private string? _accessToken;
    private DateTimeOffset _tokenExpiresAt;

    public async Task<FawaterakTransactionDataResultModel?> CreatePaymentAsync(
        int orderId, string userId, CancellationToken cancellationToken = default)
    {
        using var logScope = BeginPaymentLogScope("create", orderId);
        var timer = Stopwatch.StartNew();
        logger.LogInformation("Payment creation started for order {OrderId}", orderId);
        try
        {
            var result = await CreatePaymentCoreAsync(orderId, userId, cancellationToken);
            if (result is null)
                logger.LogWarning("Payment creation stopped because order {OrderId} was not found or not accessible", orderId);
            else
                logger.LogInformation("Payment creation completed for order {OrderId} in {ElapsedMilliseconds} ms; checkout URL returned: {HasCheckoutUrl}",
                    orderId, timer.ElapsedMilliseconds, !string.IsNullOrWhiteSpace(result.Url));
            return result;
        }
        catch (PaymentRuleException exception)
        {
            logger.LogWarning("Payment creation rejected for order {OrderId} with HTTP {StatusCode}: {Reason}",
                orderId, (int)exception.StatusCode, exception.Message);
            throw;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            logger.LogInformation("Payment creation cancelled for order {OrderId} after {ElapsedMilliseconds} ms",
                orderId, timer.ElapsedMilliseconds);
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Payment creation failed for order {OrderId} after {ElapsedMilliseconds} ms",
                orderId, timer.ElapsedMilliseconds);
            throw;
        }
    }

    private async Task<FawaterakTransactionDataResultModel?> CreatePaymentCoreAsync(
        int orderId, string userId, CancellationToken cancellationToken)
    {
        var order = await uow.OrderRepository.GetForPaymentAsync(orderId, cancellationToken);
        if (order is null || order.UserId != userId)
            return null;
        logger.LogInformation("Payment order {OrderId} loaded with {ItemCount} items and status {OrderStatus}/{PaymentStatus}",
            orderId, order.Items.Count, order.Status, order.PaymentStatus);
        if (order.PaymentStatus == "Paid")
            throw new PaymentRuleException("هذا الطلب مدفوع بالفعل. يمكنك مراجعة حالته من صفحة طلباتك.", HttpStatusCode.Conflict);
        if (order.Status == E_commerce.Data.Enum.OrderStatus.Cancelled)
            throw new PaymentRuleException("لا يمكن دفع طلب ملغي. أنشئ طلبًا جديدًا لإتمام الشراء.", HttpStatusCode.Conflict);
        if (!string.IsNullOrWhiteSpace(order.PaymentIntentKey) && !string.IsNullOrWhiteSpace(order.PaymentUrl))
        {
            logger.LogInformation("Reusing existing checkout session for order {OrderId}", orderId);
            return new(order.Id, order.PaymentIntentKey, order.PaymentUrl, 0);
        }
        if (order.Items.Count == 0 || order.TotalAmount <= 0)
            throw new PaymentRuleException("لا يحتوي الطلب على منتجات قابلة للدفع. راجع تفاصيل الطلب أو أنشئ طلبًا جديدًا.", HttpStatusCode.Conflict);

        var names = order.User.FullName.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (names.Length < 2)
            throw new PaymentRuleException("أكمل الاسم الأول واسم العائلة في ملفك الشخصي قبل الدفع.", HttpStatusCode.BadRequest);

        var request = new CreateTransactionRequest(
            order.Currency,
            order.TotalAmount,
            new CustomerRequest(names[0], string.Join(' ', names.Skip(1)), order.User.Email,
                order.ShippingPhoneNumber, order.ShippingAddress),
            order.Items.Select(item => new CartItemRequest(item.Product.Title, item.UnitPrice, item.Quantity)).ToArray(),
            new RedirectionUrlsRequest(
                BuildReturnUrl(order.Id, "success"),
                BuildReturnUrl(order.Id, "failed"),
                BuildReturnUrl(order.Id, "pending")),
            new Dictionary<string, int> { ["orderId"] = order.Id });

        var json = JsonSerializer.Serialize(request);


        var token = await GetAccessTokenAsync(cancellationToken);
        logger.LogInformation("Sending transaction creation request to payment provider for order {OrderId}", orderId);

        var url = $"{settings.Value.ApiBaseUrl.TrimEnd('/')}/createTransaction";

        

        using var message = new HttpRequestMessage(HttpMethod.Post, url);
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        message.Content = JsonContent.Create(request, options: JsonOptions);
        using var response = await clients.CreateClient(nameof(FawaterakPaymentService))
            .SendAsync(message, cancellationToken);


        logger.LogInformation("Payment provider transaction request for order {OrderId} returned HTTP {StatusCode}",
            orderId, (int)response.StatusCode);
        await EnsureGatewaySuccessAsync(response, "create checkout");
        var result = await response.Content.ReadFromJsonAsync<CreateTransactionResponse>(JsonOptions, cancellationToken)
            ?? throw new HttpRequestException("Fawaterak returned an empty transaction response.");
        if (!string.Equals(result.Status, "success", StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(result.Data?.IntentKey) || string.IsNullOrWhiteSpace(result.Data.Url))
            throw new HttpRequestException("Fawaterak did not return a hosted checkout URL.");

        order.PaymentIntentKey = result.Data.IntentKey;
        order.PaymentUrl = result.Data.Url;
        order.PaymentStatus = "Pending";
        uow.OrderRepository.Edit(order);
        await uow.SaveAsync();
        logger.LogInformation("Checkout session saved for order {OrderId}; expires in {ExpiresInSeconds} seconds",
            orderId, result.Data.ExpiresIn);
        return new(order.Id, result.Data.IntentKey, result.Data.Url, result.Data.ExpiresIn);
    }

    public async Task<PaymentVerificationResult?> VerifyPaymentAsync(int orderId, string userId, CancellationToken cancellationToken = default)
    {
        using var logScope = BeginPaymentLogScope("verify", orderId);
        var timer = Stopwatch.StartNew();
        logger.LogInformation("Payment verification started for order {OrderId}", orderId);
        try
        {
            var result = await VerifyPaymentCoreAsync(orderId, userId, cancellationToken);
            logger.LogInformation("Payment verification completed for order {OrderId} with result {IsPaid} in {ElapsedMilliseconds} ms",
                orderId, result, timer.ElapsedMilliseconds);
            return result;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            logger.LogInformation("Payment verification cancelled for order {OrderId} after {ElapsedMilliseconds} ms",
                orderId, timer.ElapsedMilliseconds);
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Payment verification failed for order {OrderId} after {ElapsedMilliseconds} ms",
                orderId, timer.ElapsedMilliseconds);
            throw;
        }
    }

    private async Task<PaymentVerificationResult?> VerifyPaymentCoreAsync(int orderId, string userId, CancellationToken cancellationToken)
    {
        var order = await uow.OrderRepository.GetForPaymentAsync(orderId, cancellationToken);
        if (order is null || order.UserId != userId)
        {
            logger.LogWarning("Payment verification stopped because order {OrderId} was not found or not accessible", orderId);
            return null;
        }
        if (order.PaymentStatus == "Paid")
        {
            logger.LogInformation("Payment for order {OrderId} is already recorded as paid", orderId);
            return new("paid", 1, order.TotalAmount, order.Currency, "already_recorded");
        }
        if (string.IsNullOrWhiteSpace(order.PaymentIntentKey))
        {
            logger.LogWarning("Payment verification stopped because order {OrderId} has no provider intent", orderId);
            return new("unknown", null, null, null, "missing_provider_intent");
        }

        var token = await GetAccessTokenAsync(cancellationToken);
        logger.LogInformation("Sending payment verification request to provider for order {OrderId}", orderId);
        var url = $"{settings.Value.ApiBaseUrl.TrimEnd('/')}/getTransactionData";

        using var message = new HttpRequestMessage(HttpMethod.Post, url);
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        message.Content = JsonContent.Create(new GetTransactionRequest(order.PaymentIntentKey), options: JsonOptions);
        using var response = await clients.CreateClient(nameof(FawaterakPaymentService))
            .SendAsync(message, cancellationToken);


        logger.LogInformation("Payment provider verification request for order {OrderId} returned HTTP {StatusCode}",
            orderId, (int)response.StatusCode);
        await EnsureGatewaySuccessAsync(response, "verify payment");

        var result = await response.Content.ReadFromJsonAsync<GetTransactionResponse>(JsonOptions, cancellationToken)
            ?? throw new HttpRequestException("Fawaterak returned an empty payment verification response.");
        if (!string.Equals(result.Status, "success", StringComparison.OrdinalIgnoreCase) || result.Data is null ||
            !string.Equals(result.Data.IntentKey, order.PaymentIntentKey, StringComparison.Ordinal))
            throw new HttpRequestException("Fawaterak could not verify this transaction.");

        var amountMatches = decimal.Round(result.Data.Total, 2) == decimal.Round(order.TotalAmount, 2);
        var currencyMatches = string.Equals(result.Data.Currency, order.Currency, StringComparison.OrdinalIgnoreCase);
        if (result.Data.Paid == 1 )//&& amountMatches && currencyMatches)
        {
            order.PaymentStatus = "Paid";
            order.Status = E_commerce.Data.Enum.OrderStatus.Processing;
            uow.OrderRepository.Edit(order);
            await uow.SaveAsync();
            logger.LogInformation("Payment marked as paid for order {OrderId}", orderId);
            return new("paid", result.Data.Paid, result.Data.Total, result.Data.Currency, null);
        }

        logger.LogWarning("Provider verification did not confirm payment for order {OrderId}; paid flag, currency match, and amount match: {PaidFlag}, {CurrencyMatches}, {AmountMatches}",
            orderId, result.Data.Paid == 1,
            result.Data.Currency.Equals(order.Currency, StringComparison.OrdinalIgnoreCase),
            decimal.Round(result.Data.Total, 2) == decimal.Round(order.TotalAmount, 2));
        var outcome = result.Data.Paid != 1 ? "pending" : !amountMatches ? "amount_mismatch" : !currencyMatches ? "currency_mismatch" : "pending";
        var reason = result.Data.Paid != 1 ? "provider_reports_unpaid" : !amountMatches ? "amount_mismatch" : !currencyMatches ? "currency_mismatch" : null;
        return new(outcome, result.Data.Paid, result.Data.Total, result.Data.Currency, reason);
    }

    private string BuildReturnUrl(int orderId, string result) =>
        $"{settings.Value.FrontendBaseUrl.TrimEnd('/')}/payment/result?orderId={orderId}&result={Uri.EscapeDataString(result)}";

    private IDisposable? BeginPaymentLogScope(string operation, int orderId) => logger.BeginScope(
        new Dictionary<string, object>
        {
            ["PaymentOperation"] = operation,
            ["PaymentOrderId"] = orderId,
            ["TraceId"] = Activity.Current?.TraceId.ToString() ?? "unavailable"
        });

    private static Task EnsureGatewaySuccessAsync(HttpResponseMessage response, string operation)
    {
        if (response.IsSuccessStatusCode)
            return Task.CompletedTask;

        return Task.FromException(new HttpRequestException(
            $"Fawaterak {operation} failed with HTTP {(int)response.StatusCode} ({response.StatusCode}).",
            null, response.StatusCode));
    }

    private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(settings.Value.ClientId) || string.IsNullOrWhiteSpace(settings.Value.ClientSecret))
            throw new InvalidOperationException("Fawaterak OAuth credentials are not configured.");
        if (!string.IsNullOrEmpty(_accessToken) && _tokenExpiresAt > DateTimeOffset.UtcNow.AddSeconds(30))
        {
            logger.LogDebug("Using cached payment provider access token");
            return _accessToken;
        }

        await _tokenLock.WaitAsync(cancellationToken);
        try
        {
            if (!string.IsNullOrEmpty(_accessToken) && _tokenExpiresAt > DateTimeOffset.UtcNow.AddSeconds(30))
            {
                logger.LogDebug("Using cached payment provider access token after waiting for refresh lock");
                return _accessToken;
            }
            logger.LogInformation("Requesting a payment provider OAuth access token");
            using var payload = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = settings.Value.ClientId,
                ["client_secret"] = settings.Value.ClientSecret
            });
            using var response = await clients.CreateClient(nameof(FawaterakPaymentService))
                .PostAsync(settings.Value.TokenUrl, payload, cancellationToken);
            logger.LogInformation("Payment provider OAuth request returned HTTP {StatusCode}", (int)response.StatusCode);
            await EnsureGatewaySuccessAsync(response, "OAuth token request");
            var token = await response.Content.ReadFromJsonAsync<TokenResponse>(JsonOptions, cancellationToken)
                ?? throw new HttpRequestException("Fawaterak returned an empty OAuth response.");
            if (string.IsNullOrWhiteSpace(token.AccessToken))
                throw new HttpRequestException("Fawaterak OAuth response did not include an access token.");
            _accessToken = token.AccessToken;
            _tokenExpiresAt = DateTimeOffset.UtcNow.AddSeconds(Math.Max(token.ExpiresIn, 60));
            logger.LogInformation("Payment provider OAuth token acquired; expires in {ExpiresInSeconds} seconds", token.ExpiresIn);
            return _accessToken;
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    private sealed record TokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("expires_in")] int ExpiresIn);
    private sealed record CreateTransactionRequest([property: JsonPropertyName("currency")]  string Currency, decimal CartTotal, CustomerRequest Customer,
        IReadOnlyList<CartItemRequest> CartItems, RedirectionUrlsRequest RedirectionUrls,
        [property: JsonPropertyName("pay_load")] IReadOnlyDictionary<string, int> PayLoad);
    private sealed record CustomerRequest(
        [property: JsonPropertyName("first_name")] string FirstName,
        [property: JsonPropertyName("last_name")] string LastName,
        string? Email, string? Phone, string? Address);
    private sealed record CartItemRequest(string Name, decimal Price, int Quantity);
    private sealed record RedirectionUrlsRequest(
        [property: JsonPropertyName("success_url")] string SuccessUrl,
        [property: JsonPropertyName("fail_url")] string FailUrl,
        [property: JsonPropertyName("pending_url")] string PendingUrl);
    private sealed record CreateTransactionResponse(string? Status, CreateTransactionData? Data);
    private sealed record CreateTransactionData(
        [property: JsonPropertyName("intent_key")] string? IntentKey,
        string? Url,
        [property: JsonPropertyName("expires_in")] int ExpiresIn);
    private sealed record GetTransactionRequest(
        [property: JsonPropertyName("intent_key")] string IntentKey);
    private sealed record GetTransactionResponse(string? Status, GetTransactionData? Data);
    private sealed record GetTransactionData(
        [property: JsonPropertyName("intent_key")] string? IntentKey,
        int Paid,
        decimal Total,
         [property: JsonPropertyName("currency")] string Currency);
}
