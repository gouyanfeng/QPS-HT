using QPS.Domain.Common;
using QPS.Domain.Events;

namespace QPS.Domain.Aggregates.OrderAggregate;

public class Order : AggregateRoot
{
    public string OrderNumber { get; private set; }
    public Guid RoomId { get; private set; }
    public Guid MerchantId { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime? EndTime { get; private set; }
    public PricingStrategy PricingStrategy { get; private set; }

    protected Order() { }

    public Order(string orderNumber, Guid roomId, Guid merchantId, PricingStrategy pricingStrategy)
    {
        OrderNumber = orderNumber;
        RoomId = roomId;
        MerchantId = merchantId;
        Status = OrderStatus.Pending;
        PricingStrategy = pricingStrategy;
    }

    public void Pay(decimal amount)
    {
        Amount = amount;
        Status = OrderStatus.Paid;
        StartTime = DateTime.UtcNow;
        AddDomainEvent(new OrderPaidEvent(this.Id));
    }

    public void StartUsing() { Status = OrderStatus.Using; }
    public void Complete() 
    {
        Status = OrderStatus.Completed;
        EndTime = DateTime.UtcNow;
        AddDomainEvent(new SessionExpiredEvent(this.Id));
    }
    public void Cancel() { Status = OrderStatus.Cancelled; }
}