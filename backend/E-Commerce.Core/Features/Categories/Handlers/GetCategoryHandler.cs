using E_commerce.Core.Bases;
using E_commerce.Core.Features.Categories.Models;
using E_commerce.Data.Dtos.Categories;
using E_commerce.Service.Abstraction;
using MediatR;

namespace E_commerce.Core.Features.Categories.Handlers;

public sealed class GetCategoryHandler(ICategoryService categories)
    : ResponseHandler, IRequestHandler<GetCategoryQuery, Response<CategoryDto>>
{
    public async Task<Response<CategoryDto>> Handle(GetCategoryQuery request, CancellationToken cancellationToken)
    {
        var category = await categories.GetByIdAsync(request.Id, cancellationToken);
        return category is null ? NotFound<CategoryDto>() : Success(CategoryDtoMapper.Map(category));
    }
}
