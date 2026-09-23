using E_commerce.Core.Bases;
using E_commerce.Core.Features.Categories.Models;
using E_commerce.Data.Dtos.Categories;
using E_commerce.Service.Abstraction;
using MediatR;

namespace E_commerce.Core.Features.Categories.Handlers;

public sealed class GetCategoriesHandler(ICategoryService categories)
    : ResponseHandler, IRequestHandler<GetCategoriesQuery, Response<IReadOnlyList<CategoryDto>>>
{
    public async Task<Response<IReadOnlyList<CategoryDto>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken) =>
        Success<IReadOnlyList<CategoryDto>>((await categories.GetAllAsync(cancellationToken)).Select(CategoryDtoMapper.Map).ToList());
}
