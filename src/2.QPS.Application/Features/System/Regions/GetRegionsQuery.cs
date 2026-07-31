using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.System.Regions;
using QPS.Application.Extensions;
using QPS.Application.Interfaces;

namespace QPS.Application.Features.System.Regions;

public class GetRegionsQuery : PaginationRequest, IRequest<PaginationResponse<RegionDto>>
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public int? Level { get; set; }
    public Guid? ParentId { get; set; }
    public bool? IsActive { get; set; }
}

public class GetRegionsQueryHandler : IRequestHandler<GetRegionsQuery, PaginationResponse<RegionDto>>
{
    private readonly IDbContext _dbContext;

    public GetRegionsQueryHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaginationResponse<RegionDto>> Handle(GetRegionsQuery request, CancellationToken cancellationToken)
    {
        var regions = await _dbContext.SystemRegions
            .AsNoTracking()
            .OrderBy(r => r.SortOrder)
            .ThenBy(r => r.Code)
            .ToListAsync(cancellationToken);

        var query = regions.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            query = query.Where(r => r.Code.Contains(request.Code));
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            query = query.Where(r => r.Name.Contains(request.Name));
        }

        if (request.Level.HasValue)
        {
            query = query.Where(r => r.Level == request.Level.Value);
        }

        if (request.ParentId.HasValue)
        {
            query = query.Where(r => r.ParentId == request.ParentId.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(r => r.IsActive == request.IsActive.Value);
        }

        var dtoQuery = query.AsEnumerable().Select(r => RegionMapper.ToDto(
            r,
            r.ParentId.HasValue ? regions.FirstOrDefault(x => x.Id == r.ParentId.Value)?.Name : null))
            .AsQueryable();

        return await dtoQuery.ToPaginationResponseAsync(request);
    }
}


