using QPS.Domain.Common;

namespace QPS.Domain.Entities.Crm;

/// <summary>
/// CRM厂商采购计划，一次采购计划一行。
/// </summary>
public class CrmVendorPurchasePlan : BaseEntity
{
    /// <summary>
    /// 厂商ID。
    /// </summary>
    public Guid VendorId { get; private set; }

    /// <summary>
    /// 采购计划名称。
    /// </summary>
    public string PurchasePlanName { get; private set; } = string.Empty;

    /// <summary>
    /// 采购时间。
    /// </summary>
    public DateTime? PurchaseTime { get; private set; }

    /// <summary>
    /// 本次采购品类和数量摘要。
    /// </summary>
    public string Products { get; private set; } = string.Empty;

    /// <summary>
    /// 来源网页地址。
    /// </summary>
    public string PageUrl { get; private set; } = string.Empty;

    /// <summary>
    /// 备注。
    /// </summary>
    public string Remark { get; private set; } = string.Empty;

    public virtual CrmVendor? Vendor { get; private set; }

    private CrmVendorPurchasePlan()
    {
    }

    private CrmVendorPurchasePlan(
        Guid vendorId,
        string purchasePlanName,
        DateTime? purchaseTime,
        string products,
        string pageUrl,
        string remark)
    {
        VendorId = vendorId;
        PurchasePlanName = purchasePlanName;
        PurchaseTime = purchaseTime;
        Products = products;
        PageUrl = pageUrl;
        Remark = remark;
    }

    public static CrmVendorPurchasePlan Create(
        Guid vendorId,
        string purchasePlanName,
        DateTime? purchaseTime,
        string products,
        string pageUrl,
        string remark)
    {
        return new CrmVendorPurchasePlan(vendorId, purchasePlanName, purchaseTime, products, pageUrl, remark);
    }

    public void Update(
        string purchasePlanName,
        DateTime? purchaseTime,
        string products,
        string pageUrl,
        string remark)
    {
        PurchasePlanName = purchasePlanName;
        PurchaseTime = purchaseTime;
        Products = products;
        PageUrl = pageUrl;
        Remark = remark;
    }
}


