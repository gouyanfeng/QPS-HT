namespace QPS.Application.Contracts.Crm;

public class CrmHerbBaseSubjectDto
{
    public Guid Id { get; set; }
    public string? SubjectName { get; set; }
    public string SubjectType { get; set; } = string.Empty;
    public Guid? OwnerUserId { get; set; }
    public string? OwnerUserName { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Grade { get; set; } = string.Empty;
    public int Score { get; set; }
    public string? PrimaryContactName { get; set; }
    public string? PrimaryContactPhone { get; set; }
    public DateTime? LastFollowAt { get; set; }
    public string? LastFollowResult { get; set; }
    public DateTime? NextFollowAt { get; set; }
    public string? Remark { get; set; }
    public int BaseCount { get; set; }
    public decimal TotalScale { get; set; }
    public List<string> MainProducts { get; set; } = new();
    public List<string> Regions { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CrmHerbBaseSubjectDetailDto : CrmHerbBaseSubjectDto
{
    public List<CrmHerbBaseDto> HerbBases { get; set; } = new();
    public List<CrmContactDto> Contacts { get; set; } = new();
    public List<CrmFollowRecordDto> FollowRecords { get; set; } = new();
    public List<CrmTransferRecordDto> TransferRecords { get; set; } = new();
}
