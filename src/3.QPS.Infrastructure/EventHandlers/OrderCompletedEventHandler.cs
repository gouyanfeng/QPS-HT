using MediatR;
using Microsoft.Extensions.Logging;
using QPS.Application.Interfaces;
using QPS.Domain.Events;

namespace QPS.Infrastructure.EventHandlers;

public class OrderCompletedEventHandler : INotificationHandler<OrderCompletedEvent>
{
    private readonly IDbContext _dbContext;
    private readonly IMqttService _mqttService;
    private readonly ILogger<OrderCompletedEventHandler> _logger;

    public OrderCompletedEventHandler(IDbContext dbContext, IMqttService mqttService, ILogger<OrderCompletedEventHandler> logger)
    {
        _dbContext = dbContext;
        _mqttService = mqttService;
        _logger = logger;
    }

    public async Task Handle(OrderCompletedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var room = await _dbContext.Rooms.FindAsync(notification.RoomId, cancellationToken);
            if (room == null)
            {
                _logger.LogWarning($"订单完成事件：房间 {notification.RoomId} 不存在");
                return;
            }

            await SendPowerOffCommand(room, notification.OrderId);
            _logger.LogInformation($"订单 {notification.OrderId} 已完成，已发送断电指令到房间 {room.Name}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"处理订单完成事件失败，订单ID: {notification.OrderId}");
            // 断电失败不影响主流程，记录日志但不抛出异常
            // 可考虑将失败记录到数据库，后续通过定时任务重试
        }
    }

    private async Task SendPowerOffCommand(Domain.Entities.Room room, Guid orderId)
    {
        var shop = await _dbContext.Shops.FindAsync(room.ShopId);
        if (shop == null)
        {
            _logger.LogWarning($"订单完成事件：店铺 {room.ShopId} 不存在");
            return;
        }

        var powerOffCommand = new
        {
            OrderId = orderId.ToString(),
            RoomId = room.Id.ToString(),
            RoomName = room.Name,
            ShopId = shop.Id.ToString(),
            Command = "power_off",
            Timestamp = DateTime.UtcNow
        };

        var topic = $"qps/{shop.Id}/{room.Id}/command";
        await _mqttService.SendCommandAsync(topic, System.Text.Json.JsonSerializer.Serialize(powerOffCommand));
    }
}