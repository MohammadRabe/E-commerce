using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_commerce.Data.Entities;

/// <summary>
/// A product and its purchase-time price and quantity within an order.
/// </summary>
public class OrderItem
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int OrderId { get; set; }

    public Order Order { get; set; } = null!;

    [Required]
    public int ProductId { get; set; }

    public Product Product { get; set; } = null!;

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    [Range(typeof(decimal), "0.01", "9999999999999999.99")]
    public decimal UnitPrice { get; set; }
}
