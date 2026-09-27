using E_commerce.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace E_commerce.Api.Controllers.Notifications;

[ApiController]
[Authorize]
[Route("api/v1/notifications")]
public sealed class NotificationsController(AppDbContext db) : ControllerBase
{
    [HttpGet("mine")]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var notifications = await db.UserNotifications
            .AsNoTracking()
            .Where(notification => notification.RecipientUserId == userId)
            .OrderByDescending(notification => notification.CreatedAt)
            .Take(50)
            .Select(notification => new
            {
                notification.Id,
                notification.OrderId,
                notification.Type,
                notification.Title,
                notification.Message,
                notification.Link,
                notification.CreatedAt,
                notification.IsRead
            })
            .ToListAsync(cancellationToken);

        return Ok(notifications);
    }

    [HttpPatch("read")]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        await db.UserNotifications
            .Where(notification => notification.RecipientUserId == userId && !notification.IsRead)
            .ExecuteUpdateAsync(update => update.SetProperty(notification => notification.IsRead, true), cancellationToken);
        return NoContent();
    }

    [HttpDelete("mine")]
    public async Task<IActionResult> ClearMine(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        await db.UserNotifications
            .Where(notification => notification.RecipientUserId == userId)
            .ExecuteDeleteAsync(cancellationToken);
        return NoContent();
    }
}
