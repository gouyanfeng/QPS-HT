using Microsoft.EntityFrameworkCore;
using QPS.Application.Interfaces;
using QPS.Domain.Entities.Crm;

namespace QPS.Application.Features.Crm.CrmVendors;

internal static class CrmVendorPurchasePlans
{
    public static async Task RefreshLatestAsync(
        IDbContext dbContext,
        CrmVendor vendor,
        CancellationToken cancellationToken,
        CrmVendorPurchasePlan? candidatePlan = null,
        Guid? excludedPlanId = null)
    {
        var latestPlan = await dbContext.CrmVendorPurchasePlans
            .Where(plan =>
                !plan.IsDeleted &&
                plan.VendorId == vendor.Id &&
                (!excludedPlanId.HasValue || plan.Id != excludedPlanId.Value))
            .OrderByDescending(plan => plan.PurchaseTime ?? plan.CreatedAt)
            .ThenByDescending(plan => plan.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (candidatePlan is { IsDeleted: false } &&
            candidatePlan.VendorId == vendor.Id &&
            (!excludedPlanId.HasValue || candidatePlan.Id != excludedPlanId.Value) &&
            IsLater(candidatePlan, latestPlan))
        {
            latestPlan = candidatePlan;
        }

        vendor.Update(
            vendor.VendorName,
            vendor.NormalizedVendorName,
            vendor.PriorityLevel,
            latestPlan?.PurchaseTime,
            latestPlan?.PurchasePlanName ?? string.Empty,
            vendor.Remark,
            vendor.OwnerUserId);
    }

    private static bool IsLater(CrmVendorPurchasePlan plan, CrmVendorPurchasePlan? other)
    {
        if (other == null)
        {
            return true;
        }

        var planTime = plan.PurchaseTime ?? plan.CreatedAt;
        var otherTime = other.PurchaseTime ?? other.CreatedAt;
        return planTime > otherTime || (planTime == otherTime && plan.CreatedAt > other.CreatedAt);
    }
}
