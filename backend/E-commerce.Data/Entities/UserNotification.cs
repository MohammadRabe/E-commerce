using System.ComponentModel.DataAnnotations;

namespace E_commerce.Data.Entities;

public sealed class UserNotification
{
    public int Id { get; set; }

    [Required]
    public string RecipientUserId { get; set; } = string.Empty;

    public User RecipientUser { get; set; } = null!;

    public int? OrderId { get; set; }

    [Required, MaxLength(50)]
    public string Type { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(1000)]
    public string Message { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string Link { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public bool IsRead { get; set; }
}
