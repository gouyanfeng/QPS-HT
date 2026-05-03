using MediatR;
using QPS.Application.Contracts.Statistics;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;

namespace QPS.Application.Features.Statistics;

public class GetStatisticsQuery : IRequest<StatisticsDto>
{
    public string? TimeRange { get; set; } = "7days";
}

public class GetStatisticsHandler : IRequestHandler<GetStatisticsQuery, StatisticsDto>
{
    private readonly IDbContext _dbContext;

    public GetStatisticsHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<StatisticsDto> Handle(GetStatisticsQuery request, CancellationToken cancellationToken)
    {
        var days = request.TimeRange switch
        {
            "7days" => 7,
            "30days" => 30,
            "90days" => 90,
            _ => 7
        };

        var startDate = DateTime.UtcNow.AddDays(-days).Date;
        var endDate = DateTime.UtcNow.Date;

        var summary = GetSummaryData(startDate, endDate);

        return Task.FromResult(new StatisticsDto
        {
            Summary = summary
        });
    }

    private SummaryData GetSummaryData(DateTime startDate, DateTime endDate)
    {
        var totalOrders = _dbContext.Orders
            .Where(o => o.PaidAt.HasValue && o.PaidAt.Value >= startDate && o.PaidAt.Value < endDate.AddDays(1))
            .Count();

        var totalRevenue = _dbContext.Orders
            .Where(o => o.Status == OrderStatus.Completed && o.PaidAt.HasValue && o.PaidAt.Value >= startDate && o.PaidAt.Value < endDate.AddDays(1))
            .Select(o => o.ActualAmount)
            .AsEnumerable()
            .Sum();

        var totalRooms = _dbContext.Rooms.Count();

        var availableRooms = _dbContext.Rooms
            .Where(r => r.Status == RoomStatus.Idle && r.IsEnabled)
            .Count();

        return new SummaryData
        {
            TotalOrders = totalOrders,
            TotalRevenue = totalRevenue,
            TotalRooms = totalRooms,
            AvailableRooms = availableRooms
        };
    }
}