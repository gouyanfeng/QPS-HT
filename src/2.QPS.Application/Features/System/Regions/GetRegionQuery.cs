using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.System.Regions;
using QPS.Application.Interfaces;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.System.Regions;

public record GetRegionQuery : IRequest<RegionDto>
{
    public Guid Id { get; set; }
}

public class GetRegionQueryHandler : IRequestHandler<GetRegionQuery, RegionDto>
{
    private readonly IDbContext _dbContext;

    public GetRegionQueryHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RegionDto> Handle(GetRegionQuery request, CancellationToken cancellationToken)
    {
        var region = await _dbContext.SystemRegions
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (region == null)
        {
            throw new BusinessException(404, "Region does not exist.");
        }

        string? parentName = null;
        if (region.ParentId.HasValue)
        {
            parentName = await _dbContext.SystemRegions
                .Where(r => r.Id == region.ParentId.Value)
                .Select(r => r.Name)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return RegionMapper.ToDto(region, parentName);
    }
}


