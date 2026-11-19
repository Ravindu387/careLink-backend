namespace CareLink.Application.DTOs.Reports;

public class StatMetricDto
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public string Tone { get; set; } = "blue"; // "blue" or "green"
    public string? Change { get; set; }
}
