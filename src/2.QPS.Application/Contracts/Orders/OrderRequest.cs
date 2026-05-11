namespace QPS.Application.Contracts.Orders;

public class CreateOrderRequest
{
    public Guid RoomId { get; set; }
    public decimal Amount { get; set; }
    public int DurationMinutes { get; set; }
}

public class PayOrderRequest
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; }
}

public class CompleteOrderRequest
{
    public decimal OriginAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal ActualAmount { get; set; }
    public string PaymentMethod { get; set; }
}