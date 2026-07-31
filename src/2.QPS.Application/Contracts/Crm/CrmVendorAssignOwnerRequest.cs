namespace QPS.Application.Contracts.Crm;

public class CrmVendorAssignOwnerRequest
{
    public List<Guid> VendorIds { get; set; } = new();

    public Guid? OwnerUserId { get; set; }

    public string? Remark { get; set; }
}
