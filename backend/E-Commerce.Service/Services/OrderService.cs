using E_commerce.Data.Entities;
using E_commerce.Data.Dtos.Orders;
using E_commerce.Data.Wrappers;
using E_commerce.Infrastructure.Abstraction;
using E_commerce.Service.Abstraction;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace E_commerce.Service.Services;

public sealed class OrderService : BaseService, IOrderService
{
    public OrderService(IUOW uow) : base(uow)
    {
    }

    public async Task<Order> PlaceOrderAsync(string userId, CreateOrderDto request, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _uow.OrderRepository.BeginTransactionAsync(cancellationToken);
        var order = new Order
        {
            UserId = userId,
            ShippingAddress = request.ShippingAddress.Trim(),
            ShippingPhoneNumber = request.ShippingPhoneNumber.Trim(),
            Status = E_commerce.Data.Enum.OrderStatus.Pending
        };

        foreach (var requestedItem in request.Items)
        {
            var product = await _uow.ProductRepository.GetById(requestedItem.ProductId, cancellationToken)
                ?? throw new KeyNotFoundException($"Product {requestedItem.ProductId} was not found.");
            if (product.StockQuantity < requestedItem.Quantity)
                throw new InvalidOperationException($"Insufficient stock for product {product.Id}.");

            product.StockQuantity -= requestedItem.Quantity;
            _uow.ProductRepository.Edit(product);
            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                Quantity = requestedItem.Quantity,
                UnitPrice = product.Price
            });
        }

        _uow.OrderRepository.Add(order);
        await _uow.SaveAsync();
        await transaction.CommitAsync(cancellationToken);
        return order;
    }

    public  async Task<PagedList<Order>> GetPagedList(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default,
        params Expression<Func<Order, object?>>[] includes) =>
        await _uow.OrderRepository.GetAll(includes: includes)
            .ToPagedList(pageNumber, pageSize, cancellationToken);

    public  async Task<IReadOnlyList<Order>> GetAllAsync(
        CancellationToken cancellationToken = default,
        params Expression<Func<Order, object?>>[] includes) =>
        await _uow.OrderRepository.GetAll(includes: includes).ToListAsync(cancellationToken);

    public  async Task<Order?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default,
        params Expression<Func<Order, object?>>[] includes)
    {
        return await _uow.OrderRepository.GetById(id, cancellationToken, includes);
    }

    public async Task<IReadOnlyList<Order>> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default,
        params Expression<Func<Order, object?>>[] includes) =>
        await _uow.OrderRepository.GetAll(includes: includes)
            .Where(order => order.UserId == userId)
            .OrderByDescending(order => order.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Order>> GetByStatusAsync(
        E_commerce.Data.Enum.OrderStatus status,
        CancellationToken cancellationToken = default,
        params Expression<Func<Order, object?>>[] includes) =>
        await _uow.OrderRepository.GetAll(includes: includes)
            .Where(order => order.Status == status)
            .OrderByDescending(order => order.CreatedAt)
            .ToListAsync(cancellationToken);

    public  async Task<Order> AddAsync(Order entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var created = _uow.OrderRepository.Add(entity);
        await _uow.SaveAsync();
        return created;
    }

    public  async Task<Order> UpdateAsync(Order entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var existing = await _uow.OrderRepository.GetById(entity.Id)
            ?? throw new KeyNotFoundException($"Order {entity.Id} was not found.");

        existing.Status = entity.Status;
        existing.ShippingAddress = entity.ShippingAddress;
        existing.ShippingPhoneNumber = entity.ShippingPhoneNumber;

        _uow.OrderRepository.Edit(existing);
        await _uow.SaveAsync();
        return existing;
    }

    public async Task<Order> UpdateStatusAsync(int orderId, E_commerce.Data.Enum.OrderStatus status, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _uow.OrderRepository.BeginTransactionAsync(cancellationToken);
        var existing = await _uow.OrderRepository.GetById(orderId, cancellationToken, order => order.Items)
            ?? throw new KeyNotFoundException($"Order {orderId} was not found.");

        if (existing.Status != E_commerce.Data.Enum.OrderStatus.Cancelled && status == E_commerce.Data.Enum.OrderStatus.Cancelled)
        {
            foreach (var item in existing.Items)
            {
                var product = await _uow.ProductRepository.GetById(item.ProductId, cancellationToken)
                    ?? throw new KeyNotFoundException($"Product {item.ProductId} was not found.");
                product.StockQuantity += item.Quantity;
                _uow.ProductRepository.Edit(product);
            }
        }

        existing.Status = status;
        _uow.OrderRepository.Edit(existing);
        await _uow.SaveAsync();
        await transaction.CommitAsync(cancellationToken);
        return existing;
    }

    public  async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!_uow.OrderRepository.Delete(id))
        {
            return false;
        }

        await _uow.SaveAsync();
        return true;
    }

    public  void IncludeTo(Order entity, Expression<Func<Order, object?>> includeExpression) =>
        _uow.OrderRepository.IncludeTo(entity, includeExpression);

}
