namespace CareLink.Application.DTOs;

public class HealthSafetyMetaDto
{
    public List<string> IncidentTypes { get; set; } = new();
    public List<string> StaffMembers { get; set; } = new();
    public List<string> Severities { get; set; } = new();
    public List<string> Statuses { get; set; } = new();
}
