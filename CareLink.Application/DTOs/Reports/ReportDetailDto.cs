namespace CareLink.Application.DTOs.Reports;

public class ReportDetailDto
{
    public string ReportId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public string GeneratedBy { get; set; } = "System Administrator";
    public int TotalRecords { get; set; }
    public string FilterSummary { get; set; } = string.Empty;
    public Dictionary<string, string> SummaryMetrics { get; set; } = new();
    public List<ReportColumnDto> Columns { get; set; } = new();
    public List<Dictionary<string, object?>> Rows { get; set; } = new();
}
