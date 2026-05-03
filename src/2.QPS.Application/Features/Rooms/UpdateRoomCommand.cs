using MediatR;
using QPS.Application.Contracts.Rooms;
using QPS.Application.Extensions;
using QPS.Application.Interfaces;
using QPS.Domain.Exceptions;
using QPS.Domain.Entities;

namespace QPS.Application.Features.Rooms;

public class UpdateRoomCommand : IRequest<RoomDto>
{
    /// <summary>
    /// 房间ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 更新房间请求
    /// </summary>
    public RoomUpdateRequest Request { get; set; }
}

public class UpdateRoomHandler : IRequestHandler<UpdateRoomCommand, RoomDto>
{
    private readonly IDbContext _dbContext;

    public UpdateRoomHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RoomDto> Handle(UpdateRoomCommand request, CancellationToken cancellationToken)
    {
        var room = await _dbContext.Rooms.FindAsync(request.Id, cancellationToken);

        if (room == null)
        {
            throw new BusinessException(404, "房间不存在");
        }

        room.Update(
            request.Request.RoomNumber,
            request.Request.UnitPrice,
            request.Request.IsEnabled
        );

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new RoomDto
        {
            Id = room.Id,
            RoomNumber = room.Name,
            Status = room.Status.ToChinese(),
            ShopId = room.ShopId,
            UnitPrice = room.UnitPrice,
            IsEnabled = room.IsEnabled
        };
    }
}