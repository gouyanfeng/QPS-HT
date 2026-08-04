using QPS.Domain.Common;

namespace QPS.Domain.Entities.System;

public class SystemOperationLog : BaseEntity
{
    public string ActionType { get; private set; } = string.Empty;
    public string EntityType { get; private set; } = string.Empty;
    public string EntityId { get; private set; } = string.Empty;
    public string OperatorName { get; private set; } = string.Empty;
    public string RequestPath { get; private set; } = string.Empty;
    public string IpAddress { get; private set; } = string.Empty;
    public string ChangeJson { get; private set; } = string.Empty;

    private SystemOperationLog() { }

    public SystemOperationLog(
        string actionType,
        string entityType,
        string entityId,
        string operatorName,
        string requestPath,
        string ipAddress,
        string changeJson)
    {
        ActionType = actionType;
        EntityType = entityType;
        EntityId = entityId;
        OperatorName = operatorName;
        RequestPath = requestPath;
        IpAddress = ipAddress;
        ChangeJson = changeJson;
    }
}
