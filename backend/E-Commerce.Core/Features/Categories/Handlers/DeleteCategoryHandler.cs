using E_commerce.Core.Bases;
using E_commerce.Core.Features.Categories.Models;
using E_commerce.Service.Abstraction;
using MediatR;

namespace E_commerce.Core.Features.Categories.Handlers;

public sealed class DeleteCategoryHandler(ICategoryService categories)
    : ResponseHandler, IRequestHandler<DeleteCategoryCommand, Response<bool>>
{
    public async Task<Response<bool>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var deleted = await categories.DeleteAsync(request.Id, cancellationToken);
        return deleted ? Deleted(true) : NotFound<bool>();
    }
}
