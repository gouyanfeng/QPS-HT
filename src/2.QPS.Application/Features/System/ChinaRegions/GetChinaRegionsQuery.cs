using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.System.ChinaRegions;
using QPS.Application.Interfaces;

namespace QPS.Application.Features.System.ChinaRegions;

public class GetChinaRegionsQuery : IRequest<List<ChinaRegionDto>>
{
    public bool ActiveOnly { get; set; } = true;
}

public class GetChinaRegionsQueryHandler : IRequestHandler<GetChinaRegionsQuery, List<ChinaRegionDto>>
{
    private readonly IDbContext _dbContext;

    public GetChinaRegionsQueryHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ChinaRegionDto>> Handle(GetChinaRegionsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.SystemChinaRegions
            .AsNoTracking()
            .Where(region => !region.IsDeleted);

        if (request.ActiveOnly)
        {
            query = query.Where(region => region.IsActive);
        }

        return await query
            .OrderBy(region => region.Level)
            .ThenBy(region => region.SortOrder)
            .ThenBy(region => region.Code)
            .Select(region => new ChinaRegionDto
            {
                Code = region.Code,
                Name = region.Name,
                FullName = region.FullName,
                Level = region.Level,
                ParentCode = region.ParentCode,
                ProvinceCode = region.ProvinceCode,
                CityCode = region.CityCode,
                SortOrder = region.SortOrder,
                IsActive = region.IsActive
            })
            .ToListAsync(cancellationToken);
    }
}


