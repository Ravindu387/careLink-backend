namespace CareLink.Core.Entities;

public class MaintenanceRequest
{
    public string Id { get; set; } = string.Empty;
    public string Issue { get; set; } = string.Empty;
    public string Category { get; set; } = "Plumbing";
    public string Shelter { get; set; } = string.Empty;
    public string Room { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium"; // Critical, High, Medium, Low
    public string Technician { get; set; } = "Unassigned";
    public string Status { get; set; } = "Pending"; // In Progress, Assigned, Completed, Pending, On Hold
    public string Description { get; set; } = string.Empty;
    public DateTime ReportedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
