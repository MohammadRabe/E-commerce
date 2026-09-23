using E_commerce.Core.Bases;
using E_commerce.Data.Dtos.Categories;
using MediatR;

namespace E_commerce.Core.Features.Categories.Models;

public sealed record UpdateCategoryCommand(int Id, string Name) : IRequest<Response<CategoryDto>>;
