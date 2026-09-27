using E_commerce.Data.Entities;

namespace E_commerce.Infrastructure.Abstraction;

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetForPaymentAsync(int orderId, CancellationToken cancellationToken = default);
}
