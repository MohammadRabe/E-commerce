using E_commerce.Core.Bases;
using E_commerce.Core.Features.Orders.Models;
using E_commerce.Data.Dtos.Orders;
using E_commerce.Service.Abstraction;
using MediatR;

namespace E_commerce.Core.Features.Orders.Handlers;

public sealed class GetOrderDetailsHandler(IOrderService orders)
    : ResponseHandler, IRequestHandler<GetOrderDetailsQuery, Response<OrderDetailsDto>>
{
    public async Task<Response<OrderDetailsDto>> Handle(GetOrderDetailsQuery request, CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdAsync(request.OrderId, cancellationToken, entity => entity.Items);
        if (order is null) return NotFound<OrderDetailsDto>();
        if (!request.IsAdmin && order.UserId != request.UserId) return Forbidden<OrderDetailsDto>();
        return Success(OrderDtoMapper.Map(order));
    }
}
