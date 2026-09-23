using E_commerce.Data.Entities;

namespace E_commerce.Service.Abstraction;

public interface ICategoryService 
{
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default, params System.Linq.Expressions.Expression<Func<Category, object?>>[] includes);
    Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken = default, params System.Linq.Expressions.Expression<Func<Category, object?>>[] includes);
    Task<Category> AddAsync(Category category,CancellationToken cancellationToken = default);
    Task<Category> UpdateAsync(Category category, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
