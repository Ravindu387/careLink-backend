namespace CareLink.Application.DTOs;

public class ReportItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;
    public string Generated { get; set; } = string.Empty;
    public string Format { get; set; } = "PDF";
    public string? DownloadUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ReportScheduleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Frequency { get; set; } = "Weekly";
    public string Format { get; set; } = "PDF";
    public string Period { get; set; } = "Last 30 days";
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class GenerateReportRequest
{
    public string Name { get; set; } = "Operational Analytics Report";
    public string Category { get; set; } = "Fulfilment";
    public string Period { get; set; } = "Last 30 days";
    public string Format { get; set; } = "CSV";
}

public class CreateReportScheduleRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Frequency { get; set; } = "Weekly";
    public string Format { get; set; } = "PDF";
    public string? Period { get; set; } = "Last 30 days";
}

public class ReportFilterParams
{
    public string? Search { get; set; }
    public string? Category { get; set; }
    public string? Format { get; set; }
}

public class AnalyticsQueryParams
{
    public string? Period { get; set; } = "Last 30 days";
    public string? Operation { get; set; } = "Monsoon Floods 2025";
    public string? Warehouse { get; set; } = "All warehouses";
}

public class AnalyticsKpiDto
{
    public string RequestsFulfilled { get; set; } = "93.8%";
    public string RequestsFulfilledTrend { get; set; } = "+2.1% vs previous period";
    public string AverageFulfilmentTime { get; set; } = "3h 18m";
    public string AverageFulfilmentTimeTrend { get; set; } = "-24 min improvement";
    public string InventoryTurnover { get; set; } = "4.6×";
    public string InventoryTurnoverNote { get; set; } = "Current operation";
    public string StockLossRate { get; set; } = "0.42%";
    public string StockLossRateNote { get; set; } = "Within 0.5% target";
}

public class WeeklyTrendDto
{
    public string Label { get; set; } = string.Empty;
    public int Received { get; set; }
    public int Fulfilled { get; set; }
}

public class CategoryDistributionDto
{
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
    public string Color { get; set; } = "bg-sky-600";
}

public class AnalyticsDashboardDto
{
    public AnalyticsKpiDto Kpis { get; set; } = new();
    public List<WeeklyTrendDto> WeeklyTrends { get; set; } = [];
    public List<CategoryDistributionDto> CategoryDistributions { get; set; } = [];
    public string Period { get; set; } = "Last 30 days";
    public string Operation { get; set; } = "Monsoon Floods 2025";
    public string Warehouse { get; set; } = "All warehouses";
}
