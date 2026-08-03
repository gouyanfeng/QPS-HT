namespace QPS.Application.Contracts.Crm;

public class CrmHerbBaseSubjectAssignOwnerRequest
{
    public List<Guid> HerbBaseSubjectIds { get; set; } = new();
    public Guid? OwnerUserId { get; set; }
    public string? Remark { get; set; }
}
