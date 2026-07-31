namespace QPS.Application.Contracts.Crm;

public class CrmHerbBaseAssignOwnerRequest
{
    public List<Guid> HerbBaseIds { get; set; } = new();

    public Guid? OwnerUserId { get; set; }

    public string? Remark { get; set; }
}



