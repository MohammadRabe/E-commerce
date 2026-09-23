using E_commerce.Infrastructure.Abstraction;
using E_commerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace E_commerce.Infrastructure.Repositories;

public abstract class BaseRepository<TEntity> : IRepository<TEntity>
    where TEntity : class
{
    private readonly AppDbContext _context;

    protected BaseRepository(AppDbContext context)
    {
        _context = context;
    }

    public TEntity Add(TEntity entity)
    {
        _context.Add(entity);
        return entity;
    }

    public IDbContextTransaction BeginTransaction() => _context.Database.BeginTransaction();
    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default) =>
        _context.Database.BeginTransactionAsync(cancellationToken);

    public bool Delete(int id)
    {
        var entity = _context.Set<TEntity>().Find(id);
        if (entity is null)
        {
            return false;
        }

        _context.Set<TEntity>().Remove(entity);
        return true;
    }

    public TEntity Edit(TEntity entity)
    {
        _context.Update(entity);
        return entity;
    }

    public IQueryable<TEntity> GetAll(
        bool asNoTracking = true,
        params Expression<Func<TEntity, object?>>[] includes)
    {
        IQueryable<TEntity> query = asNoTracking
            ? _context.Set<TEntity>().AsNoTracking()
            : _context.Set<TEntity>();

        foreach (var includeExpression in includes)
        {
            query = query.Include(includeExpression);
        }

        return query;
    }

    public Task<TEntity?> GetById(
        int id,
        CancellationToken cancellationToken = default,
        params Expression<Func<TEntity, object?>>[] includes)
    {
        IQueryable<TEntity> query = _context.Set<TEntity>();
        foreach (var includeExpression in includes)
        {
            query = query.Include(includeExpression);
        }

        return query.FirstOrDefaultAsync(
            entity => EF.Property<int>(entity, "Id") == id,
            cancellationToken);
    }

    public void IncludeTo(TEntity entity, Expression<Func<TEntity, object?>> includeExpression)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(includeExpression);

        var memberExpression = includeExpression.Body as MemberExpression;
        if (memberExpression is null && includeExpression.Body is UnaryExpression unaryExpression)
        {
            memberExpression = unaryExpression.Operand as MemberExpression;
        }

        if (memberExpression is null || memberExpression.Expression != includeExpression.Parameters[0])
        {
            throw new ArgumentException(
                "The include expression must select a direct navigation property, such as entity => entity.RelatedEntity.",
                nameof(includeExpression));
        }

        var entry = _context.Entry(entity);
        if (entry.State == EntityState.Detached)
        {
            _context.Attach(entity);
            entry = _context.Entry(entity);
        }

        entry.Navigation(memberExpression.Member.Name).Load();
    }
}
