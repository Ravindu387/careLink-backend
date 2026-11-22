namespace CareLink.Application.DTOs;

public class UpdateCampaignDto
{
    public string? Name { get; set; }
    public string? Event { get; set; }
    public decimal? Target { get; set; }
    public decimal? Raised { get; set; }
    public string? Status { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public string? Description { get; set; }
}
