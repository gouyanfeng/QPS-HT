using MediatR;
using QPS.Application.Interfaces;
using QPS.Domain.Aggregates.RoomAggregate;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace QPS.Application.Features.Rooms;

public class TogglePowerCommand : IRequest<bool>
{
    public Guid RoomId { get; set; }
    public bool PowerOn { get; set; }
}

public class TogglePowerHandler : IRequestHandler<TogglePowerCommand, bool>
{
    private readonly IDbContext _dbContext;
    private readonly IMqttService _mqttService;

    public TogglePowerHandler(IDbContext dbContext, IMqttService mqttService)
    {
        _dbContext = dbContext;
        _mqttService = mqttService;
    }

    public async Task<bool> Handle(TogglePowerCommand request, CancellationToken cancellationToken)
    {
        var room = await _dbContext.Rooms.FindAsync(request.RoomId, cancellationToken);
        if (room == null) return false;

        var command = request.PowerOn ? "POWER_ON" : "POWER_OFF";
        await _mqttService.SendCommandAsync(room.MqttTopic, command);
        return true;
    }
}