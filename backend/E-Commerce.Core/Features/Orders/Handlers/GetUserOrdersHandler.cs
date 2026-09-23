using E_commerce.Core.Bases;
using E_commerce.Core.Features.Orders.Models;
using E_commerce.Data.Dtos.Orders;
using E_commerce.Service.Abstraction;
using MediatR;

namespace E_commerce.Core.Features.Orders.Handlers;

public sealed class GetUserOrdersHandler(IOrderService orders)
    : ResponseHandler, IRequestHandler<GetUserOrdersQuery, Response<IReadOnlyList<OrderDetailsDto>>>
{
    public async Task<Response<IReadOnlyList<OrderDetailsDto>>> Handle(GetUserOrdersQuery request, CancellationToken cancellationToken)
    {
        var entities = await orders.GetByUserIdAsync(request.UserId, cancellationToken, order => order.Items);
        IReadOnlyList<OrderDetailsDto> results = entities.Select(OrderDtoMapper.Map).ToList();
        return Success(results);
    }
}
