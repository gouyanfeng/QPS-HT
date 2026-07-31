using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Interfaces;

namespace QPS.Application.Features.Crm.CrmVendors;

public static class CrmVendorOwners
{
    public static async Task FillAsync(IDbContext dbContext, List<CrmVendorDto> vendors, CancellationToken cancellationToken)
    {
        var ownerIds = vendors
            .Where(vendor => vendor.OwnerUserId.HasValue)
            .Select(vendor => vendor.OwnerUserId!.Value)
            .Distinct()
            .ToList();

        if (ownerIds.Count == 0)
        {
            return;
        }

        var ownerLookup = await dbContext.SystemUsers
            .AsNoTracking()
            .Where(user => ownerIds.Contains(user.Id))
            .ToDictionaryAsync(user => user.Id, user => user.RealName, cancellationToken);

        foreach (var vendor in vendors)
        {
            if (vendor.OwnerUserId.HasValue &&
                ownerLookup.TryGetValue(vendor.OwnerUserId.Value, out var ownerUserName))
            {
                vendor.OwnerUserName = ownerUserName;
            }
        }
    }
}
