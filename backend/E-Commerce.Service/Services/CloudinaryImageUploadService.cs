using E_commerce.Data.Options;
using E_commerce.Service.Abstraction;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace E_commerce.Service.Services;

public sealed class CloudinaryImageUploadService(HttpClient httpClient, IOptions<CloudinarySettings> options)
    : IImageUploadService
{
    private readonly CloudinarySettings _settings = options.Value;

    public async Task<string> UploadAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.CloudName) || string.IsNullOrWhiteSpace(_settings.ApiKey) || string.IsNullOrWhiteSpace(_settings.ApiSecret))
            throw new InvalidOperationException("Cloudinary CloudName, ApiKey, and ApiSecret must be configured.");

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);
        var signature = Sign($"timestamp={timestamp}", _settings.ApiSecret);
        using var form = new MultipartFormDataContent();
        var fileContent = new StreamContent(content);
        if (System.Net.Http.Headers.MediaTypeHeaderValue.TryParse(contentType, out var mediaType))
            fileContent.Headers.ContentType = mediaType;
        form.Add(fileContent, "file", Path.GetFileName(fileName));
        form.Add(new StringContent(_settings.ApiKey), "api_key");
        form.Add(new StringContent(timestamp), "timestamp");
        form.Add(new StringContent(signature), "signature");

        using var response = await httpClient.PostAsync(
            $"https://api.cloudinary.com/v1_1/{Uri.EscapeDataString(_settings.CloudName)}/image/upload",
            form, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Cloudinary upload failed ({(int)response.StatusCode}): {body}");
        using var json = JsonDocument.Parse(body);
        return json.RootElement.GetProperty("secure_url").GetString()
            ?? throw new InvalidOperationException("Cloudinary upload response did not contain secure_url.");
    }

    private static string Sign(string parameter, string secret) =>
        Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes(parameter + secret))).ToLowerInvariant();
}
