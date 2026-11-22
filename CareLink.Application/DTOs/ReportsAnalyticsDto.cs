namespace CareLink.Application.DTOs;

public class ReportsAnalyticsDto
{
    public ReportsMetricsDto Metrics { get; set; } = new();
    public List<TrendPointDto> TrendData { get; set; } = new();
    public List<string> Shelters { get; set; } = new();
    public List<string> Districts { get; set; } = new();
    public List<string> DateRanges { get; set; } = new();
    public Dictionary<string, string> AppliedFilters { get; set; } = new();
}
