namespace QPS.Application.Contracts.Crm;

public class CrmFollowRecordCreateRequest
{
    public Guid? ContactId { get; set; }

    public string FollowType { get; set; } = "电话";

    public string FollowResult { get; set; } = string.Empty;

    public string IntentLevel { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateTime? NextFollowAt { get; set; }
}
