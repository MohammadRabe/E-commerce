using System.ComponentModel.DataAnnotations;

namespace E_commerce.Data.Entities;

public sealed class UserLike
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    public User User { get; set; } = null!;

    public int ProductId { get; set; }

    public Product Product { get; set; } = null!;
}
