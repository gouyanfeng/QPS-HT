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
            // 获取房间信息
            var room = await _dbContext.Rooms.FindAsync(notification.RoomId, cancellationToken);
            if (room == null)
            {
                _logger.LogWarning($"房间 {notification.RoomId} 不存在");
                return;
            }

            // 发送断电指令（通过MQTT）
            await SendPowerOffCommand(room, notification.OrderId);

            _logger.LogInformation($"订单 {notification.OrderId} 已完成，已发送断电指令到房间 {room.Name}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"处理订单完成事件失败，订单ID: {notification.OrderId}");
            // 断电失败不影响主流程，不抛出异常
        }
    }

    private async Task SendPowerOffCommand(Domain.Entities.Room room, Guid orderId)
    {
        // 获取房间所在的店铺
        var shop = await _dbContext.Shops.FindAsync(room.ShopId);
        if (shop == null)
        {
            _logger.LogWarning($"店铺 {room.ShopId} 不存在");
            return;
        }

        // 构建断电命令
        // 实际项目中需要根据硬件协议构建具体的命令格式
        var powerOffCommand = new
        {
            OrderId = orderId.ToString(),
            RoomId = room.Id.ToString(),
            RoomName = room.Name,
            ShopId = shop.Id.ToString(),
            Command = "power_off",
            Timestamp = DateTime.UtcNow
        };

        // 发送MQTT消息
        // 主题格式可以是: qps/{shopId}/{roomId}/command
        var topic = $"qps/{shop.Id}/{room.Id}/command";
        await _mqttService.SendCommandAsync(topic, System.Text.Json.JsonSerializer.Serialize(powerOffCommand));
    }
}