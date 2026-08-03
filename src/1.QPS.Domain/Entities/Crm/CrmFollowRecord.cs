using QPS.Domain.Common;

namespace QPS.Domain.Entities.Crm;

/// <summary>
/// CRM客户跟进记录。
/// </summary>
public class CrmFollowRecord : BaseEntity
{
    /// <summary>
    /// 跟进所属的药材基地主体ID。
    /// </summary>
    public Guid? HerbBaseSubjectId { get; private set; }

    /// <summary>
    /// 跟进对应的具体基地ID，可为空。
    /// </summary>
    public Guid? HerbBaseId { get; private set; }

    /// <summary>
    /// 跟进联系人ID，可为空。
    /// </summary>
    public Guid? ContactId { get; private set; }

    /// <summary>
    /// 跟进方式，例如电话、微信、拜访。
    /// </summary>
    public string FollowType { get; private set; } = string.Empty;

    /// <summary>
    /// 跟进结果。
    /// </summary>
    public string FollowResult { get; private set; } = string.Empty;

    /// <summary>
    /// 意向等级。
    /// </summary>
    public string IntentLevel { get; private set; } = string.Empty;

    /// <summary>
    /// 跟进内容。
    /// </summary>
    public string Content { get; private set; } = string.Empty;

    /// <summary>
    /// 下次跟进时间。
    /// </summary>
    public DateTime? NextFollowAt { get; private set; }

    /// <summary>
    /// 跟进操作人用户ID。
    /// </summary>
    public Guid? OperatorUserId { get; private set; }

    /// <summary>
    /// 跟进所属的药材基地主体。
    /// </summary>
    public virtual CrmHerbBaseSubject? HerbBaseSubject { get; private set; }

    /// <summary>
    /// 跟进对应的具体基地。
    /// </summary>
    public virtual CrmHerbBase? HerbBase { get; private set; }

    /// <summary>
    /// 跟进联系人。
    /// </summary>
    public virtual CrmContact? Contact { get; private set; }

    private CrmFollowRecord() { }

    private CrmFollowRecord(
        Guid? herbBaseSubjectId,
        Guid? herbBaseId,
        Guid? contactId,
        string followType,
        string followResult,
        string intentLevel,
        string content,
        DateTime? nextFollowAt,
        Guid? operatorUserId)
    {
        HerbBaseSubjectId = herbBaseSubjectId;
        HerbBaseId = herbBaseId;
        ContactId = contactId;
        FollowType = followType;
        FollowResult = followResult;
        IntentLevel = intentLevel;
        Content = content;
        NextFollowAt = nextFollowAt;
        OperatorUserId = operatorUserId;
    }

    public static CrmFollowRecord Create(
        Guid? herbBaseSubjectId,
        Guid? herbBaseId,
        Guid? contactId,
        string followType,
        string followResult,
        string intentLevel,
        string content,
        DateTime? nextFollowAt,
        Guid? operatorUserId)
    {
        return new CrmFollowRecord(
            herbBaseSubjectId,
            herbBaseId,
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


