using E_commerce.Core.Bases;
using E_commerce.Core.Features.Products.Commands.Models;
using E_commerce.Service.Abstraction;
using MediatR;

namespace E_commerce.Core.Features.Products.Commands.Handlers;

public sealed class DeleteProductCommandHandler(IProductService products)
    : ResponseHandler, IRequestHandler<DeleteProductCommand, Response<bool>>
{
    public async Task<Response<bool>> Handle(
        DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var deleted = await products.DeleteAsync(request.Id, cancellationToken);
        return deleted
            ? Deleted(true, "Product deleted successfully.")
            : NotFound<bool>(errors: [$"Product {request.Id} was not found or could not be deleted."]);
    }
}
