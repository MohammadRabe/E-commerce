using E_commerce.Core.Bases;
using E_commerce.Data.Enum;
using MediatR;

namespace E_commerce.Core.Features.Orders.Models;

public sealed record UpdateOrderStatusCommand(int OrderId, string UserId, bool IsAdmin, OrderStatus Status)
    : IRequest<Response<bool>>;
