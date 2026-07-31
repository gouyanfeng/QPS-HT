using QPS.Application.Contracts.System.Regions;
using QPS.Domain.Entities.System;

namespace QPS.Application.Features.System.Regions;

internal static class RegionMapper
{
    public static RegionDto ToDto(SystemRegion region, string? parentName = null)
    {
        return new RegionDto
        {
            Id = region.Id,
            ParentId = region.ParentId,
            ParentName = parentName,
            Code = region.Code,
            Name = region.Name,
            Level = region.Level,
            SortOrder = region.SortOrder,
            IsActive = region.IsActive
        };
    }
}


