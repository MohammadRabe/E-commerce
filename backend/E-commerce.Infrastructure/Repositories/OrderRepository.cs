using E_commerce.Data.Entities;
using E_commerce.Infrastructure.Abstraction;
using E_commerce.Infrastructure.Data;

namespace E_commerce.Infrastructure.Repositories;

public sealed class OrderRepository(AppDbContext context)
    : BaseRepository<Order>(context), IOrderRepository
{
}
