using QPS.Application.Extensions;

namespace QPS.Application.Contracts.System.OperationLogs;

public class OperationLogQueryRequest : PaginationRequest
{
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public string? ActionType { get; set; }
    public string? OperatorUserId { get; set; }
    public string? OperatorName { get; set; }
    public string? RequestPath { get; set; }
    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }
}
