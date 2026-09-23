using E_commerce.Core.Bases;
using E_commerce.Core.Features.Products.Queries.Models;
using E_commerce.Data.Dtos.Products_;
using E_commerce.Data.Entities;
using E_commerce.Service.Abstraction;
using MediatR;

namespace E_commerce.Core.Features.Products.Queries.Handlers;

public sealed class GetProductByIdQueryHandler(IProductService products)
    : ResponseHandler, IRequestHandler<GetProductByIdQuery, Response<ProductPageViewDto>>
{
    public async Task<Response<ProductPageViewDto>> Handle(
        GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await products.GetByIdAsync(request.Id, cancellationToken, p => p.Category, p => p.ImagePaths);
        if (entity == null)
            return BadRequest<ProductPageViewDto>(null, new[] { "product doesn't exist"});

        return Success(new ProductPageViewDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            Price = entity.Price,
            Discount = entity.Discount,
            CategoryId = entity.CategoryId,
            CategoryName = entity.Category.Name,
            ImageUrl = entity.ImagePaths.OrderBy(image => image.Id).Select(image => image.Url).FirstOrDefault(),
            Rating = entity.Rating,
            RatingCount = entity.RatingCount,
            StockQuantity = entity.StockQuantity
        });

    }
}
