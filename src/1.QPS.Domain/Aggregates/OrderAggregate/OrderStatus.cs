namespace QPS.Domain.Aggregates.OrderAggregate;

public enum OrderStatus
{
    Pending,
    Paid,
    Using,
    Completed,
    Cancelled
}