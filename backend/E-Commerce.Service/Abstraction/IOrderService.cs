using E_commerce.Data.Entities;
using E_commerce.Data.Dtos.Orders;
using E_commerce.Data.Enum;
using System.Linq.Expressions;

namespace E_commerce.Service.Abstraction;

public interface IOrderService 
{
    Task<Order> PlaceOrderAsync(string userId, CreateOrderDto request, CancellationToken cancellationToken = default);
    Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default, params Expression<Func<Order, object?>>[] includes);
    Task<IReadOnlyList<Order>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default, params Expression<Func<Order, object?>>[] includes);
    Task<IReadOnlyList<Order>> GetByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default, params Expression<Func<Order, object?>>[] includes);
    Task<Order> UpdateAsync(Order entity, CancellationToken cancellationToken = default);
    Task<Order> UpdateStatusAsync(int orderId, OrderStatus status, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
