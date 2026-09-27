using E_commerce.Data.Entities;
using E_commerce.Infrastructure.Abstraction;
using E_commerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Infrastructure.Repositories;

public sealed class OrderRepository(AppDbContext context)
    : BaseRepository<Order>(context), IOrderRepository
{
    public Task<Order?> GetForPaymentAsync(int orderId, CancellationToken cancellationToken = default) =>
        context.Orders.Include(order => order.User)
            .Include(order => order.Items).ThenInclude(item => item.Product)
            .FirstOrDefaultAsync(order => order.Id == orderId, cancellationToken);
}
