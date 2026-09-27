using E_commerce.Data.Dtos.Orders;
using E_commerce.Data.Entities;

namespace E_commerce.Core.Features.Orders.Handlers;

internal static class OrderDtoMapper
{
    public static OrderDetailsDto Map(Order order) => new(
        order.Id,
        order.CreatedAt,
        order.Status,
        order.ShippingAddress,
        order.ShippingPhoneNumber,
        order.Items.Sum(item => item.UnitPrice * item.Quantity),
        order.Currency,
        order.PaymentStatus,
        order.Items.Select(item => new OrderItemDto(
            item.ProductId,
            item.Quantity,
            item.UnitPrice,
            item.UnitPrice * item.Quantity)).ToList());
}
