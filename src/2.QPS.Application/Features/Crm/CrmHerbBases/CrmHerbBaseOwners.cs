using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Interfaces;

namespace QPS.Application.Features.Crm.CrmHerbBases;

internal static class CrmHerbBaseOwners
{
    public static async Task FillAsync(IDbContext dbContext, List<CrmHerbBaseDto> customers, CancellationToken cancellationToken)
    {
        var ownerIds = customers
            .Where(customer => customer.OwnerUserId.HasValue)
            .Select(customer => customer.OwnerUserId!.Value)
            .Distinct()
            .ToList();

        if (ownerIds.Count == 0)
        {
            return;
        }

        var ownerLookup = await dbContext.SystemUsers
            .Where(user => ownerIds.Contains(user.Id))
            .ToDictionaryAsync(
                user => user.Id,
                user => string.IsNullOrWhiteSpace(user.RealName) ? user.Username : user.RealName,
                cancellationToken);

        foreach (var customer in customers)
        {
            if (customer.OwnerUserId.HasValue &&
                ownerLookup.TryGetValue(customer.OwnerUserId.Value, out var ownerUserName))
            {
                customer.OwnerUserName = ownerUserName;
            }
        }
    }
}



