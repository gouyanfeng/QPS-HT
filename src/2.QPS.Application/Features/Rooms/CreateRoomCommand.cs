using MediatR;
using QPS.Application.Contracts.Rooms;
using QPS.Application.Extensions;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;

namespace QPS.Application.Features.Rooms;

public class CreateRoomCommand : IRequest<RoomDto>
{
    /// <summary>
    /// 创建房间请求
    /// </summary>
    public RoomCreateRequest Request { get; set; }
}

public class CreateRoomHandler : IRequestHandler<CreateRoomCommand, RoomDto>
{
    private readonly IDbContext _dbContext;

    public CreateRoomHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RoomDto> Handle(CreateRoomCommand request, CancellationToken cancellationToken)
    {
        var room = Room.Create(
            request.Request.ShopId,
            request.Request.RoomNumber,
            request.Request.UnitPrice,
            request.Request.IsEnabled
        );

        _dbContext.Rooms.Add(room);
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