using E_commerce.Data.Dtos.Products_;
using E_commerce.Data.Entities;

namespace E_commerce.Core.Features.Products.Commands.Handlers;

internal static class ProductResultMapper
{
    public static ProductResultDto Map(Product product) => new(
        product.Title, product.Description, product.Price, product.Discount,
        product.CategoryId, product.Category?.Name,
        product.Rating, product.RatingCount, product.StockQuantity);
}
