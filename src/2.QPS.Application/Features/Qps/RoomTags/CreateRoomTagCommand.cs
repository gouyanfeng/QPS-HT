using MediatR;
using QPS.Application.Contracts.Qps.RoomTags;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;
using QPS.Domain.Entities.Qps;using QPS.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace QPS.Application.Features.Qps.RoomTags;

public class CreateRoomTagCommand : IRequest<RoomTagDto>
{
    public Guid RoomId { get; set; }
    public Guid TagId { get; set; }
}

public class CreateRoomTagHandler : IRequestHandler<CreateRoomTagCommand, RoomTagDto>
{
    private readonly IDbContext _dbContext;

    public CreateRoomTagHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RoomTagDto> Handle(CreateRoomTagCommand request, CancellationToken cancellationToken)
    {
        var roomExists = await _dbContext.Rooms.AnyAsync(r => r.Id == request.RoomId, cancellationToken);
        if (!roomExists)
        {
            throw new BusinessException(404, "房间不存在");
        }

        var tagExists = await _dbContext.Tags.AnyAsync(t => t.Id == request.TagId, cancellationToken);
        if (!tagExists)
        {
            throw new BusinessException(404, "标签不存在");
        }

        var existingMapping = await _dbContext.RoomTags
            .AnyAsync(m => m.RoomId == request.RoomId && m.TagId == request.TagId, cancellationToken);
        if (existingMapping)
        {
            throw new BusinessException(400, "该房间已关联此标签");
        }

        var mapping = new RoomTag(request.RoomId, request.TagId);

        _dbContext.RoomTags.Add(mapping);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var room = await _dbContext.Rooms.FirstOrDefaultAsync(r => r.Id == request.RoomId, cancellationToken);
        var tag = await _dbContext.Tags.FirstOrDefaultAsync(t => t.Id == request.TagId, cancellationToken);

        return new RoomTagDto
        {
            Id = mapping.Id,
            RoomId = mapping.RoomId,
            RoomNumber = room != null ? room.Name : null,
            TagId = mapping.TagId,
            TagName = tag != null ? tag.TagName : null
        };
    }
}