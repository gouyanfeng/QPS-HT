namespace QPS.Application.Contracts.Crm;

public class CrmHerbBaseSubjectUpdateRequest
{
    public string SubjectName { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string SubjectType { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string Grade { get; set; } = string.Empty;

    public int Score { get; set; }

    public string Remark { get; set; } = string.Empty;
}
