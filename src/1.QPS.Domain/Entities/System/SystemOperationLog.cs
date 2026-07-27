using QPS.Domain.Common;

namespace QPS.Domain.Entities.System;

public class SystemOperationLog : BaseEntity
{
    public string EntityType { get; private set; } = string.Empty;
    public string EntityId { get; private set; } = string.Empty;
    public string ActionType { get; private set; } = string.Empty;
    public string ChangeJson { get; private set; } = string.Empty;
    public string OperatorUserId { get; private set; } = string.Empty;
    public string OperatorName { get; private set; } = string.Empty;
    public string RequestPath { get; private set; } = string.Empty;
    public string IpAddress { get; private set; } = string.Empty;
    public string UserAgent { get; private set; } = string.Empty;

    private SystemOperationLog() { }

    private SystemOperationLog(
        string entityType,
        string entityId,
        string actionType,
        string changeJson,
        string operatorUserId,
        string operatorName,
        string requestPath,
        string ipAddress,
        string userAgent)
    {
        EntityType = entityType;
        EntityId = entityId;
        ActionType = actionType;
        ChangeJson = changeJson;
        OperatorUserId = operatorUserId;
        OperatorName = operatorName;
        RequestPath = requestPath;
        IpAddress = ipAddress;
        UserAgent = userAgent;
    }

    public static SystemOperationLog Create(
        string entityType,
        string entityId,
        string actionType,
        string changeJson,
        string operatorUserId,
        string operatorName,
        string requestPath,
        string ipAddress,
        string userAgent)
    {
        return new SystemOperationLog(entityType, entityId, actionType, changeJson, operatorUserId, operatorName, requestPath, ipAddress, userAgent);
    }
}
