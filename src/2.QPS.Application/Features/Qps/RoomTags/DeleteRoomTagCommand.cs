using MediatR;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;
using QPS.Domain.Entities.Qps;using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Qps.RoomTags;

public class DeleteRoomTagCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}

public class DeleteRoomTagHandler : IRequestHandler<DeleteRoomTagCommand, bool>
{
    private readonly IDbContext _dbContext;

    public DeleteRoomTagHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(DeleteRoomTagCommand request, CancellationToken cancellationToken)
    {
        var mapping = await _dbContext.RoomTags.FindAsync(request.Id, cancellationToken);
        if (mapping == null)
        {
            throw new BusinessException(404, "关联记录不存在");
        }

        _dbContext.RoomTags.Remove(mapping);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}