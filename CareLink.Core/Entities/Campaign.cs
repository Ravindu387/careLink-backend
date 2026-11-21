namespace CareLink.Core.Entities;

public class Campaign
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Event { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;
    public decimal Raised { get; set; }
    public decimal Target { get; set; }
    public string Status { get; set; } = "Active"; // Active | Completed | Archived
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
