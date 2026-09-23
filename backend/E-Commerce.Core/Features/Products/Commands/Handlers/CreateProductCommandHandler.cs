using E_commerce.Core.Bases;
using E_commerce.Core.Features.Products.Commands.Models;
using E_commerce.Data.Entities;
using E_commerce.Data.Dtos.Products_;
using E_commerce.Service.Abstraction;
using MediatR;

namespace E_commerce.Core.Features.Products.Commands.Handlers;

public sealed class CreateProductCommandHandler(IProductService products, IImageUploadService imageUploads)
    : ResponseHandler, IRequestHandler<CreateProductCommand, Response<ProductResultDto>>
{
    public async Task<Response<ProductResultDto>> Handle(
        CreateProductCommand request, CancellationToken cancellationToken)
    {
        var imagePaths = new List<ImagePath>();
        foreach (var image in request.Images)
        {
            var url = await imageUploads.UploadAsync(image.Content, image.FileName, image.ContentType, cancellationToken);
            imagePaths.Add(new ImagePath { Url = url });
        }

        var product = await products.AddAsync(new Product
        {
            Title = request.Title,
            Description = request.Description,
            Price = request.Price,
            CategoryId = request.CategoryId,
            ImagePaths = imagePaths,
            Rating = request.Rating,
            RatingCount = request.RatingCount,
            StockQuantity = request.StockQuantity
        }, cancellationToken);

        return Created(ProductResultMapper.Map(product));
    }
}
