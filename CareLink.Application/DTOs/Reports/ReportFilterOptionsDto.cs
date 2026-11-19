namespace CareLink.Application.DTOs.Reports;

public class ReportFilterOptionsDto
{
    public List<string> DateRanges { get; set; } = new();
    public List<string> Organisations { get; set; } = new();
    public List<string> Regions { get; set; } = new();
}
