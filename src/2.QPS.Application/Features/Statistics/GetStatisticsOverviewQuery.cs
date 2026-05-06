using MediatR;
using QPS.Application.Contracts.Statistics;
using QPS.Application.Extensions;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;

namespace QPS.Application.Features.Statistics;

public class GetStatisticsOverviewQuery : IRequest<StatisticsOverviewDto>
{
    public string? TimeRange { get; set; } = "7days";
}

public class GetStatisticsRealtimeQuery : IRequest<StatisticsRealtimeDto>
{
}

public class GetStatisticsOverviewHandler : IRequestHandler<GetStatisticsOverviewQuery, StatisticsOverviewDto>
{
    private readonly IDbContext _dbContext;

    public GetStatisticsOverviewHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<StatisticsOverviewDto> Handle(GetStatisticsOverviewQuery request, CancellationToken cancellationToken)
    {
        var days = request.TimeRange switch
        {
            "7days" => 7,
            "30days" => 30,
            "90days" => 90,
            _ => 7
        };

        var endDate = DateTime.UtcNow.Date;
        var startDate = endDate.AddDays(-days + 1);
        var previousStartDate = startDate.AddDays(-days);
        var previousEndDate = startDate.AddDays(-1);

        var ordersInRange = _dbContext.Orders
            .Where(o => o.CreatedAt.Date >= startDate && o.CreatedAt.Date <= endDate)
            .ToList();

        var completedOrdersInRange = ordersInRange
            .Where(o => o.Status == OrderStatus.Paid)
            .ToList();

        var totalOrders = ordersInRange.Count;
        var completedOrders = completedOrdersInRange.Count;

        // 收入统计基于支付时间，确保与 previousRevenue 计算一致
        var totalRevenue = _dbContext.Orders
            .Where(o => o.Status == OrderStatus.Paid && o.PaidAt.Value.Date >= startDate && o.PaidAt.Value.Date <= endDate)
            .Select(o => o.ActualAmount)
            .AsEnumerable()
            .Sum();
        var averageOrderValue = completedOrders > 0
            ? Math.Round(totalRevenue / completedOrders, 2)
            : 0m;

        var previousRevenue = _dbContext.Orders
            .Where(o => o.Status == OrderStatus.Paid && o.PaidAt.Value.Date >= previousStartDate && o.PaidAt.Value.Date <= previousEndDate)
            .Select(o => o.ActualAmount)
            .AsEnumerable()
            .Sum();

        var revenueGrowthRate = previousRevenue > 0
            ? Math.Round((totalRevenue - previousRevenue) / previousRevenue * 100, 2)
            : 0m;

        var dateLabels = Enumerable.Range(0, days)
            .Select(i => startDate.AddDays(i))
            .ToArray();

        var orderCountByDate = ordersInRange
            .GroupBy(o => o.CreatedAt.Date)
            .ToDictionary(g => g.Key, g => g.Count());

        // 收入趋势基于支付时间
        var revenueByDate = _dbContext.Orders
            .Where(o => o.Status == OrderStatus.Paid && o.PaidAt.HasValue && o.PaidAt.Value.Date >= startDate && o.PaidAt.Value.Date <= endDate)
            .AsEnumerable()
            .GroupBy(o => o.PaidAt.Value.Date)
            .ToDictionary(g => g.Key, g => g.Sum(o => o.ActualAmount));

        var orderTrend = dateLabels.Select(date => orderCountByDate.TryGetValue(date, out var count) ? (decimal)count : 0m).ToArray();
        var revenueTrend = dateLabels.Select(date => revenueByDate.TryGetValue(date, out var revenue) ? revenue : 0m).ToArray();

        var topShops = _dbContext.Orders
            .Where(o => o.Status == OrderStatus.Paid && o.PaidAt.HasValue && o.PaidAt.Value.Date >= startDate && o.PaidAt.Value.Date <= endDate)
            .AsEnumerable()
            .GroupBy(o => o.ShopId)
            .Select(g => new { ShopId = g.Key, Revenue = g.Sum(o => o.ActualAmount) })
            .OrderByDescending(g => g.Revenue)
            .Take(5)
            .ToList();

        var topShopRevenue = topShops
            .Join(_dbContext.Shops, shopRevenue => shopRevenue.ShopId, shop => shop.Id, (shopRevenue, shop) => new { shop.Name, shopRevenue.Revenue })
            .ToList();

        var topShopLabels = topShopRevenue.Select(x => x.Name).ToArray();
        var topShopData = topShopRevenue.Select(x => x.Revenue).ToArray();

        var overview = new StatisticsOverviewDto
        {
            Summary = new OverviewSummaryDto
            {
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                CompletedOrders = completedOrders,
                AverageOrderValue = averageOrderValue,
                PreviousRevenue = previousRevenue,
                RevenueGrowthRate = revenueGrowthRate,
                TimeRange = request.TimeRange ?? "7days"
            },
            OrderTrend = new TrendChartDto
            {
                Labels = dateLabels.Select(d => d.ToString("yyyy-MM-dd")).ToArray(),
                Series = new[]
                {
                    new ChartSeriesDto { Name = "订单数", Data = orderTrend, Type = "bar" }
                }
            },
            RevenueTrend = new TrendChartDto
            {
                Labels = dateLabels.Select(d => d.ToString("yyyy-MM-dd")).ToArray(),
                Series = new[]
                {
                    new ChartSeriesDto { Name = "收入", Data = revenueTrend, Type = "line" }
                }
            },
            TopShopRevenue = new BarChartDto
            {
                Labels = topShopLabels,
                Data = topShopData
            }
        };

        return Task.FromResult(overview);
    }
}

public class GetStatisticsRealtimeHandler : IRequestHandler<GetStatisticsRealtimeQuery, StatisticsRealtimeDto>
{
    private readonly IDbContext _dbContext;

    public GetStatisticsRealtimeHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<StatisticsRealtimeDto> Handle(GetStatisticsRealtimeQuery request, CancellationToken cancellationToken)
    {
        var enabledRooms = _dbContext.Rooms.Where(r => r.IsEnabled);
        var allRooms = enabledRooms.Count();

        var roomStatusCounts = enabledRooms
            .GroupBy(r => r.Status)
            .ToDictionary(g => g.Key, g => g.Count());

        var roomStatusData = Enum.GetValues<RoomStatus>()
            .Select(status => new PieSliceDto
            {
                Name = status.ToChinese(),
                Value = roomStatusCounts.TryGetValue(status, out var count) ? count : 0,
                Percentage = allRooms > 0 ? Math.Round((decimal)(roomStatusCounts.TryGetValue(status, out var count2) ? count2 : 0) / allRooms * 100, 2) : 0m
            })
            .ToArray();

        var realtime = new StatisticsRealtimeDto
        {
            RoomStatusDistribution = new PieChartDto
            {
                Data = roomStatusData
            }
        };

        return Task.FromResult(realtime);
    }
}