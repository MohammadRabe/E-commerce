using E_commerce.Core.Bases;
using MediatR;

namespace E_commerce.Core.Features.Products.Commands.Models;

public sealed record DeleteProductCommand(int Id) : IRequest<Response<bool>>;
