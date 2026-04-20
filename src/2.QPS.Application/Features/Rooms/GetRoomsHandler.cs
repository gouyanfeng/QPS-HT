using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Rooms;
using QPS.Application.Interfaces;
using QPS.Domain.Aggregates.RoomAggregate;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QPS.Application.Features.Rooms;

public class GetRoomsQuery : IRequest<List<RoomDto>> { }

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
            .ToListAsync(cancellationToken);

        return rooms.Select(r => new RoomDto
        {
            Id = r.Id,
            RoomNumber = r.RoomNumber,
            Status = r.Status.ToString(),
            MqttTopic = r.DeviceConfig.MqttTopic
        }).ToList();
    }
}