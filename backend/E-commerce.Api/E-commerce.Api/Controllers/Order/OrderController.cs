using CleanArch.Api.Controllers.Base;
using CleanArch.Data.Routing;
using E_commerce.Core.Features.Orders.Models;
using E_commerce.Data.Dtos.Orders;
using E_commerce.Data.Enum;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using E_commerce.Service.Abstraction;

namespace E_commerce.Api.Controllers.Order;

[ApiController]
public sealed class OrderController(IMediator mediator, IPaymentService payments) : AppControllerBase(mediator)
{
    public sealed record UpdateOrderStatusRequest(OrderStatus Status);

    [HttpGet(Router.Version1.Order.GetMyOrders)]
    public async Task<IActionResult> GetMyOrders(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) 
            return Unauthorized();
        return NewResult(await _mediator.Send(new GetUserOrdersQuery(userId), cancellationToken));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet(Router.Version1.Order.GetByStatus)]
    public async Task<IActionResult> GetByStatus([FromQuery] OrderStatus status, CancellationToken cancellationToken) =>
        NewResult(await _mediator.Send(new GetOrdersByStatusQuery(status), cancellationToken));

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

    [HttpPost(Router.Version1.Order.CreatePayment)]
    public async Task<IActionResult> CreatePayment(int orderId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        var result = await payments.CreatePaymentAsync(orderId, userId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost(Router.Version1.Order.VerifyPayment)]
    public async Task<IActionResult> VerifyPayment(int orderId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        var result = await payments.VerifyPaymentAsync(orderId, userId, cancellationToken);
        return result is null ? NotFound() : Ok(new { isPaid = result.Value });
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

    private string? GetUserId() 
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"); }
}
