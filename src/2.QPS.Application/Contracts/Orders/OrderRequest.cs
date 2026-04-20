namespace QPS.Application.Contracts.Orders;

public class CreateOrderRequest
{
    public Guid RoomId { get; set; }
    public decimal Amount { get; set; }
    public int DurationMinutes { get; set; }
}

public class SettleOrderRequest
{
    public Guid OrderId { get; set; }
}