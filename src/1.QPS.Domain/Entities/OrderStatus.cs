namespace QPS.Domain.Entities;

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