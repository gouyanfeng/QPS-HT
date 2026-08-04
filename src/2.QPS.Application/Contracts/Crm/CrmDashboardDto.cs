namespace QPS.Application.Contracts.Crm;

public class CrmDashboardDto
{
    public CrmDashboardMetricsDto Metrics { get; set; } = new();
    public List<CrmDashboardFollowSubjectDto> TodayFollowSubjects { get; set; } = new();
    public List<CrmDashboardRecentFollowRecordDto> RecentFollowRecords { get; set; } = new();
    public List<CrmDashboardChartItemDto> FollowFunnel { get; set; } = new();
    public List<CrmDashboardChartItemDto> MainProductDistribution { get; set; } = new();
    public List<CrmDashboardTrendItemDto> FollowTrend { get; set; } = new();
}

public class CrmDashboardMetricsDto
{
    public int TodayFollowCount { get; set; }
    public int OverdueFollowCount { get; set; }
    public int MySubjectCount { get; set; }
    public int HighIntentSubjectCount { get; set; }
}

public class CrmDashboardFollowSubjectDto
{
    public Guid Id { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public List<string> MainProducts { get; set; } = new();
    public string Grade { get; set; } = string.Empty;
    public List<string> Regions { get; set; } = new();
    public string PrimaryContactName { get; set; } = string.Empty;
    public string PrimaryContactPhone { get; set; } = string.Empty;
    public string LastFollowResult { get; set; } = string.Empty;
    public DateTime? NextFollowAt { get; set; }
}

public class CrmDashboardRecentFollowRecordDto
{
    public Guid Id { get; set; }
    public Guid HerbBaseSubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
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
