using E_commerce.Core.Bases;
using E_commerce.Core.Features.Categories.Models;
using E_commerce.Data.Dtos.Categories;
using E_commerce.Data.Entities;
using E_commerce.Service.Abstraction;
using MediatR;

namespace E_commerce.Core.Features.Categories.Handlers;

public sealed class UpdateCategoryHandler(ICategoryService categories)
    : ResponseHandler, IRequestHandler<UpdateCategoryCommand, Response<CategoryDto>>
{
    public async Task<Response<CategoryDto>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        try { return Success(CategoryDtoMapper.Map(await categories.UpdateAsync(new Category { Id = request.Id, Name = request.Name.Trim() }, cancellationToken))); }
        catch (KeyNotFoundException) { return NotFound<CategoryDto>(); }
    }
}
