namespace QPS.Application.Contracts.Orders;

public class OrderDto
{
    public Guid Id { get; set; }
    public string OrderNo { get; set; }
    public Guid ShopId { get; set; }
    public string ShopName { get; set; }
    public Guid RoomId { get; set; }
    public string RoomNumber { get; set; }
    public Guid? CustomerId { get; set; }
    public string CustomerName { get; set; }
    public string Status { get; set; }
    public decimal OriginAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal ActualAmount { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? PaymentMethod { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
}