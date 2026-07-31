namespace QPS.Application.Contracts.Dashboard;

public class CrmDashboardDto
{
    public CrmDashboardMetricsDto Metrics { get; set; } = new();
    public List<CrmDashboardFollowCustomerDto> TodayFollowCustomers { get; set; } = new();
    public List<CrmDashboardRecentFollowRecordDto> RecentFollowRecords { get; set; } = new();
    public List<CrmDashboardChartItemDto> FollowFunnel { get; set; } = new();
    public List<CrmDashboardChartItemDto> MainProductDistribution { get; set; } = new();
    public List<CrmDashboardTrendItemDto> FollowTrend { get; set; } = new();
}

public class CrmDashboardMetricsDto
{
    public int TodayFollowCount { get; set; }
    public int OverdueFollowCount { get; set; }
    public int MyCustomerCount { get; set; }
    public int HighIntentCustomerCount { get; set; }
}

public class CrmDashboardFollowCustomerDto
{
    public Guid Id { get; set; }
    public string BaseName { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public string MainProduct { get; set; } = string.Empty;
    public List<string> MainProducts { get; set; } = new();
    public string Grade { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string PrimaryContactName { get; set; } = string.Empty;
    public string PrimaryContactPhone { get; set; } = string.Empty;
    public string LastFollowResult { get; set; } = string.Empty;
    public DateTime? NextFollowAt { get; set; }
}

public class CrmDashboardRecentFollowRecordDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string BaseName { get; set; } = string.Empty;
    public string FollowType { get; set; } = string.Empty;
    public string FollowResult { get; set; } = string.Empty;
    public string IntentLevel { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime? NextFollowAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CrmDashboardChartItemDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
}

public class CrmDashboardTrendItemDto
{
    public DateTime Date { get; set; }
    public int FollowCount { get; set; }
    public int EffectiveFollowCount { get; set; }
}
