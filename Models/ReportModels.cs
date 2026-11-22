namespace CareLink.Backend.Models;

public class ReportsFilterDto
{
    public string DateRange { get; set; } = "18–25 June 2025";
    public string CrisisEvent { get; set; } = "Monsoon Floods 2025";
    public string OperationalArea { get; set; } = "All areas";
}

public class AnalyticsKpi
{
    public int Received { get; set; } = 1947;
    public int Resolved { get; set; } = 1502;
    public string ResolutionRate { get; set; } = "77.1%";
    public string MedianResponse { get; set; } = "2h 18m";
    public string AiOverrides { get; set; } = "8.4%";
    public string AiOverridesDetails { get; set; } = "126 of 1,498 reviews";
    public string ReceivedTrend { get; set; } = "+8% today";
    public string ResponseTrend { get; set; } = "-18 min this week";
}

public class CategoryBreakdown
{
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
}

public class DailyResolutionPoint
{
    public string Day { get; set; } = string.Empty;
    public int Received { get; set; }
    public int Resolved { get; set; }
}

public class ReportItem
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconKey { get; set; } = "Inbox";
    public string Color { get; set; } = "bg-sky-50 text-sky-700";
}

public class ReportsAnalyticsResponse
{
    public ReportsFilterDto Filters { get; set; } = new();
    public AnalyticsKpi Analytics { get; set; } = new();
    public List<CategoryBreakdown> Categories { get; set; } = [];
    public List<DailyResolutionPoint> ResolutionTrend { get; set; } = [];
    public List<ReportItem> ReportLibrary { get; set; } = [];
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
