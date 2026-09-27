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
    public async Task<Response<CategoryDto>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        var existing = await categories.GetAllAsync(cancellationToken);
        if (existing.Any(category => string.Equals(category.Name.Trim(), name, StringComparison.OrdinalIgnoreCase)))
        {
            return Conflict<CategoryDto>(errors: ["A category with this name already exists."], message: "Category already exists.");
        }

        var created = await categories.AddAsync(new Category { Name = name }, cancellationToken);
        return Created(CategoryDtoMapper.Map(created));
    }
}
