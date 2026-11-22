namespace CareLink.Application.DTOs.Reports;

public class TrendPointDto
{
    public string Month { get; set; } = string.Empty;
    public int ActiveUsers { get; set; }
    public int NewRegistrations { get; set; }
    public int Y { get; set; } // SVG coordinate point or normalized 0-100 value
}
