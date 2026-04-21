using MediatR;
using QPS.Application.Contracts.Rooms;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace QPS.Application.Features.Rooms;

public class GetRoomsQuery : IRequest<List<RoomDto>>
{}

public class GetRoomsHandler : IRequestHandler<GetRoomsQuery, List<RoomDto>>
{
    private readonly IDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetRoomsHandler(IDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<List<RoomDto>> Handle(GetRoomsQuery request, CancellationToken cancellationToken)
    {
        var merchantId = _currentUserService.MerchantId;
        var rooms = await _dbContext.Rooms
            .Where(r => r.MerchantId == merchantId)
            .Select(r => new RoomDto
            {
                Id = r.Id,
                RoomNumber = r.Name,
                Status = r.Status.ToString(),
                MqttTopic = r.MqttTopic,
                ShopId = r.ShopId,
                DeviceSn = r.DeviceSn
            })
            .ToListAsync(cancellationToken);

        return rooms;
    }
}