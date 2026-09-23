using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace E_commerce.Infrastructure.Abstraction;

public interface IRepository<TEntity> where TEntity : class
{
    IQueryable<TEntity> GetAll(
        bool asNoTracking = true,
        params Expression<Func<TEntity, object?>>[] includes);
    void IncludeTo(TEntity entity, Expression<Func<TEntity, object?>> includeExpression);
    IDbContextTransaction BeginTransaction();
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task<TEntity?> GetById(
        int id,
        CancellationToken cancellationToken = default,
        params Expression<Func<TEntity, object?>>[] includes);
    TEntity Add(TEntity entity);
    TEntity Edit(TEntity entity);
    bool Delete(int id);
}
