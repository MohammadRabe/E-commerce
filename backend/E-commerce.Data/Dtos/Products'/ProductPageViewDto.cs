namespace E_commerce.Data.Dtos.Products_;

/// <summary>Product detail fields returned to catalog clients.</summary>
public sealed class ProductPageViewDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? Discount { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public decimal Rating { get; set; }
    public int RatingCount { get; set; }
    public int StockQuantity { get; set; }
}
