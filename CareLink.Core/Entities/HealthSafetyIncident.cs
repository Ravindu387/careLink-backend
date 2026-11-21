namespace CareLink.Core.Entities;

public class HealthSafetyIncident
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = "Medical assistance";
    public string Location { get; set; } = string.Empty;
    public string ReportedBy { get; set; } = string.Empty;
    public string Severity { get; set; } = "Medium"; // Critical, High, Medium, Low
    public string AssignedStaff { get; set; } = string.Empty;
    public string Status { get; set; } = "Action Required"; // Action Required, Under Investigation, Resolved
    public string Description { get; set; } = string.Empty;
    public DateTime IncidentDate { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
