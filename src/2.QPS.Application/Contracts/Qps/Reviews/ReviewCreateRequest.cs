namespace QPS.Application.Contracts.Qps.Reviews;

public class ReviewCreateRequest
{
    public Guid OrderId { get; set; }
    public int Score { get; set; }
    public string Content { get; set; }
}