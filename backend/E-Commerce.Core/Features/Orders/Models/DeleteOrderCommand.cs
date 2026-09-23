using E_commerce.Core.Bases;
using MediatR;

namespace E_commerce.Core.Features.Orders.Models;

public sealed record DeleteOrderCommand(int OrderId, string UserId, bool IsAdmin) : IRequest<Response<bool>>;
