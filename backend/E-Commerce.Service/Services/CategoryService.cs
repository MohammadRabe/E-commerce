using E_commerce.Data.Entities;
using E_commerce.Data.Wrappers;
using E_commerce.Infrastructure.Abstraction;
using E_commerce.Service.Abstraction;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace E_commerce.Service.Services;

public sealed class CategoryService : BaseService, ICategoryService
{
    public CategoryService(IUOW uow) : base(uow)
    {
    }

    public  async Task<PagedList<Category>> GetPagedList(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default,
        params Expression<Func<Category, object?>>[] includes) =>
        await _uow.CategoryRepository.GetAll(includes: includes)
            .ToPagedList(pageNumber, pageSize, cancellationToken);

    public  async Task<IReadOnlyList<Category>> GetAllAsync(
        CancellationToken cancellationToken = default,
        params Expression<Func<Category, object?>>[] includes) =>
        await _uow.CategoryRepository.GetAll(includes: includes).ToListAsync(cancellationToken);

    public  async Task<Category?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default,
        params Expression<Func<Category, object?>>[] includes)
    {
        return await _uow.CategoryRepository.GetById(id, cancellationToken, includes);
    }

    public  async Task<Category> AddAsync(Category entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var created = _uow.CategoryRepository.Add(entity);
        await _uow.SaveAsync();
        return created;
    }

    public  async Task<Category> UpdateAsync(Category entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var existing = await _uow.CategoryRepository.GetById(entity.Id)
            ?? throw new KeyNotFoundException($"Category {entity.Id} was not found.");
        existing.Name = entity.Name;

        _uow.CategoryRepository.Edit(existing);
        await _uow.SaveAsync();
        return existing;
    }

    public  async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!_uow.CategoryRepository.Delete(id))
        {
            return false;
        }

        await _uow.SaveAsync();
        return true;
    }

    public  void IncludeTo(Category entity, Expression<Func<Category, object?>> includeExpression) =>
        _uow.CategoryRepository.IncludeTo(entity, includeExpression);
}
