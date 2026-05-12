using MediatR;
using Microsoft.Extensions.Logging;
using QPS.Application.Interfaces;
using QPS.Domain.Events;

namespace QPS.Infrastructure.EventHandlers;

public class SessionExpiredEventHandler : INotificationHandler<SessionExpiredEvent>
{
    private readonly IDbContext _dbContext;
    private readonly IMqttService _mqttService;
    private readonly ILogger<SessionExpiredEventHandler> _logger;

    public SessionExpiredEventHandler(IDbContext dbContext, IMqttService mqttService, ILogger<SessionExpiredEventHandler> logger)
    {
        _dbContext = dbContext;
        _mqttService = mqttService;
        _logger = logger;
    }

    public async Task Handle(SessionExpiredEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _dbContext.Orders.FindAsync(notification.OrderId, cancellationToken);
            if (order == null)
            {
                _logger.LogWarning($"会话过期事件：订单 {notification.OrderId} 不存在");
                return;
            }

            var room = await _dbContext.Rooms.FindAsync(order.RoomId, cancellationToken);
            if (room == null)
            {
                _logger.LogWarning($"会话过期事件：房间 {order.RoomId} 不存在");
                return;
            }

            await SendSessionExpiredCommand(room, order.Id);
            _logger.LogInformation($"会话过期：订单 {order.OrderNo} 已发送会话过期指令到房间 {room.Name}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"处理会话过期事件失败，订单ID: {notification.OrderId}");
            // 会话过期处理失败不影响主流程，记录日志但不抛出异常
            // 可考虑将失败记录到数据库，后续通过定时任务重试
        }
    }

    private async Task SendSessionExpiredCommand(Domain.Entities.Room room, Guid orderId)
    {
        var shop = await _dbContext.Shops.FindAsync(room.ShopId);
        if (shop == null)
        {
            _logger.LogWarning($"会话过期事件：店铺 {room.ShopId} 不存在");
            return;
        }

        var sessionExpiredCommand = new
        {
            OrderId = orderId.ToString(),
            RoomId = room.Id.ToString(),
            RoomName = room.Name,
            ShopId = shop.Id.ToString(),
            Command = "session_expired",
            Timestamp = DateTime.UtcNow
        };

        var topic = $"qps/{shop.Id}/{room.Id}/command";
        await _mqttService.SendCommandAsync(topic, System.Text.Json.JsonSerializer.Serialize(sessionExpiredCommand));
    }
}