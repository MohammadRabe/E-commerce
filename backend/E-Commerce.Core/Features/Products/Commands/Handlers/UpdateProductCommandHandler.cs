using E_commerce.Core.Bases;
using E_commerce.Core.Features.Products.Commands.Models;
using E_commerce.Data.Entities;
using E_commerce.Data.Dtos.Products_;
using E_commerce.Service.Abstraction;
using MediatR;

namespace E_commerce.Core.Features.Products.Commands.Handlers;

public sealed class UpdateProductCommandHandler(IProductService products, IImageUploadService imageUploads)
    : ResponseHandler, IRequestHandler<UpdateProductCommand, Response<ProductResultDto>>
{
    public async Task<Response<ProductResultDto>> Handle(
        UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var existing = await products.GetByIdAsync(request.Id, cancellationToken, product => product.ImagePaths);
        if (existing is null)
        {
            return NotFound<ProductResultDto>(errors: [$"Product {request.Id} was not found."]);
        }

        var imagePaths = request.Images.Count == 0
            ? existing.ImagePaths.Select(image => new ImagePath { Url = image.Url, ProductId = request.Id }).ToList()
            : new List<ImagePath>();
        foreach (var image in request.Images)
        {
            var url = await imageUploads.UploadAsync(image.Content, image.FileName, image.ContentType, cancellationToken);
            imagePaths.Add(new ImagePath { Url = url, ProductId = request.Id });
        }

        var product = await products.UpdateAsync(new Product
        {
            Id = request.Id,
            Title = request.Title,
            Description = request.Description,
            Price = request.Price,
            CategoryId = request.CategoryId,
            ImagePaths = imagePaths,
            Rating = request.Rating,
            RatingCount = request.RatingCount,
            StockQuantity = request.StockQuantity
        }, cancellationToken);
        return Success(ProductResultMapper.Map(product), "Product updated successfully.");
    }
}
