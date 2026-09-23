using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_commerce.Data.Entities;

public class Product
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(4000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(18, 2)")]
    [Range(typeof(decimal), "0.01", "9999999999999999.99")]
    public decimal Price { get; set; }

    [Range(typeof(decimal), "0.01", "99.99")]
    [Column(TypeName = "decimal(4, 2)")]

    public decimal? Discount { get; set; }

    [Required]
    public int CategoryId { get; set; }

    public Category Category { get; set; } = null!;

    public virtual ICollection<ImagePath> ImagePaths { get; set; } = new List<ImagePath>();

    [Range(0, 5)]
    [Column(TypeName = "decimal(3, 2)")]
    public decimal Rating { get; set; }

    [Range(0, int.MaxValue)]
    public int RatingCount { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }
}
