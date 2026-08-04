using QPS.Domain.Common;

namespace QPS.Domain.Entities.Crm;

public class CrmHerbBaseSubject : BaseEntity
{
    private const string PendingStatus = "PENDING";
    private const string FollowingStatus = "FOLLOWING";
    private const string InterestedStatus = "INTERESTED";

    public string? SubjectName { get; private set; }
    public string SubjectType { get; private set; } = "UNKNOWN";
    public Guid? OwnerUserId { get; private set; }
    public string Status { get; private set; } = "PENDING";
    public string Grade { get; private set; } = string.Empty;
    public int Score { get; private set; }
    public string? PrimaryContactName { get; private set; }
    public string? PrimaryContactPhone { get; private set; }
    public DateTime? LastFollowAt { get; private set; }
    public string? LastFollowResult { get; private set; }
    public DateTime? NextFollowAt { get; private set; }
    public string? Remark { get; private set; }
    public ICollection<CrmHerbBase> HerbBases { get; private set; } = new List<CrmHerbBase>();
    public ICollection<CrmFollowRecord> FollowRecords { get; private set; } = new List<CrmFollowRecord>();

    private CrmHerbBaseSubject() { }

    private CrmHerbBaseSubject(string subjectName, string baseName, string subjectType, Guid? ownerUserId, string status, string grade, int score, string remark)
    {
        SubjectName = string.IsNullOrWhiteSpace(subjectName) ? baseName.Trim() : subjectName.Trim();
        SubjectType = subjectType;
        OwnerUserId = ownerUserId;
        Status = status;
        Grade = grade;
        Score = score;
        Remark = remark;
    }

    public static CrmHerbBaseSubject Create(string subjectName, string baseName, string subjectType, Guid? ownerUserId, string status, string grade, int score, string remark)
        => new(subjectName, baseName, subjectType, ownerUserId, status, grade, score, remark);

    public void AssignOwner(Guid? ownerUserId)
    {
        OwnerUserId = ownerUserId;
    }

    public void UpdateBasicInfo(string subjectName, string subjectType, string status, string grade, int score, string remark)
    {
        SubjectName = string.IsNullOrWhiteSpace(subjectName) ? SubjectName : subjectName.Trim();
        SubjectType = string.IsNullOrWhiteSpace(subjectType) ? SubjectType : subjectType;
        Status = string.IsNullOrWhiteSpace(status) ? Status : status;
        Grade = string.IsNullOrWhiteSpace(grade) ? Grade : grade;
        Score = score;
        Remark = remark;
    }

    public void UpdatePrimaryContact(string contactName, string phone)
    {
        PrimaryContactName = contactName;
        PrimaryContactPhone = phone;
    }

    public void ClearPrimaryContact()
    {
        PrimaryContactName = null;
        PrimaryContactPhone = null;
    }

    public void UpdateFollowSummary(DateTime followAt, string followResult, DateTime? nextFollowAt)
    {
        LastFollowAt = followAt;
        LastFollowResult = followResult;
        NextFollowAt = nextFollowAt;

        if (followResult == InterestedStatus || followResult == "有意向")
        {
            Status = InterestedStatus;
        }
        else if (Status == PendingStatus)
        {
            Status = FollowingStatus;
        }
    }
}
