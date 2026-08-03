namespace QPS.Application.Contracts.Crm;

public class CrmFollowRecordDto
{
    public Guid Id { get; set; }

    public Guid? HerbBaseSubjectId { get; set; }

    public Guid? HerbBaseId { get; set; }

    public Guid? ContactId { get; set; }

    public string? ContactName { get; set; }

    public string FollowType { get; set; } = string.Empty;

    public string FollowResult { get; set; } = string.Empty;

    public string IntentLevel { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateTime? NextFollowAt { get; set; }

    public Guid? OperatorUserId { get; set; }

    public DateTime CreatedAt { get; set; }
}


