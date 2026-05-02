using MediatR;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.RoomPlans;

public class DeleteRoomPlanCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}

public class DeleteRoomPlanHandler : IRequestHandler<DeleteRoomPlanCommand, bool>
{
    private readonly IDbContext _dbContext;

    public DeleteRoomPlanHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(DeleteRoomPlanCommand request, CancellationToken cancellationToken)
    {
        var mapping = await _dbContext.RoomPlans.FindAsync(request.Id, cancellationToken);

        if (mapping == null)
        {
            throw new BusinessException(404, "关联不存在");
        }

        _dbContext.RoomPlans.Remove(mapping);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}