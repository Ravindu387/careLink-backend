namespace CareLink.Application.DTOs;

public class HealthSafetyIncidentDto
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string ReportedBy { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string AssignedStaff { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime IncidentDateUtc { get; set; }
}
