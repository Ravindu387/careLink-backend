namespace CareLink.Application.DTOs.Reports;

public class ReportFilterDto
{
    public string DateRange { get; set; } = "Last 30 days";
    public string? Organisation { get; set; }
    public string? Region { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
