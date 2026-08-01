using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Interfaces;

namespace QPS.Application.Features.Crm;

public class GetCrmDashboardQuery : IRequest<CrmDashboardDto>;

public class GetCrmDashboardHandler : IRequestHandler<GetCrmDashboardQuery, CrmDashboardDto>
{
    private static readonly string[] ClosedStatuses = [CrmCodes.Status.Deal, CrmCodes.Status.Lost, "已成交", "已流失"];
    private static readonly string[] EffectiveFollowResults = [CrmCodes.FollowResult.Connected, CrmCodes.FollowResult.Interested, "已接通", "有意向"];

    private readonly IDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetCrmDashboardHandler(IDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<CrmDashboardDto> Handle(GetCrmDashboardQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var ownerUserId))
        {
            return BuildEmptyDashboard();
        }

        var now = DateTime.Now;
        var todayStart = now.Date;
        var tomorrowStart = todayStart.AddDays(1);
        var trendStart = todayStart.AddDays(-6);
        var myCustomers = _dbContext.CrmHerbBases
            .Where(customer => !customer.IsDeleted && customer.OwnerUserId == ownerUserId);
        var activeCustomers = myCustomers.Where(customer => !ClosedStatuses.Contains(customer.Status));

        var todayFollowCount = await activeCustomers.CountAsync(
            customer => customer.NextFollowAt >= todayStart && customer.NextFollowAt < tomorrowStart,
            cancellationToken);
        var overdueFollowCount = await activeCustomers.CountAsync(
            customer => customer.NextFollowAt.HasValue && customer.NextFollowAt.Value < now,
            cancellationToken);
        var myCustomerCount = await myCustomers.CountAsync(cancellationToken);
        var highIntentCustomerCount = await activeCustomers.CountAsync(
            customer => customer.Status == CrmCodes.Status.Interested, cancellationToken);

        var todayFollowCustomers = await activeCustomers
            .Where(customer => customer.NextFollowAt.HasValue && customer.NextFollowAt.Value < tomorrowStart)
            .OrderBy(customer => customer.NextFollowAt >= now)
            .ThenByDescending(customer => customer.Status == CrmCodes.Status.Interested)
            .ThenBy(customer => customer.NextFollowAt)
            .Take(10)
            .Select(customer => new CrmDashboardFollowCustomerDto
            {
                Id = customer.Id,
                BaseName = customer.BaseName,
                SubjectName = customer.SubjectName,
                Grade = customer.Grade,
                Province = customer.Province,
                City = customer.City,
                Area = customer.Area,
                PrimaryContactName = customer.PrimaryContactName,
                PrimaryContactPhone = customer.PrimaryContactPhone,
                LastFollowResult = customer.LastFollowResult,
                NextFollowAt = customer.NextFollowAt
            })
            .ToListAsync(cancellationToken);

        var recentFollowRecords = await (
                from record in _dbContext.CrmFollowRecords
                join customer in myCustomers on record.CustomerId equals customer.Id
                where !record.IsDeleted
                orderby record.CreatedAt descending
                select new CrmDashboardRecentFollowRecordDto
                {
                    Id = record.Id,
                    CustomerId = record.CustomerId,
                    BaseName = customer.BaseName,
                    FollowType = record.FollowType,
                    FollowResult = record.FollowResult,
                    IntentLevel = record.IntentLevel,
                    Content = record.Content,
                    NextFollowAt = record.NextFollowAt,
                    CreatedAt = record.CreatedAt
                })
            .Take(5)
            .ToListAsync(cancellationToken);

        var funnelStatuses = new[]
        {
            new { Code = CrmCodes.Status.Pending, Name = "待联系" },
            new { Code = CrmCodes.Status.Following, Name = "跟进中" },
            new { Code = CrmCodes.Status.Interested, Name = "有意向" },
            new { Code = CrmCodes.Status.Deal, Name = "成交" },
            new { Code = CrmCodes.Status.Lost, Name = "流失" }
        };
        var statusCounts = await myCustomers
            .GroupBy(customer => customer.Status)
            .Select(group => new { Status = group.Key, Count = group.Count() })
            .ToListAsync(cancellationToken);
        var followFunnel = funnelStatuses
            .Select(status => new CrmDashboardChartItemDto
            {
                Code = status.Code,
                Name = status.Name,
                Value = statusCounts.FirstOrDefault(item => item.Status == status.Code)?.Count ?? 0
            })
            .ToList();

        var productAttributes = await _dbContext.CrmBusinessEntityAttributes
            .Where(attribute =>
                !attribute.IsDeleted &&
                attribute.EntityType == CrmCodes.HerbBaseEntityType &&
                attribute.AttributeCode == CrmCodes.MainProductAttributeCode &&
                myCustomers.Select(customer => customer.Id).Contains(attribute.EntityId))
            .GroupBy(attribute => attribute.AttributeValue)
            .Select(group => new { Code = group.Key, Count = group.Count() })
            .ToListAsync(cancellationToken);
        var mainProductDistribution = productAttributes
            .Select(item => new CrmDashboardChartItemDto
            {
                Code = item.Code,
                Name = FormatMainProduct(item.Code),
                Value = item.Count
            })
            .OrderByDescending(item => item.Value)
            .ThenBy(item => item.Name)
            .ToList();

        var trendRecords = await (
                from record in _dbContext.CrmFollowRecords
                join customer in myCustomers on record.CustomerId equals customer.Id
                where !record.IsDeleted && record.CreatedAt >= trendStart && record.CreatedAt < tomorrowStart
                select new { record.CreatedAt, record.FollowResult })
            .ToListAsync(cancellationToken);
        var followTrend = Enumerable.Range(0, 7)
            .Select(offset => trendStart.AddDays(offset))
            .Select(date =>
            {
                var nextDate = date.AddDays(1);
                var records = trendRecords.Where(record => record.CreatedAt >= date && record.CreatedAt < nextDate).ToList();
                return new CrmDashboardTrendItemDto
                {
                    Date = date,
                    FollowCount = records.Count,
                    EffectiveFollowCount = records.Count(record => EffectiveFollowResults.Contains(record.FollowResult))
                };
            })
            .ToList();

        await FillMainProductsAsync(todayFollowCustomers, cancellationToken);

        return new CrmDashboardDto
        {
            Metrics = new CrmDashboardMetricsDto
            {
                TodayFollowCount = todayFollowCount,
                OverdueFollowCount = overdueFollowCount,
                MyCustomerCount = myCustomerCount,
                HighIntentCustomerCount = highIntentCustomerCount
            },
            TodayFollowCustomers = todayFollowCustomers,
            RecentFollowRecords = recentFollowRecords,
            FollowFunnel = followFunnel,
            MainProductDistribution = mainProductDistribution,
            FollowTrend = followTrend
        };
    }

    private static CrmDashboardDto BuildEmptyDashboard()
    {
        return new CrmDashboardDto
        {
            FollowFunnel =
            [
                new() { Code = CrmCodes.Status.Pending, Name = "待联系", Value = 0 },
                new() { Code = CrmCodes.Status.Following, Name = "跟进中", Value = 0 },
                new() { Code = CrmCodes.Status.Interested, Name = "有意向", Value = 0 },
                new() { Code = CrmCodes.Status.Deal, Name = "成交", Value = 0 },
                new() { Code = CrmCodes.Status.Lost, Name = "流失", Value = 0 }
            ],
            FollowTrend = Enumerable.Range(0, 7)
                .Select(offset => DateTime.Today.AddDays(-6 + offset))
                .Select(date => new CrmDashboardTrendItemDto { Date = date })
                .ToList()
        };
    }

    private async Task FillMainProductsAsync(List<CrmDashboardFollowCustomerDto> customers, CancellationToken cancellationToken)
    {
        var customerIds = customers.Select(customer => customer.Id).ToList();
        if (customerIds.Count == 0)
        {
            return;
        }

        var attributes = await _dbContext.CrmBusinessEntityAttributes
            .Where(attribute =>
                !attribute.IsDeleted &&
                customerIds.Contains(attribute.EntityId) &&
                attribute.EntityType == CrmCodes.HerbBaseEntityType &&
                attribute.AttributeCode == CrmCodes.MainProductAttributeCode)
            .OrderBy(attribute => attribute.SortOrder)
            .ThenBy(attribute => attribute.CreatedAt)
            .Select(attribute => new { attribute.EntityId, attribute.AttributeValue })
            .ToListAsync(cancellationToken);
        var lookup = attributes
            .GroupBy(attribute => attribute.EntityId)
            .ToDictionary(group => group.Key, group => group.Select(attribute => attribute.AttributeValue).Distinct().ToList());

        foreach (var customer in customers)
        {
            customer.MainProducts = lookup.TryGetValue(customer.Id, out var mainProducts)
                ? mainProducts
                : new List<string>();
        }
    }

    private static string FormatMainProduct(string code)
    {
        return code switch
        {
            "HUANG_QI" => "黄芪",
            "DANG_GUI" => "当归",
            "DANG_SHEN" => "党参",
            "TIAN_MA" => "天麻",
            "OTHER" => "其他",
            _ => code
        };
    }
}
