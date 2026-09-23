namespace E_commerce.Data.Dtos.Orders;

/// <summary>Order line fields needed to display a placed order.</summary>
public sealed record OrderItemDto(int ProductId, int Quantity, decimal UnitPrice, decimal LineTotal);

/// <summary>Fields a customer supplies for one requested order line.</summary>
public sealed record CreateOrderItemDto(int ProductId, int Quantity);
