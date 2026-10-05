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
using Microsoft.Extensions.Options;
using System.Net;

namespace E_commerce.Service.Services;

public sealed class FawaterakPaymentService(
    IUOW uow,
    IHttpClientFactory clients,
    IOptions<FawaterakSettings> settings) : IPaymentService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly SemaphoreSlim _tokenLock = new(1, 1);
    private string? _accessToken;
    private DateTimeOffset _tokenExpiresAt;

    public async Task<FawaterakTransactionDataResultModel?> CreatePaymentAsync(
        int orderId, string userId, CancellationToken cancellationToken = default)
    {
        var order = await uow.OrderRepository.GetForPaymentAsync(orderId, cancellationToken);
        if (order is null || order.UserId != userId)
            return null;
        if (order.PaymentStatus == "Paid")
            throw new PaymentRuleException("هذا الطلب مدفوع بالفعل. يمكنك مراجعة حالته من صفحة طلباتك.", HttpStatusCode.Conflict);
        if (order.Status == E_commerce.Data.Enum.OrderStatus.Cancelled)
            throw new PaymentRuleException("لا يمكن دفع طلب ملغي. أنشئ طلبًا جديدًا لإتمام الشراء.", HttpStatusCode.Conflict);
        if (!string.IsNullOrWhiteSpace(order.PaymentIntentKey) && !string.IsNullOrWhiteSpace(order.PaymentUrl))
            return new(order.Id, order.PaymentIntentKey, order.PaymentUrl, 0);
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

        var token = await GetAccessTokenAsync(cancellationToken);
        using var message = new HttpRequestMessage(HttpMethod.Post,
            new Uri(new Uri(settings.Value.ApiBaseUrl.TrimEnd('/') + "/"), "api/v3/createTransaction"));
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        message.Content = JsonContent.Create(request, options: JsonOptions);
        using var response = await clients.CreateClient(nameof(FawaterakPaymentService))
            .SendAsync(message, cancellationToken);
        await EnsureGatewaySuccessAsync(response, "create checkout", cancellationToken);
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
        return new(order.Id, result.Data.IntentKey, result.Data.Url, result.Data.ExpiresIn);
    }

    public async Task<bool?> VerifyPaymentAsync(int orderId, string userId, CancellationToken cancellationToken = default)
    {
        var order = await uow.OrderRepository.GetForPaymentAsync(orderId, cancellationToken);
        if (order is null || order.UserId != userId)
            return null;
        if (order.PaymentStatus == "Paid")
            return true;
        if (string.IsNullOrWhiteSpace(order.PaymentIntentKey))
            return false;

        var token = await GetAccessTokenAsync(cancellationToken);
        using var message = new HttpRequestMessage(HttpMethod.Post,
            new Uri(new Uri(settings.Value.ApiBaseUrl.TrimEnd('/') + "/"), "api/v3/getTransactionData"));
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        message.Content = JsonContent.Create(new GetTransactionRequest(order.PaymentIntentKey), options: JsonOptions);
        using var response = await clients.CreateClient(nameof(FawaterakPaymentService))
            .SendAsync(message, cancellationToken);
        await EnsureGatewaySuccessAsync(response, "verify payment", cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<GetTransactionResponse>(JsonOptions, cancellationToken)
            ?? throw new HttpRequestException("Fawaterak returned an empty payment verification response.");
        if (!string.Equals(result.Status, "success", StringComparison.OrdinalIgnoreCase) || result.Data is null ||
            !string.Equals(result.Data.IntentKey, order.PaymentIntentKey, StringComparison.Ordinal))
            throw new HttpRequestException("Fawaterak could not verify this transaction.");

        if (result.Data.Paid == 1 &&
            result.Data.Currency.Equals(order.Currency, StringComparison.OrdinalIgnoreCase) &&
            decimal.Round(result.Data.Total, 2) == decimal.Round(order.TotalAmount, 2))
        {
            order.PaymentStatus = "Paid";
            order.Status = E_commerce.Data.Enum.OrderStatus.Processing;
            uow.OrderRepository.Edit(order);
            await uow.SaveAsync();
            return true;
        }

        return false;
    }

    private string BuildReturnUrl(int orderId, string result) =>
        $"{settings.Value.FrontendBaseUrl.TrimEnd('/')}/payment/result?orderId={orderId}&result={Uri.EscapeDataString(result)}";

    private static async Task EnsureGatewaySuccessAsync(
        HttpResponseMessage response, string operation, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
            return;

        // Keep the diagnostic bounded and avoid returning provider responses containing secrets.
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        body = body.Length > 500 ? body[..500] : body;
        throw new HttpRequestException(
            $"Fawaterak {operation} failed with HTTP {(int)response.StatusCode} ({response.StatusCode}). " +
            (string.IsNullOrWhiteSpace(body) ? "The provider returned no details." : $"Provider response: {body}"),
            null, response.StatusCode);
    }

    private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(settings.Value.ClientId) || string.IsNullOrWhiteSpace(settings.Value.ClientSecret))
            throw new InvalidOperationException("Fawaterak OAuth credentials are not configured.");
        if (!string.IsNullOrEmpty(_accessToken) && _tokenExpiresAt > DateTimeOffset.UtcNow.AddSeconds(30))
            return _accessToken;

        await _tokenLock.WaitAsync(cancellationToken);
        try
        {
            if (!string.IsNullOrEmpty(_accessToken) && _tokenExpiresAt > DateTimeOffset.UtcNow.AddSeconds(30))
                return _accessToken;
            using var payload = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = settings.Value.ClientId,
                ["client_secret"] = settings.Value.ClientSecret
            });
            using var response = await clients.CreateClient(nameof(FawaterakPaymentService))
                .PostAsync(settings.Value.TokenUrl, payload, cancellationToken);
            await EnsureGatewaySuccessAsync(response, "OAuth token request", cancellationToken);
            var token = await response.Content.ReadFromJsonAsync<TokenResponse>(JsonOptions, cancellationToken)
                ?? throw new HttpRequestException("Fawaterak returned an empty OAuth response.");
            if (string.IsNullOrWhiteSpace(token.AccessToken))
                throw new HttpRequestException("Fawaterak OAuth response did not include an access token.");
            _accessToken = token.AccessToken;
            _tokenExpiresAt = DateTimeOffset.UtcNow.AddSeconds(Math.Max(token.ExpiresIn, 60));
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
    private sealed record CreateTransactionRequest(string Currency, decimal CartTotal, CustomerRequest Customer,
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
        string Currency);
}
