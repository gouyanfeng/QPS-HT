using MediatR;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;

namespace QPS.Application.Features.Statistics;

public class GetRevenueSummaryQuery : IRequest<RevenueSummaryDto>
{
    public string? TimeRange { get; set; } = "7days";
}

public class RevenueSummaryDto
{
    public decimal CurrentRevenue { get; set; }
    public decimal PreviousRevenue { get; set; }
    public decimal GrowthRate { get; set; }
    public string TimeRange { get; set; } = string.Empty;
}

public class GetRevenueSummaryHandler : IRequestHandler<GetRevenueSummaryQuery, RevenueSummaryDto>
{
    private readonly IDbContext _dbContext;

    public GetRevenueSummaryHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<RevenueSummaryDto> Handle(GetRevenueSummaryQuery request, CancellationToken cancellationToken)
    {
        var days = request.TimeRange switch
        {
            "7days" => 7,
            "30days" => 30,
            "90days" => 90,
            _ => 7
        };

        var endDate = DateTime.UtcNow.Date;
        var currentStartDate = endDate.AddDays(-days);

        var currentRevenue = _dbContext.Orders
            .Where(o => o.Status == OrderStatus.Completed && o.PaidAt.HasValue && o.PaidAt.Value >= currentStartDate && o.PaidAt.Value < endDate.AddDays(1))
            .Select(o => o.ActualAmount)
            .AsEnumerable()
            .Sum();

        var previousEndDate = currentStartDate;
        var previousStartDate = previousEndDate.AddDays(-days);

        var previousRevenue = _dbContext.Orders
            .Where(o => o.Status == OrderStatus.Completed && o.PaidAt.HasValue && o.PaidAt.Value >= previousStartDate && o.PaidAt.Value < previousEndDate.AddDays(1))
            .Select(o => o.ActualAmount)
            .AsEnumerable()
            .Sum();

        var growthRate = previousRevenue > 0 ? Math.Round((currentRevenue - previousRevenue) / previousRevenue * 100, 2) : 0;

        return Task.FromResult(new RevenueSummaryDto
        {
            CurrentRevenue = currentRevenue,
            PreviousRevenue = previousRevenue,
            GrowthRate = growthRate,
            TimeRange = request.TimeRange ?? "7days"
        });
    }
}