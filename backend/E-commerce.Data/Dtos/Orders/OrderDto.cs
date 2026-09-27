using E_commerce.Data.Enum;

namespace E_commerce.Data.Dtos.Orders;

/// <summary>Compact order information for order lists.</summary>
public sealed record OrderSummaryDto(int Id, DateTimeOffset CreatedAt, OrderStatus Status, decimal TotalAmount, string Currency, int ItemCount);

/// <summary>Order information and its lines for an order details view.</summary>
public sealed record OrderDetailsDto(int Id, DateTimeOffset CreatedAt, OrderStatus Status, string ShippingAddress, string ShippingPhoneNumber, decimal TotalAmount, string Currency, string PaymentStatus, IReadOnlyList<OrderItemDto> Items);

/// <summary>Customer supplied fields when placing an order. User and price are resolved server-side.</summary>
public sealed record CreateOrderDto(string ShippingAddress, string ShippingPhoneNumber, IReadOnlyList<CreateOrderItemDto> Items);
