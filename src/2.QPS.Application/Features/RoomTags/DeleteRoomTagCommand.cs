using MediatR;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.RoomTags;

public class DeleteRoomTagCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}

public class DeleteRoomTagHandler : IRequestHandler<DeleteRoomTagCommand, bool>
{
    private readonly IDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public DeleteRoomTagHandler(IDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(DeleteRoomTagCommand request, CancellationToken cancellationToken)
    {
        var mapping = await _dbContext.RoomTags.FindAsync(request.Id, cancellationToken);
        if (mapping == null)
        {
            throw new BusinessException(404, "关联记录不存在");
        }

        if (mapping.MerchantId != _currentUserService.MerchantId)
        {
            throw new BusinessException(403, "无权操作");
        }

        _dbContext.RoomTags.Remove(mapping);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}