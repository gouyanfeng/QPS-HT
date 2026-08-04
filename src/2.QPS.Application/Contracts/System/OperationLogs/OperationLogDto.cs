namespace QPS.Application.Contracts.System.OperationLogs;

public class OperationLogDto
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string OperatorName { get; set; } = string.Empty;
    public string RequestPath { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string ChangeJson { get; set; } = string.Empty;
}
