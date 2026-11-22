namespace CareLink.Application.DTOs.Reports;

public class ReportColumnDto
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Type { get; set; } = "text"; // "text", "number", "badge", "date"
    public string Align { get; set; } = "left"; // "left", "center", "right"
}
