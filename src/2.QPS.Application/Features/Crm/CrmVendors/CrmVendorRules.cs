namespace QPS.Application.Features.Crm.CrmVendors;

public static class CrmVendorRules
{

    public static string NormalizeVendorName(string vendorName)
    {
        return string.Concat(vendorName.Where(c => !char.IsWhiteSpace(c))).ToUpperInvariant();
    }

    public static string NormalizePriority(string? priorityLevel)
    {
        return priorityLevel is "High" or "Medium" or "Low" ? priorityLevel : "Medium";
    }
}
