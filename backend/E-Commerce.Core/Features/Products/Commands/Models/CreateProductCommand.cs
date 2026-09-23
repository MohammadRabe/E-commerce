using E_commerce.Core.Bases;
using E_commerce.Data.Dtos.Products_;
using MediatR;

namespace E_commerce.Core.Features.Products.Commands.Models;

public sealed record CreateProductCommand(
    string Title,
    string Description,
    decimal Price,
    int CategoryId,
    IReadOnlyList<ProductImageUpload> Images,
    decimal Rating,
    int RatingCount,
    int StockQuantity) : IRequest<Response<ProductResultDto>>;
