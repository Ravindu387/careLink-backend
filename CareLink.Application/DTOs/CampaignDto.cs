namespace CareLink.Application.DTOs;

public class CampaignDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Event { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;
    public decimal Raised { get; set; }
    public decimal Target { get; set; }
    public string Status { get; set; } = "Active";
    public string StartDate { get; set; } = string.Empty;
    public string EndDate { get; set; } = string.Empty;
    public decimal ProgressPercentage => Target > 0 ? Math.Round((Raised / Target) * 100, 1) : 0;
    public string? Description { get; set; }
}
