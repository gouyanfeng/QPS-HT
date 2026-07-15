using MediatR;
using QPS.Application.Contracts.Qps.Statistics;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;
using QPS.Domain.Entities.Qps;
namespace QPS.Application.Features.Qps.Statistics;

public class GetRoomStatusQuery : IRequest<List<RoomStatusDto>>
{
}

public class GetRoomStatusHandler : IRequestHandler<GetRoomStatusQuery, List<RoomStatusDto>>
{
    private readonly IDbContext _dbContext;

    public GetRoomStatusHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<RoomStatusDto>> Handle(GetRoomStatusQuery request, CancellationToken cancellationToken)
    {
        var statusCounts = _dbContext.Rooms
            .GroupBy(r => r.Status)
            .ToDictionary(g => g.Key, g => g.Count());

        var totalRooms = _dbContext.Rooms.Count();
        var statuses = Enum.GetValues<RoomStatus>();

        var result = statuses.Select(status => new RoomStatusDto
        {
            Status = status.ToString(),
            Count = statusCounts.TryGetValue(status, out var count) ? count : 0,
            Percentage = totalRooms > 0 ? Math.Round((decimal)(statusCounts.TryGetValue(status, out var c) ? c : 0) / totalRooms * 100, 2) : 0
        }).ToList();

        return Task.FromResult(result);
    }
}