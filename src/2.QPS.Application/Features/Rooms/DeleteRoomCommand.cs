using MediatR;
using QPS.Application.Interfaces;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Rooms;

public class DeleteRoomCommand : IRequest<bool>
{
    /// <summary>
    /// 房间ID
    /// </summary>
    public Guid Id { get; set; }
}

public class DeleteRoomHandler : IRequestHandler<DeleteRoomCommand, bool>
{
    private readonly IDbContext _dbContext;

    public DeleteRoomHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(DeleteRoomCommand request, CancellationToken cancellationToken)
    {
        var room = await _dbContext.Rooms.FindAsync(request.Id, cancellationToken);

        if (room == null)
        {
            throw new BusinessException(404, "房间不存在");
        }

        _dbContext.Rooms.Remove(room);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}