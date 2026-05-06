namespace QPS.Application.Contracts.Reviews;

public class ReviewUpdateRequest
{
    public int Score { get; set; }
    public string Content { get; set; }
}