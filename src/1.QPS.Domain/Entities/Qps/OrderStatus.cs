namespace QPS.Domain.Entities.Qps;

public enum OrderStatus
{
    Completed,
    WaitingPayment,
    Paid,
    Refunding,
    Refunded,
    Cancelled,
    Timeout
}