using E_commerce.Core.Bases;
using E_commerce.Core.Features.Orders.Models;
using E_commerce.Data.Dtos.Orders;
using E_commerce.Service.Abstraction;
using MediatR;

namespace E_commerce.Core.Features.Orders.Handlers;

public sealed class PlaceOrderHandler(IOrderService orders)
    : ResponseHandler, IRequestHandler<PlaceOrderCommand, Response<OrderDetailsDto>>
{
    public async Task<Response<OrderDetailsDto>> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await orders.PlaceOrderAsync(request.UserId, request.Order, cancellationToken);
            return Created(OrderDtoMapper.Map(order));
        }
        catch (KeyNotFoundException exception) { return BadRequest<OrderDetailsDto>(errors: [exception.Message]); }
        catch (InvalidOperationException exception) { return BadRequest<OrderDetailsDto>(errors: [exception.Message]); }
    }
}
