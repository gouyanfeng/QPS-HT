namespace QPS.Application.Contracts.Plans;

public class PlanCreateRequest
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int DurationMinutes { get; set; }
}