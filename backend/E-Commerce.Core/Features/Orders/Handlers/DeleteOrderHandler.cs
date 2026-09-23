using E_commerce.Core.Bases;
using E_commerce.Core.Features.Orders.Models;
using E_commerce.Data.Entities;
using E_commerce.Service.Abstraction;
using MediatR;

namespace E_commerce.Core.Features.Orders.Handlers;

public sealed class DeleteOrderHandler(IOrderService orders)
    : ResponseHandler, IRequestHandler<DeleteOrderCommand, Response<bool>>
{
    public async Task<Response<bool>> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null) return NotFound<bool>();
        if (!request.IsAdmin && (order.UserId != request.UserId || order.Status != E_commerce.Data.Enum.OrderStatus.Pending))
            return Forbidden<bool>();
        return await orders.DeleteAsync(request.OrderId, cancellationToken) ? Deleted(true) : NotFound<bool>();
    }
}
