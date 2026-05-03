using MediatR;
using QPS.Application.Contracts.Statistics;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;

namespace QPS.Application.Features.Statistics;

public class GetOrderTrendQuery : IRequest<List<OrderTrendDto>>
{
    public string? TimeRange { get; set; } = "7days";
}

public class GetOrderTrendHandler : IRequestHandler<GetOrderTrendQuery, List<OrderTrendDto>>
{
    private readonly IDbContext _dbContext;

    public GetOrderTrendHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<OrderTrendDto>> Handle(GetOrderTrendQuery request, CancellationToken cancellationToken)
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

        var dateLabels = Enumerable.Range(0, days)
            .Select(i => DateTime.UtcNow.AddDays(-i).Date)
            .Reverse()
            .ToList();

        var orderCounts = _dbContext.Orders
            .Where(o => o.PaidAt.HasValue && o.PaidAt.Value >= startDate && o.PaidAt.Value < endDate.AddDays(1))
            .GroupBy(o => o.PaidAt.Value.Date)
            .ToDictionary(g => g.Key, g => g.Count());

        var revenueByDate = _dbContext.Orders
            .Where(o => o.Status == OrderStatus.Completed && o.PaidAt.HasValue && o.PaidAt.Value >= startDate && o.PaidAt.Value < endDate.AddDays(1))
            .Select(o => new { o.PaidAt.Value.Date, o.ActualAmount })
            .AsEnumerable()
            .GroupBy(o => o.Date)
            .ToDictionary(g => g.Key, g => g.Sum(o => o.ActualAmount));

        var result = dateLabels.Select(date => new OrderTrendDto
        {
            Date = date.ToString("yyyy-MM-dd"),
            OrderCount = orderCounts.TryGetValue(date, out var count) ? count : 0,
            Revenue = revenueByDate.TryGetValue(date, out var revenue) ? revenue : 0
        }).ToList();

        return Task.FromResult(result);
    }
}