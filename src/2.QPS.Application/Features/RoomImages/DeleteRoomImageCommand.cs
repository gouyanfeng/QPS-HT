using MediatR;
using QPS.Application.Interfaces;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.RoomImages;

public class DeleteRoomImageCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}

public class DeleteRoomImageHandler : IRequestHandler<DeleteRoomImageCommand, bool>
{
    private readonly IDbContext _dbContext;

    public DeleteRoomImageHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(DeleteRoomImageCommand request, CancellationToken cancellationToken)
    {
        var image = await _dbContext.RoomImages.FindAsync(request.Id, cancellationToken);

        if (image == null)
        {
            throw new BusinessException(404, "房间图片不存在");
        }

        _dbContext.RoomImages.Remove(image);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}