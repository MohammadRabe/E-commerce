namespace E_commerce.Data.Dtos.Products_;

/// <summary>Product fields safe to return from create, update, and detail endpoints.</summary>
public sealed record ProductResultDto(
    string Title,
    string Description,
    decimal Price,
    decimal? Discount,
    int CategoryId,
    string? CategoryName,
    decimal Rating,
    int RatingCount,
    int StockQuantity);
