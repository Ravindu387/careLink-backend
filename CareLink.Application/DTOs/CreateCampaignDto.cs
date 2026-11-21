namespace CareLink.Application.DTOs;

public class CreateCampaignDto
{
    public string Name { get; set; } = string.Empty;
    public string Event { get; set; } = string.Empty;
    public decimal Target { get; set; }
    public string StartDate { get; set; } = string.Empty;
    public string EndDate { get; set; } = string.Empty;
    public string? Description { get; set; }
}
