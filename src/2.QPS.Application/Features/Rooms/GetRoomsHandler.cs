using MediatR;
using QPS.Application.Contracts.Rooms;
using QPS.Application.Interfaces;
using QPS.Domain.Aggregates.RoomAggregate;
using Microsoft.EntityFrameworkCore;

namespace QPS.Application.Features.Rooms;

public class GetRoomsQuery : IRequest<List<RoomDto>>
{}

public class GetRoomsHandler : IRequestHandler<GetRoomsQuery, List<RoomDto>>
{
    private readonly IDbContext _dbContext;
    private readonly ITenantService _tenantService;

    public GetRoomsHandler(IDbContext dbContext, ITenantService tenantService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
    }

    public async Task<List<RoomDto>> Handle(GetRoomsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantService.GetCurrentTenantId();
        var rooms = await _dbContext.Rooms
            .Where(r => r.MerchantId == tenantId)
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