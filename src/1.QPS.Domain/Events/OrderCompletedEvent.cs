using QPS.Domain.Common;

namespace QPS.Domain.Events;

public class OrderCompletedEvent : DomainEvent
{
    public Guid OrderId { get; }
    public Guid RoomId { get; }

    public OrderCompletedEvent(Guid orderId, Guid roomId)
    {
        OrderId = orderId;
        RoomId = roomId;
    }
}