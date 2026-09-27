using E_commerce.Data.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_commerce.Data.Entities;

/// <summary>
/// A customer's order and the shipping details captured when it was placed.
/// </summary>
public class Order
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public User User { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = "SAR";

    [MaxLength(100)]
    public string? PaymentIntentKey { get; set; }

    [MaxLength(20)]
    public string PaymentStatus { get; set; } = "Unpaid";

    [MaxLength(1000)]
    public string? PaymentUrl { get; set; }

    [Required]
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    [Required]
    [MaxLength(500)]
    public string ShippingAddress { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string ShippingPhoneNumber { get; set; } = string.Empty;

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

    [NotMapped]
    public decimal TotalAmount => Items.Sum(item => item.UnitPrice * item.Quantity);
}
