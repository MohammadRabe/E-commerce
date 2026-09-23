using E_commerce.Core.Bases;
using E_commerce.Core.Features.Categories.Models;
using E_commerce.Data.Dtos.Categories;
using E_commerce.Data.Entities;
using E_commerce.Service.Abstraction;
using MediatR;

namespace E_commerce.Core.Features.Categories.Handlers;

public sealed class CreateCategoryHandler(ICategoryService categories)
    : ResponseHandler, IRequestHandler<CreateCategoryCommand, Response<CategoryDto>>
{
    public async Task<Response<CategoryDto>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken) =>
        Created(CategoryDtoMapper.Map(await categories.AddAsync(new Category { Name = request.Name.Trim() }, cancellationToken)));
}
