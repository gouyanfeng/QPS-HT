namespace QPS.Application.Contracts.Reviews;

public class ReviewDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string OrderNo { get; set; }
    public Guid RoomId { get; set; }
    public string RoomNumber { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; }
    public string CustomerPhone { get; set; }
    public int Score { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
}