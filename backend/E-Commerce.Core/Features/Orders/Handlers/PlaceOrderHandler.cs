using E_commerce.Core.Bases;
using E_commerce.Core.Features.Orders.Models;
using E_commerce.Data.Dtos.Orders;
using E_commerce.Service.Abstraction;
using MediatR;
using Microsoft.Extensions.Logging;

namespace E_commerce.Core.Features.Orders.Handlers;

public sealed class PlaceOrderHandler(
    IOrderService orders,
    IOrderNotificationService notifications,
    ILogger<PlaceOrderHandler> logger)
    : ResponseHandler, IRequestHandler<PlaceOrderCommand, Response<OrderDetailsDto>>
{
    public async Task<Response<OrderDetailsDto>> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await orders.PlaceOrderAsync(request.UserId, request.Order, cancellationToken);
            await notifications.NotifyNewOrderAsync(order.Id, order.TotalAmount, cancellationToken);
            logger.LogInformation("Created order {OrderId} for user {UserId} with total {TotalAmount}",
                order.Id, request.UserId, order.TotalAmount);
            return Created(OrderDtoMapper.Map(order));
        }
        catch (KeyNotFoundException exception)
        {
            logger.LogWarning(exception, "Order creation rejected because a requested record was missing for user {UserId}", request.UserId);
            return BadRequest<OrderDetailsDto>(errors: [exception.Message]);
        }
        catch (InvalidOperationException exception)
        {
            logger.LogWarning(exception, "Order creation rejected for user {UserId}", request.UserId);
            return BadRequest<OrderDetailsDto>(errors: [exception.Message]);
        }
    }
}
