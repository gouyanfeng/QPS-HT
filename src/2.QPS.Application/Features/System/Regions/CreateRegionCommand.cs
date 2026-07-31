using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.System.Regions;
using QPS.Application.Interfaces;
using QPS.Domain.Entities.System;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.System.Regions;

public record CreateRegionCommand : IRequest<RegionDto>
{
    public RegionCreateRequest Request { get; set; }
}

public class CreateRegionCommandHandler : IRequestHandler<CreateRegionCommand, RegionDto>
{
    private readonly IDbContext _dbContext;

    public CreateRegionCommandHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RegionDto> Handle(CreateRegionCommand request, CancellationToken cancellationToken)
    {
        await ValidateRegionAsync(request.Request.ParentId, request.Request.Code, request.Request.Level, null, cancellationToken);

        var region = new SystemRegion(
            Guid.NewGuid(),
            request.Request.ParentId,
            request.Request.Code,
            request.Request.Name,
            request.Request.Level,
            request.Request.SortOrder,
            request.Request.IsActive);

        await _dbContext.SystemRegions.AddAsync(region, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return RegionMapper.ToDto(region);
    }

    private async Task ValidateRegionAsync(Guid? parentId, string code, int level, Guid? id, CancellationToken cancellationToken)
    {
        if (level < 1 || level > 3)
        {
            throw new BusinessException(400, "Region level must be between 1 and 3.");
        }

        if (parentId.HasValue)
        {
            var parentExists = await _dbContext.SystemRegions
                .AnyAsync(r => r.Id == parentId.Value, cancellationToken);

            if (!parentExists)
            {
                throw new BusinessException(404, "Parent region does not exist.");
            }
        }

        var codeExists = await _dbContext.SystemRegions
            .AnyAsync(r => r.Code == code && (!id.HasValue || r.Id != id.Value), cancellationToken);

        if (codeExists)
        {
            throw new BusinessException(400, "Region code already exists.");
        }
    }
}


