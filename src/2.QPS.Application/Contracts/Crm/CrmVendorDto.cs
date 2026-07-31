namespace QPS.Application.Contracts.Crm;

public class CrmVendorDto
{
    public Guid Id { get; set; }

    public string VendorName { get; set; } = string.Empty;

    public string NormalizedVendorName { get; set; } = string.Empty;

    public string PriorityLevel { get; set; } = string.Empty;

    public DateTime? LatestPurchaseTime { get; set; }

    public string LatestPurchasePlanName { get; set; } = string.Empty;

    public string Remark { get; set; } = string.Empty;

    public Guid? OwnerUserId { get; set; }

    public string? OwnerUserName { get; set; }

    public string PrimaryContactName { get; set; } = string.Empty;

    public string PrimaryContactPhone { get; set; } = string.Empty;

    public int PurchasePlanCount { get; set; }

    public int ProductCount { get; set; }

    public int ContactCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public List<CrmContactDto> Contacts { get; set; } = new();

    public List<CrmVendorProductDto> Products { get; set; } = new();

    public List<CrmVendorPurchasePlanDto> PurchasePlans { get; set; } = new();
}

public class CrmVendorProductDto
{
    public Guid Id { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public string Remark { get; set; } = string.Empty;
}

public class CrmVendorPurchasePlanDto
{
    public Guid Id { get; set; }

    public Guid VendorId { get; set; }

    public string PurchasePlanName { get; set; } = string.Empty;

    public DateTime? PurchaseTime { get; set; }

    public string Products { get; set; } = string.Empty;

    public string PageUrl { get; set; } = string.Empty;

    public string Remark { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}


