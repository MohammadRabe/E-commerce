using E_commerce.Data.Dtos.Categories;
using E_commerce.Data.Entities;

namespace E_commerce.Core.Features.Categories.Handlers;

internal static class CategoryDtoMapper
{
    public static CategoryDto Map(Category category) => new(category.Id, category.Name);
}
