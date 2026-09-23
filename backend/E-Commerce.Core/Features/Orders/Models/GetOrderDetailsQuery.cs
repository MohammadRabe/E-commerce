using E_commerce.Core.Bases;
using E_commerce.Data.Dtos.Orders;
using MediatR;

namespace E_commerce.Core.Features.Orders.Models;

public sealed record GetOrderDetailsQuery(int OrderId, string UserId, bool IsAdmin) : IRequest<Response<OrderDetailsDto>>;
