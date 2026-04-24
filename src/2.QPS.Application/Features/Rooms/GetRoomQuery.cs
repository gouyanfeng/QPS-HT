using MediatR;
using QPS.Application.Contracts.Rooms;
using QPS.Application.Interfaces;
using QPS.Domain.Exceptions;
using QPS.Domain.Entities;

namespace QPS.Application.Features.Rooms;

public class GetRoomQuery : IRequest<RoomDto>
{
    /// <summary>
    /// 房间ID
    /// </summary>
    public Guid Id { get; set; }
}

public class GetRoomHandler : IRequestHandler<GetRoomQuery, RoomDto>
{
    private readonly IDbContext _dbContext;

    public GetRoomHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RoomDto> Handle(GetRoomQuery request, CancellationToken cancellationToken)
    {
        var room = await _dbContext.Rooms.FindAsync(request.Id, cancellationToken);

        if (room == null)
        {
            throw new BusinessException(404, "房间不存在");
        }

        return new RoomDto
        {
            Id = room.Id,
            RoomNumber = room.Name,
            Status = room.Status.ToString(),
            ShopId = room.ShopId,
            UnitPrice = room.UnitPrice,
            IsEnabled = room.IsEnabled
        };
    }
}