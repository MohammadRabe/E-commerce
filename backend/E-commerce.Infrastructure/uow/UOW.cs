using E_commerce.Infrastructure.Abstraction;
using E_commerce.Infrastructure.Data;
using E_commerce.Infrastructure.Repositories;

namespace E_commerce.Infrastructure.uow;

public sealed class UOW : IUOW
{
    private readonly AppDbContext _context;

    public IProductRepository ProductRepository { get; }
    public ICategoryRepository CategoryRepository { get; }
    public IOrderRepository OrderRepository { get; }

    public UOW(AppDbContext context)
    {
        _context = context;
        ProductRepository = new ProductRepository(_context);
        CategoryRepository = new CategoryRepository(_context);
        OrderRepository = new OrderRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    public async Task<bool> SaveAsync()
    {
        await _context.SaveChangesAsync();
        return true;
    }
}
