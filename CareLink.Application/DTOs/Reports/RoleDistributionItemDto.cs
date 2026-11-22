namespace CareLink.Application.DTOs.Reports;

public class RoleDistributionItemDto
{
    public string Label { get; set; } = string.Empty;
    public string RoleKey { get; set; } = string.Empty;
    public int Count { get; set; }
    public string Value { get; set; } = string.Empty;
    public string Width { get; set; } = "0%";
    public double Percentage { get; set; }
}
