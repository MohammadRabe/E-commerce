using E_commerce.Core.Bases;
using E_commerce.Data.Dtos.Orders;
using E_commerce.Data.Enum;
using MediatR;

namespace E_commerce.Core.Features.Orders.Models;

public sealed record GetOrdersByStatusQuery(OrderStatus Status)
    : IRequest<Response<IReadOnlyList<OrderDetailsDto>>>;
