using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Interfaces;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.System.Regions;

public record DeleteRegionCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
}

public class DeleteRegionCommandHandler : IRequestHandler<DeleteRegionCommand, Unit>
{
    private readonly IDbContext _dbContext;

    public DeleteRegionCommandHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(DeleteRegionCommand request, CancellationToken cancellationToken)
    {
        var region = await _dbContext.SystemRegions
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (region == null)
        {
            throw new BusinessException(404, "Region does not exist.");
        }

        var hasChildren = await _dbContext.SystemRegions
            .AnyAsync(r => r.ParentId == request.Id, cancellationToken);

        if (hasChildren)
        {
            throw new BusinessException(400, "Cannot delete a region with child regions.");
        }

        _dbContext.SystemRegions.Remove(region);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}


