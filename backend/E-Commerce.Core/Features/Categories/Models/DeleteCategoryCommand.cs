using E_commerce.Core.Bases;
using MediatR;

namespace E_commerce.Core.Features.Categories.Models;

public sealed record DeleteCategoryCommand(int Id) : IRequest<Response<bool>>;
