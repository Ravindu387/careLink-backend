namespace CareLink.Application.DTOs.Reports;

public class ExportReportRequestDto
{
    public string Format { get; set; } = "csv"; // "csv" or "json"
    public string? DateRange { get; set; }
    public string? Organisation { get; set; }
    public string? Region { get; set; }
    public string? Rationale { get; set; }
}
