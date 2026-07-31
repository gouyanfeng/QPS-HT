using QPS.Domain.Common;

namespace QPS.Domain.Entities.Crm;

public class CrmTransferRecord : BaseEntity
{
    public string EntityType { get; private set; } = string.Empty;

    public Guid EntityId { get; private set; }

    public Guid? FromOwnerUserId { get; private set; }

    public string FromOwnerUserName { get; private set; } = string.Empty;

    public Guid? ToOwnerUserId { get; private set; }

    public string ToOwnerUserName { get; private set; } = string.Empty;

    public Guid? OperatorUserId { get; private set; }

    public string OperatorUserName { get; private set; } = string.Empty;

    public string Remark { get; private set; } = string.Empty;

    private CrmTransferRecord()
    {
    }

    private CrmTransferRecord(
        string entityType,
        Guid entityId,
        Guid? fromOwnerUserId,
        string fromOwnerUserName,
        Guid? toOwnerUserId,
        string toOwnerUserName,
        Guid? operatorUserId,
        string operatorUserName,
        string remark)
    {
        EntityType = entityType;
        EntityId = entityId;
        FromOwnerUserId = fromOwnerUserId;
        FromOwnerUserName = fromOwnerUserName;
        ToOwnerUserId = toOwnerUserId;
        ToOwnerUserName = toOwnerUserName;
        OperatorUserId = operatorUserId;
        OperatorUserName = operatorUserName;
        Remark = remark;
    }

    public static CrmTransferRecord Create(
        string entityType,
        Guid entityId,
        Guid? fromOwnerUserId,
        string fromOwnerUserName,
        Guid? toOwnerUserId,
        string toOwnerUserName,
        Guid? operatorUserId,
        string operatorUserName,
        string remark)
    {
        return new CrmTransferRecord(
            entityType,
            entityId,
            fromOwnerUserId,
            fromOwnerUserName,
            toOwnerUserId,
            toOwnerUserName,
            operatorUserId,
            operatorUserName,
            remark);
    }
}




