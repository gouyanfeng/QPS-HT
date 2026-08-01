using QPS.Domain.Common;

namespace QPS.Domain.Entities.Crm;

public class CrmTransferRecord : BaseEntity
{
    public string EntityType { get; private set; } = string.Empty;

    public Guid EntityId { get; private set; }

    public Guid? FromOwnerUserId { get; private set; }

    public Guid? ToOwnerUserId { get; private set; }

    public Guid? OperatorUserId { get; private set; }

    public string Remark { get; private set; } = string.Empty;

    private CrmTransferRecord()
    {
    }

    private CrmTransferRecord(
        string entityType,
        Guid entityId,
        Guid? fromOwnerUserId,
        Guid? toOwnerUserId,
        Guid? operatorUserId,
        string remark)
    {
        EntityType = entityType;
        EntityId = entityId;
        FromOwnerUserId = fromOwnerUserId;
        ToOwnerUserId = toOwnerUserId;
        OperatorUserId = operatorUserId;
        Remark = remark;
    }

    public static CrmTransferRecord Create(
        string entityType,
        Guid entityId,
        Guid? fromOwnerUserId,
        Guid? toOwnerUserId,
        Guid? operatorUserId,
        string remark)
    {
        return new CrmTransferRecord(
            entityType,
            entityId,
            fromOwnerUserId,
            toOwnerUserId,
            operatorUserId,
            remark);
    }
}




