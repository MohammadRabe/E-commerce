using E_commerce.Data.Enum;

namespace E_commerce.Service.Abstraction;

public interface IOrderNotificationService
{
    Task NotifyNewOrderAsync(int orderId, decimal totalAmount, CancellationToken cancellationToken = default);
    Task NotifyOrderStatusChangedAsync(int orderId, string userId, OrderStatus status, CancellationToken cancellationToken = default);
}
