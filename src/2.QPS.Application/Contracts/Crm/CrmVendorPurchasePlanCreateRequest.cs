namespace QPS.Application.Contracts.Crm;

public class CrmVendorPurchasePlanCreateRequest
{
    public string PurchasePlanName { get; set; } = string.Empty;

    public DateTime? PurchaseTime { get; set; }

    public string Products { get; set; } = string.Empty;

    public string PageUrl { get; set; } = string.Empty;

    public string Remark { get; set; } = string.Empty;
}
