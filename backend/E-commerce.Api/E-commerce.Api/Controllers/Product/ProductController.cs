using CleanArch.Api.Controllers.Base;
using CleanArch.Data.Routing;
using E_commerce.Core.Features.Products.Commands.Models;
using E_commerce.Core.Features.Products.Queries.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace E_commerce.Api.Controllers.Product
{
    [ApiController]
    [Authorize(Roles = "Admin")]

    public class ProductController : AppControllerBase
    {
        public ProductController(IMediator mediator) : base(mediator)
        {
        }

        [AllowAnonymous]
        [HttpGet(Router.Version1.Product.GetPagedProducts)]
        public async Task<IActionResult> GetPageProducts(int pageNumber,int PageSize)
        {
            return NewResult(await _mediator.Send(new GetPagedProductsQuery(pageNumber,PageSize)));
        }

        [AllowAnonymous]
        [HttpGet(Router.Version1.Product.GetProductById)]
        public async Task<IActionResult> GetProductById(int id)
        {
            return NewResult(await _mediator.Send(new GetProductByIdQuery(id)));
        }
        [AllowAnonymous]

        [HttpPost(Router.Version1.Product.CreateProduct)]
        public async Task<IActionResult> CreateProduct([FromForm] ProductForm request, CancellationToken cancellationToken)
        {
            if (request.Images.Count is < 1 or > 7)
                return BadRequest(new { message = "A product must have between 1 and 7 images." });
            var streams = request.Images.Select(image => image.OpenReadStream()).ToList();
            try
            {
                var images = request.Images.Select((image, index) => new ProductImageUpload(
                    streams[index], image.FileName, image.ContentType)).ToList();
                var command = new CreateProductCommand(request.Title, request.Description, request.Price,
                    request.CategoryId, images, request.Rating, request.RatingCount, request.StockQuantity);
                return NewResult(await _mediator.Send(command, cancellationToken));
            }
            finally { foreach (var stream in streams) await stream.DisposeAsync(); }
        }

        [HttpPut(Router.Version1.Product.UpdateProduct)]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] ProductForm request, CancellationToken cancellationToken)
        {
            if (request.Images.Count > 7)
                return BadRequest(new { message = "A product cannot have more than 7 images." });
            var streams = request.Images.Select(image => image.OpenReadStream()).ToList();
            try
            {
                var images = request.Images.Select((image, index) => new ProductImageUpload(
                    streams[index], image.FileName, image.ContentType)).ToList();
                var command = new UpdateProductCommand(id, request.Title, request.Description, request.Price,
                    request.CategoryId, images, request.Rating, request.RatingCount, request.StockQuantity);
                return NewResult(await _mediator.Send(command, cancellationToken));
            }
            finally { foreach (var stream in streams) await stream.DisposeAsync(); }
        }

        [HttpDelete(Router.Version1.Product.DeleteProduct)]
        public async Task<IActionResult> DeleteProduct(int id, CancellationToken cancellationToken) =>
            NewResult(await _mediator.Send(new DeleteProductCommand(id), cancellationToken));

        public sealed class ProductForm
        {
            public string Title { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public int CategoryId { get; set; }
            [FromForm(Name = "Images")]
            public List<IFormFile> Images { get; set; } = new();
            public decimal Rating { get; set; }
            public int RatingCount { get; set; }
            public int StockQuantity { get; set; }
        }
    }
}
