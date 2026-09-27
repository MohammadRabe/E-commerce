using E_commerce.Api.Hubs;
using E_commerce.Data.Entities;
using E_commerce.Data.Enum;
using E_commerce.Infrastructure.Data;
using E_commerce.Service.Abstraction;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Api.Services;

public sealed class OrderNotificationService(
    IHubContext<OrderNotificationsHub> hub,
    ILogger<OrderNotificationService> logger,
    AppDbContext db,
    UserManager<User> users) : IOrderNotificationService
{
    public async Task NotifyNewOrderAsync(int orderId, decimal totalAmount, CancellationToken cancellationToken = default)
    {
        IList<User> admins;
        try
        {
            admins = await users.GetUsersInRoleAsync("Admin");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Could not load admins to notify about order {OrderId}.", orderId);
            return;
        }
        foreach (var admin in admins)
        {
            var notification = new UserNotification
            {
                RecipientUserId = admin.Id,
                OrderId = orderId,
                Type = "new-order",
                Title = "طلب جديد",
                Message = $"وصلك طلب جديد رقم {orderId} بإجمالي {totalAmount:N2} ر.س",
                Link = $"/dashboard/orders?orderId={orderId}",
                CreatedAt = DateTimeOffset.UtcNow
            };
            await PersistAndSendAsync(notification, "NewOrder", cancellationToken);
        }
    }

    public async Task NotifyOrderStatusChangedAsync(int orderId, string userId, OrderStatus status, CancellationToken cancellationToken = default)
    {
        var notification = new UserNotification
        {
            RecipientUserId = userId,
            OrderId = orderId,
            Type = "order-status",
            Title = "تحديث على طلبك",
            Message = $"حالة طلبك رقم {orderId}: {StatusLabel(status)}",
            Link = $"/profile?orderId={orderId}",
            CreatedAt = DateTimeOffset.UtcNow
        };
        await PersistAndSendAsync(notification, "OrderStatusChanged", cancellationToken, status);
    }

    private async Task PersistAndSendAsync(UserNotification notification, string eventName, CancellationToken cancellationToken, OrderStatus? status = null)
    {
        try
        {
            db.UserNotifications.Add(notification);
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            db.Entry(notification).State = EntityState.Detached;
            logger.LogError(exception, "Could not persist order notification {EventName} for {RecipientUserId}.", eventName, notification.RecipientUserId);
        }

        try
        {
            var payload = new Dictionary<string, object?>
            {
                ["id"] = notification.Id == 0 ? $"notification-{Guid.NewGuid():N}" : notification.Id,
                ["orderId"] = notification.OrderId,
                ["link"] = notification.Link,
                ["type"] = notification.Type,
                ["title"] = notification.Title,
                ["message"] = notification.Message,
                ["createdAt"] = notification.CreatedAt,
                ["isRead"] = notification.IsRead
            };
            if (status.HasValue) payload["status"] = status.Value.ToString();
            await hub.Clients.Group(OrderNotificationsHub.UserGroup(notification.RecipientUserId)).SendAsync(eventName, payload, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Could not send real-time order notification {EventName} to {RecipientUserId}.", eventName, notification.RecipientUserId);
        }
    }

    private static string StatusLabel(OrderStatus status) => status switch
    {
        OrderStatus.Pending => "بانتظار التأكيد",
        OrderStatus.Processing => "قيد التجهيز",
        OrderStatus.Shipped => "طلع للتوصيل",
        OrderStatus.Delivered => "تم التوصيل",
        OrderStatus.Cancelled => "تم إلغاء الطلب",
        _ => status.ToString()
    };
}
