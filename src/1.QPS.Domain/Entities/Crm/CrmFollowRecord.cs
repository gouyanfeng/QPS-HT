using QPS.Domain.Common;

namespace QPS.Domain.Entities.Crm;

public class CrmFollowRecord : BaseEntity
{
    public string EntityType { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    public Guid? ContactId { get; private set; }
    public string FollowType { get; private set; } = string.Empty;
    public string FollowResult { get; private set; } = string.Empty;
    public string IntentLevel { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public DateTime? NextFollowAt { get; private set; }
    public Guid? OperatorUserId { get; private set; }
    public virtual CrmContact? Contact { get; private set; }

    private CrmFollowRecord() { }

    private CrmFollowRecord(
        string entityType,
        Guid entityId,
        Guid? contactId,
        string followType,
        string followResult,
        string intentLevel,
        string content,
        DateTime? nextFollowAt,
        Guid? operatorUserId)
    {
        EntityType = entityType;
        EntityId = entityId;
        ContactId = contactId;
        FollowType = followType;
        FollowResult = followResult;
        IntentLevel = intentLevel;
        Content = content;
        NextFollowAt = nextFollowAt;
        OperatorUserId = operatorUserId;
    }

    public static CrmFollowRecord Create(
        string entityType,
        Guid entityId,
        Guid? contactId,
        string followType,
        string followResult,
        string intentLevel,
        string content,
        DateTime? nextFollowAt,
        Guid? operatorUserId)
    {
        return new CrmFollowRecord(
            entityType,
            entityId,
            contactId,
            followType,
            followResult,
            intentLevel,
            content,
            nextFollowAt,
            operatorUserId);
    }

    public void UpdateResult(
        string followResult,
        string intentLevel,
        string content,
        DateTime? nextFollowAt)
    {
        FollowResult = followResult;
        IntentLevel = intentLevel;
        Content = content;
        NextFollowAt = nextFollowAt;
    }
}
