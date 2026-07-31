namespace QPS.Application.Contracts.Crm;

public class CrmFollowRecordCreateRequest
{
    public Guid? ContactId { get; set; }

    public string FollowType { get; set; } = "PHONE";

    public string FollowResult { get; set; } = string.Empty;

    public string IntentLevel { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateTime? NextFollowAt { get; set; }
}


