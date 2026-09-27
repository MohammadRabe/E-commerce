namespace E_commerce.Data.Options;

public sealed class FawaterakSettings
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string TokenUrl { get; set; } = "https://app.fawaterk.com/oauth/token";
    public string ApiBaseUrl { get; set; } = "https://app.fawaterk.com";
    public string FrontendBaseUrl { get; set; } = "http://localhost:5173";
}
