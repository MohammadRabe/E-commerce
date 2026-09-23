using CleanArch.Api.Controllers.Base;
using CleanArch.Data.Routing;
using E_commerce.Core.Features.Orders.Models;
using E_commerce.Data.Dtos.Orders;
using E_commerce.Data.Enum;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace E_commerce.Api.Controllers.Order;

[ApiController]
[Authorize]
public sealed class OrderController(IMediator mediator) : AppControllerBase(mediator)
{
    public sealed record UpdateOrderStatusRequest(OrderStatus Status);

    [HttpGet(Router.Version1.Order.GetMyOrders)]
    public async Task<IActionResult> GetMyOrders(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return NewResult(await _mediator.Send(new GetUserOrdersQuery(userId), cancellationToken));
    }

    [HttpGet(Router.Version1.Order.GetDetails)]
    public async Task<IActionResult> GetDetails(int orderId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return NewResult(await _mediator.Send(new GetOrderDetailsQuery(orderId, userId, User.IsInRole("Admin")), cancellationToken));
    }

    [HttpPost(Router.Version1.Order.Create)]
    public async Task<IActionResult> Create(CreateOrderDto request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return NewResult(await _mediator.Send(new PlaceOrderCommand(userId, request), cancellationToken));
    }

    [HttpDelete(Router.Version1.Order.Delete)]
    public async Task<IActionResult> Delete(int orderId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return NewResult(await _mediator.Send(new DeleteOrderCommand(orderId, userId, User.IsInRole("Admin")), cancellationToken));
    }

    [HttpPatch(Router.Version1.Order.UpdateStatus)]
    public async Task<IActionResult> UpdateStatus(int orderId, UpdateOrderStatusRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return NewResult(await _mediator.Send(new UpdateOrderStatusCommand(orderId, userId, User.IsInRole("Admin"), request.Status), cancellationToken));
    }

    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
}
