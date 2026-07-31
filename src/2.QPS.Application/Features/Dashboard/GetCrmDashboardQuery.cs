using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Dashboard;
using QPS.Application.Interfaces;

namespace QPS.Application.Features.Dashboard;

public class GetCrmDashboardQuery : IRequest<CrmDashboardDto>;

public class GetCrmDashboardHandler : IRequestHandler<GetCrmDashboardQuery, CrmDashboardDto>
{
    private const string CustomerEntityType = "CRM_HERB_BASE";
    private const string MainProductAttributeCode = "CRM_MAIN_PRODUCT";
    private static readonly string[] ClosedStatuses = ["DEAL", "LOST", "已成交", "已流失"];
    private static readonly string[] HighGrades = ["高", "A"];
    private static readonly string[] EffectiveFollowResults = ["CONNECTED", "INTERESTED", "已接通", "有意向"];

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
            customer => customer.NextFollowAt.HasValue && customer.NextFollowAt.Value < todayStart,
            cancellationToken);
        var myCustomerCount = await myCustomers.CountAsync(cancellationToken);
        var highIntentCustomerCount = await activeCustomers.CountAsync(
            customer => HighGrades.Contains(customer.Grade), cancellationToken);

        var todayFollowCustomers = await activeCustomers
            .Where(customer => customer.NextFollowAt.HasValue && customer.NextFollowAt.Value < tomorrowStart)
            .OrderBy(customer => customer.NextFollowAt >= now)
            .ThenByDescending(customer => HighGrades.Contains(customer.Grade))
            .ThenBy(customer => customer.NextFollowAt)
            .Take(10)
            .Select(customer => new CrmDashboardFollowCustomerDto
            {
                Id = customer.Id,
                BaseName = customer.BaseName,
                SubjectName = customer.SubjectName,
                MainProduct = customer.MainProduct,
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
            new { Code = "PENDING", Name = "待联系" },
            new { Code = "FOLLOWING", Name = "跟进中" },
            new { Code = "INTERESTED", Name = "有意向" },
            new { Code = "DEAL", Name = "成交" },
            new { Code = "LOST", Name = "流失" }
        };
        var statusCounts = await myCustomers
            .GroupBy(customer => customer.Status)
            .Select(group => new { Status = group.Key, Count = group.Count() })
            .ToListAsync(cancellationToken);
        var interestedCount = await myCustomers.CountAsync(
            customer => customer.LastFollowResult == "INTERESTED" || customer.LastFollowResult == "有意向",
            cancellationToken);
        var followFunnel = funnelStatuses
            .Select(status => new CrmDashboardChartItemDto
            {
                Code = status.Code,
                Name = status.Name,
                Value = status.Code == "INTERESTED"
                    ? interestedCount
                    : statusCounts.FirstOrDefault(item => item.Status == status.Code)?.Count ?? 0
            })
            .ToList();

        var productAttributes = await _dbContext.CrmBusinessEntityAttributes
            .Where(attribute =>
                !attribute.IsDeleted &&
                attribute.EntityType == CustomerEntityType &&
                attribute.AttributeCode == MainProductAttributeCode &&
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
                new() { Code = "PENDING", Name = "待联系", Value = 0 },
                new() { Code = "FOLLOWING", Name = "跟进中", Value = 0 },
                new() { Code = "INTERESTED", Name = "有意向", Value = 0 },
                new() { Code = "DEAL", Name = "成交", Value = 0 },
                new() { Code = "LOST", Name = "流失", Value = 0 }
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
                attribute.EntityType == CustomerEntityType &&
                attribute.AttributeCode == MainProductAttributeCode)
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
                : SplitMainProducts(customer.MainProduct);
        }
    }

    private static List<string> SplitMainProducts(string mainProduct)
    {
        return mainProduct.Split(',', '，', ';', '；', '/', '、')
            .Select(value => value.Trim())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
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
