namespace QPS.Application.Contracts.System.OperationLogs;

public class OperationLogDto
{
    public Guid Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string ChangeJson { get; set; } = string.Empty;
    public string OperatorUserId { get; set; } = string.Empty;
    public string OperatorName { get; set; } = string.Empty;
    public string RequestPath { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
