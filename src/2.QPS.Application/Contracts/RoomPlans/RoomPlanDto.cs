namespace QPS.Application.Contracts.RoomPlans;

public class RoomPlanDto
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public string RoomNumber { get; set; }
    public Guid PlanId { get; set; }
    public string PlanName { get; set; }
}

public class RoomPlanRequest
{
    public Guid RoomId { get; set; }
    public Guid PlanId { get; set; }
}