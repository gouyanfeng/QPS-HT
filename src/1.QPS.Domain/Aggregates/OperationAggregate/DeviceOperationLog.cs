using QPS.Domain.Common;

namespace QPS.Domain.Aggregates.OperationAggregate;

public class DeviceOperationLog : Entity
{
    public Guid RoomId { get; private set; }
    public string Action { get; private set; }
    public string Operator { get; private set; }
    public DateTime CreatedAt { get; private set; }

    protected DeviceOperationLog() { }

    public DeviceOperationLog(Guid roomId, string action, string @operator)
    {
        RoomId = roomId;
        Action = action;
        Operator = @operator;
        CreatedAt = DateTime.UtcNow;
    }
}