namespace E_commerce.Data.Options;

using System.ComponentModel.DataAnnotations;

public sealed class CloudinarySettings
{
    [Required]
    public string CloudName { get; set; } = string.Empty;

    [Required]
    public string ApiKey { get; set; } = string.Empty;

    [Required]
    public string ApiSecret { get; set; } = string.Empty;
}
