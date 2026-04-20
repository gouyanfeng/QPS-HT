using QPS.Application.Interfaces;
using QPS.Domain.Aggregates.OrderAggregate;
using QPS.Domain.Aggregates.RoomAggregate;
using System;
using System.Threading.Tasks;

namespace QPS.Infrastructure.BackgroundJobs;

public class PowerOffJob
{
    private readonly IDbContext _dbContext;
    private readonly IMqttService _mqttService;

    public PowerOffJob(IDbContext dbContext, IMqttService mqttService)
    {
        _dbContext = dbContext;
        _mqttService = mqttService;
    }

    public async Task Execute(Guid orderId)
    {
        var order = await _dbContext.Orders.FindAsync(orderId);
        if (order == null) return;

        var room = await _dbContext.Rooms.FindAsync(order.RoomId);
        if (room == null) return;

        await _mqttService.SendCommandAsync(room.DeviceConfig.MqttTopic, room.DeviceConfig.PowerOffCommand);
        room.SetToIdle();
        await _dbContext.SaveChangesAsync();
    }
}