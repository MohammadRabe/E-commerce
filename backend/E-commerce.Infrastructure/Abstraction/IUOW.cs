namespace E_commerce.Infrastructure.Abstraction;

public interface IUOW : IDisposable
{
    IProductRepository ProductRepository { get; }
    ICategoryRepository CategoryRepository { get; }
    IOrderRepository OrderRepository { get; }
    Task<bool> SaveAsync();
}
