using E_commerce.Data.Enum;
using E_commerce.Data.Entities;
using E_commerce.Infrastructure.Data;
using E_commerce.Data.Dtos.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Api.Controllers.Admin;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/v1/admin")]
public sealed class SalesStatisticsController(AppDbContext db, UserManager<User> userManager) : ControllerBase
{
    private static readonly OrderStatus[] TrackedStatuses =
    [
        OrderStatus.Pending,
        OrderStatus.Processing,
        OrderStatus.Shipped,
        OrderStatus.Delivered,
        OrderStatus.Cancelled
    ];

    [HttpGet("getStatistics")]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var customerRoleId = await db.Roles
            .Where(role => role.Name == "Customer")
            .Select(role => role.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var customerCount = string.IsNullOrEmpty(customerRoleId)
            ? 0
            : await db.UserRoles.CountAsync(userRole => userRole.RoleId == customerRoleId, cancellationToken);

        var totalOrders = await db.Orders.CountAsync(cancellationToken);
        var succeededOrders = await db.Orders.CountAsync(order => order.Status == OrderStatus.Delivered, cancellationToken);
        var activeSince = DateTimeOffset.UtcNow.AddDays(-30);
        var activeCustomers = string.IsNullOrEmpty(customerRoleId)
            ? 0
            : await db.Orders
                .Where(order => order.CreatedAt >= activeSince && order.Status != OrderStatus.Cancelled && db.UserRoles.Any(userRole =>
                    userRole.UserId == order.UserId && userRole.RoleId == customerRoleId))
                .Select(order => order.UserId)
                .Distinct()
                .CountAsync(cancellationToken);
        var statusCounts = await db.Orders
            .GroupBy(order => order.Status)
            .Select(group => new { Status = group.Key, Count = group.Count() })
            .ToListAsync(cancellationToken);

        var statusBreakdown = TrackedStatuses.Select(status =>
        {
            var count = statusCounts.FirstOrDefault(item => item.Status == status)?.Count ?? 0;
            return new
            {
                status = status.ToString(),
                count,
                percentage = totalOrders == 0 ? 0 : Math.Round(count * 100d / totalOrders, 1)
            };
        });

        return Ok(new
        {
            customerCount,
            purchasingProcessCount = totalOrders,
            succeededProcessCount = succeededOrders,
            activeCustomerCount = activeCustomers,
            succeededProcessPercentage = totalOrders == 0 ? 0 : Math.Round(succeededOrders * 100d / totalOrders, 1),
            activeCustomerPercentage = customerCount == 0 ? 0 : Math.Round(activeCustomers * 100d / customerCount, 1),
            statusBreakdown,
            activeCustomerWindowDays = 30
        });
    }

    [HttpGet("getCustomers")]
    public async Task<IActionResult> GetCustomers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string search = "",
        CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var customerRole = await db.Roles
            .Where(role => role.Name == "Customer")
            .Select(role => new { role.Id })
            .FirstOrDefaultAsync(cancellationToken);

        if (customerRole is null)
        {
            return Ok(new { items = Array.Empty<object>(), pageNumber, pageSize, totalCount = 0, next = false, prev = pageNumber > 1 });
        }

        var query = from user in userManager.Users
                    join userRole in db.UserRoles on user.Id equals userRole.UserId
                    where userRole.RoleId == customerRole.Id
                    select user;

        var term = search.Trim();
        if (term.Length > 0)
        {
            query = query.Where(user =>
                (user.UserName != null && user.UserName.Contains(term)) ||
                (user.Email != null && user.Email.Contains(term)) ||
                (user.PhoneNumber != null && user.PhoneNumber.Contains(term)) ||
                (user.Phone != null && user.Phone.Contains(term)));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var customers = await query
            .OrderBy(user => user.UserName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(user => new
            {
                id = user.Id,
                userName = user.UserName,
                email = user.Email,
                phoneNumber = user.PhoneNumber ?? user.Phone
            })
            .ToListAsync(cancellationToken);

        return Ok(new
        {
            items = customers,
            pageNumber,
            pageSize,
            totalCount,
            next = pageNumber * pageSize < totalCount,
            prev = pageNumber > 1
        });
    }

    [HttpDelete("customers/{userId}")]
    public async Task<IActionResult> DeleteCustomer(string userId, CancellationToken cancellationToken)
    {
        var customer = await userManager.FindByIdAsync(userId);
        if (customer is null || !await userManager.IsInRoleAsync(customer, "Customer"))
        {
            return NotFound();
        }

        if (await userManager.IsInRoleAsync(customer, "Admin"))
        {
            return Conflict(new { message = "Admin accounts cannot be deleted from the customer list." });
        }

        if (await db.Orders.AnyAsync(order => order.UserId == userId, cancellationToken))
        {
            return Conflict(new { message = "This customer has order history and cannot be deleted." });
        }

        var result = await userManager.DeleteAsync(customer);
        if (!result.Succeeded)
        {
            return Conflict(new { errors = result.Errors.Select(error => error.Description) });
        }

        return NoContent();
    }

    [HttpGet("getOrders")]
    public async Task<IActionResult> GetOrders(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string search = "",
        [FromQuery] OrderStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = db.Orders.AsNoTracking().AsQueryable();
        if (status.HasValue) query = query.Where(order => order.Status == status.Value);

        var term = search.Trim();
        if (term.Length > 0)
        {
            var numericId = int.TryParse(term.TrimStart('#'), out var parsedId) ? parsedId : (int?)null;
            query = query.Where(order =>
                (numericId.HasValue && order.Id == numericId.Value) ||
                order.ShippingAddress.Contains(term) ||
                order.ShippingPhoneNumber.Contains(term) ||
                order.User.UserName!.Contains(term) ||
                order.User.Email!.Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var orders = await query
            .OrderByDescending(order => order.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(order => new
            {
                order.Id,
                order.CreatedAt,
                order.Status,
                order.ShippingAddress,
                order.ShippingPhoneNumber,
                CustomerName = order.User.UserName,
                CustomerEmail = order.User.Email,
                TotalAmount = order.Items.Sum(item => item.UnitPrice * item.Quantity),
                ItemCount = order.Items.Sum(item => item.Quantity)
            })
            .ToListAsync(cancellationToken);

        return Ok(new
        {
            items = orders,
            pageNumber,
            pageSize,
            totalCount,
            next = pageNumber * pageSize < totalCount,
            prev = pageNumber > 1
        });
    }
}
