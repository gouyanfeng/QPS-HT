namespace QPS.Application.Contracts.Statistics;

public class StatisticsDto
{
    public SummaryData Summary { get; set; }
}

public class ChartData
{
    public string[] Labels { get; set; }
    public int[] Values { get; set; }
    public decimal[] DecimalValues { get; set; }
}

public class SummaryData
{
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalRooms { get; set; }
    public int AvailableRooms { get; set; }
}

public class OrderTrendDto
{
    public string Date { get; set; }
    public int OrderCount { get; set; }
    public decimal Revenue { get; set; }
}

public class RoomStatusDto
{
    public string Status { get; set; }
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

public class StatisticsOverviewDto
{
    public OverviewSummaryDto Summary { get; set; }
    public TrendChartDto OrderTrend { get; set; }
    public TrendChartDto RevenueTrend { get; set; }
    public BarChartDto TopShopRevenue { get; set; }
}

public class OverviewSummaryDto
{
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public int CompletedOrders { get; set; }
    public decimal AverageOrderValue { get; set; }
    public decimal PreviousRevenue { get; set; }
    public decimal RevenueGrowthRate { get; set; }
    public string TimeRange { get; set; } = string.Empty;
}

public class RoomRealtimeDto
{
    public int TotalRooms { get; set; }
    public int AvailableRooms { get; set; }
    public decimal OccupancyRate { get; set; }
    public decimal AvgRoomUnitPrice { get; set; }
}

public class TrendChartDto
{
    public string[] Labels { get; set; } = Array.Empty<string>();
    public ChartSeriesDto[] Series { get; set; } = Array.Empty<ChartSeriesDto>();
}

public class ChartSeriesDto
{
    public string Name { get; set; } = string.Empty;
    public decimal[] Data { get; set; } = Array.Empty<decimal>();
    public string Type { get; set; } = "line";
}

public class PieChartDto
{
    public PieSliceDto[] Data { get; set; } = Array.Empty<PieSliceDto>();
}

public class PieSliceDto
{
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
    public decimal Percentage { get; set; }
}

public class BarChartDto
{
    public string[] Labels { get; set; } = Array.Empty<string>();
    public decimal[] Data { get; set; } = Array.Empty<decimal>();
}

public class StatisticsRealtimeDto
{
    public PieChartDto RoomStatusDistribution { get; set; }
}