namespace QPS.Application.Contracts.Crm;

public class CrmVendorCreateRequest
{
    public string VendorName { get; set; } = string.Empty;

    public string PriorityLevel { get; set; } = "Medium";

    public DateTime? LatestPurchaseTime { get; set; }

    public string LatestPurchasePlanName { get; set; } = string.Empty;

    public Guid? OwnerUserId { get; set; }

    public string Remark { get; set; } = string.Empty;
}
