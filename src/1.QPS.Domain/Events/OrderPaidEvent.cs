using QPS.Domain.Common;

namespace QPS.Domain.Events;

public class OrderPaidEvent : DomainEvent
{
    public Guid OrderId { get; }

    public OrderPaidEvent(Guid orderId)
    {
        OrderId = orderId;
    }
}