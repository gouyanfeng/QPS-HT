using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.System.Regions;
using QPS.Application.Interfaces;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.System.Regions;

public record UpdateRegionCommand : IRequest<RegionDto>
{
    public Guid Id { get; set; }
    public RegionUpdateRequest Request { get; set; }
}

public class UpdateRegionCommandHandler : IRequestHandler<UpdateRegionCommand, RegionDto>
{
    private readonly IDbContext _dbContext;

    public UpdateRegionCommandHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RegionDto> Handle(UpdateRegionCommand request, CancellationToken cancellationToken)
    {
        var region = await _dbContext.SystemRegions
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (region == null)
        {
            throw new BusinessException(404, "Region does not exist.");
        }

        await ValidateRegionAsync(request.Request.ParentId, request.Request.Code, request.Request.Level, request.Id, cancellationToken);

        region.Update(
            request.Request.ParentId,
            request.Request.Code,
            request.Request.Name,
            request.Request.Level,
            request.Request.SortOrder,
            request.Request.IsActive);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return RegionMapper.ToDto(region);
    }

    private async Task ValidateRegionAsync(Guid? parentId, string code, int level, Guid id, CancellationToken cancellationToken)
    {
        if (level < 1 || level > 3)
        {
            throw new BusinessException(400, "Region level must be between 1 and 3.");
        }

        if (parentId == id)
        {
            throw new BusinessException(400, "Region cannot be its own parent.");
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
            .AnyAsync(r => r.Code == code && r.Id != id, cancellationToken);

        if (codeExists)
        {
            throw new BusinessException(400, "Region code already exists.");
        }
    }
}
