using E_commerce.Data.Dtos.Products_;
using E_commerce.Data.Entities;
using E_commerce.Data.Wrappers;
using E_commerce.Infrastructure.Abstraction;
using E_commerce.Infrastructure.uow;
using E_commerce.Service.Abstraction;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace E_commerce.Service.Services;

public sealed class ProductService: IProductService
{
    private readonly IUOW _uow;

    public ProductService(IUOW uow)
    {
        _uow = uow;
    }

    public async Task<PagedList<ProductListDto>> GetPagedListAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default,
        params Expression<Func<Product, object?>>[] includes) =>
        await _uow.ProductRepository.GetAll(includes: includes)
            .Select(product => new ProductListDto(
                product.Id,
                product.Title,
                product.Price,
                product.Discount,
                product.Category.Name,
                product.Rating,
                product.ImagePaths.OrderBy(image => image.Id).Select(image => image.Url).FirstOrDefault(),
                product.StockQuantity))
            .ToPagedList(pageNumber, pageSize, cancellationToken);

    public  async Task<IReadOnlyList<Product>> GetAllAsync(
        CancellationToken cancellationToken = default,
        params Expression<Func<Product, object?>>[] includes) =>
        await _uow.ProductRepository.GetAll(includes: includes).ToListAsync(cancellationToken);

    public  async Task<Product?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default,
        params Expression<Func<Product, object?>>[] includes)
    {
        return await _uow.ProductRepository.GetById(id, cancellationToken, includes);
    }

    public  async Task<Product> AddAsync(Product entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var created = _uow.ProductRepository.Add(entity);
        await _uow.SaveAsync();
        return created;
    }

    public  async Task<Product> UpdateAsync(Product entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var existing = await _uow.ProductRepository.GetById(entity.Id)
            ?? throw new KeyNotFoundException($"Product {entity.Id} was not found.");

        existing.Title = entity.Title;
        existing.Description = entity.Description;
        existing.Price = entity.Price;
        existing.CategoryId = entity.CategoryId;
        existing.ImagePaths.Clear();
        foreach (var image in entity.ImagePaths)
            existing.ImagePaths.Add(new ImagePath { Url = image.Url, ProductId = entity.Id });
        existing.Rating = entity.Rating;
        existing.RatingCount = entity.RatingCount;
        existing.StockQuantity = entity.StockQuantity;

        _uow.ProductRepository.Edit(existing);
        await _uow.SaveAsync();
        return existing;
    }

    public  async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!_uow.ProductRepository.Delete(id))
        {
            return false;
        }

        await _uow.SaveAsync();
        return true;
    }

    public  void IncludeTo(Product entity, Expression<Func<Product, object?>> includeExpression) =>
        _uow.ProductRepository.IncludeTo(entity, includeExpression);


}
