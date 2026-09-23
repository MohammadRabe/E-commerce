namespace E_commerce.Data.Dtos.Products_;

/// <summary>Compact product data required to render a catalog card.</summary>
public sealed record ProductListDto(
    int Id,
    string Title,
    decimal Price,
    decimal? Discount,
    string CategoryName,
    decimal Rating,
    string? ImageUrl);
