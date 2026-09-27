using E_commerce.Core.Bases;
using E_commerce.Core.Features.Orders.Models;
using E_commerce.Service.Abstraction;
using MediatR;

namespace E_commerce.Core.Features.Orders.Handlers;

public sealed class UpdateOrderStatusHandler(IOrderService orders, IOrderNotificationService notifications)
    : ResponseHandler, IRequestHandler<UpdateOrderStatusCommand, Response<bool>>
{
    public async Task<Response<bool>> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null) return NotFound<bool>();
        if (!request.IsAdmin && (order.UserId != request.UserId || request.Status != E_commerce.Data.Enum.OrderStatus.Cancelled
            || order.Status != E_commerce.Data.Enum.OrderStatus.Pending))
            return Forbidden<bool>();
        if (order.Status == E_commerce.Data.Enum.OrderStatus.Cancelled && request.Status != E_commerce.Data.Enum.OrderStatus.Cancelled)
            return BadRequest<bool>(errors: ["A cancelled order cannot be reopened."]);
        if (order.Status == request.Status) return Success(true, "Order status is already up to date.");
        await orders.UpdateStatusAsync(order.Id, request.Status, cancellationToken);
        if (request.IsAdmin)
            await notifications.NotifyOrderStatusChangedAsync(order.Id, order.UserId, request.Status, cancellationToken);
        return Success(true, "Order status updated.");
    }
}
