using E_commerce.Core.Bases;
using E_commerce.Core.Features.Orders.Models;
using E_commerce.Data.Dtos.Orders;
using E_commerce.Service.Abstraction;
using MediatR;

namespace E_commerce.Core.Features.Orders.Handlers;

public sealed class GetOrdersByStatusHandler(IOrderService orders)
    : ResponseHandler, IRequestHandler<GetOrdersByStatusQuery, Response<IReadOnlyList<OrderDetailsDto>>>
{
    public async Task<Response<IReadOnlyList<OrderDetailsDto>>> Handle(
        GetOrdersByStatusQuery request,
        CancellationToken cancellationToken)
    {
        var entities = await orders.GetByStatusAsync(request.Status, cancellationToken, order => order.Items);
        IReadOnlyList<OrderDetailsDto> results = entities.Select(OrderDtoMapper.Map).ToList();
        return Success(results);
    }
}
