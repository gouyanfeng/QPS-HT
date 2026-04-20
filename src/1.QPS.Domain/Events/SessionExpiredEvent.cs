using QPS.Domain.Common;

namespace QPS.Domain.Events;

public class SessionExpiredEvent : DomainEvent
{
    public Guid OrderId { get; }

    public SessionExpiredEvent(Guid orderId)
    {
        OrderId = orderId;
    }
}