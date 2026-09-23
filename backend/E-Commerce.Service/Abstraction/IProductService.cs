using E_commerce.Data.Dtos.Products_;
using E_commerce.Data.Entities;
using E_commerce.Data.Wrappers;
using System.Linq.Expressions;

namespace E_commerce.Service.Abstraction;

public interface IProductService 
{
    Task<PagedList<ProductListDto>> GetPagedListAsync(int pageNumber, int pageSize, CancellationToken cancellationToken, params Expression<Func<Product, object?>>[] includes);
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default, params Expression<Func<Product, object?>>[] includes);
    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default, params Expression<Func<Product, object?>>[] includes);
     Task<Product> AddAsync(Product entity, CancellationToken cancellationToken = default);

    Task<Product> UpdateAsync(Product entity, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    public void IncludeTo(Product entity, Expression<Func<Product, object?>> includeExpression);

}
