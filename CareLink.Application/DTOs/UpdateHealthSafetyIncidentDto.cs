namespace CareLink.Application.DTOs;

public class UpdateHealthSafetyIncidentDto
{
    public string? AssignedStaff { get; set; }
    public string? Severity { get; set; }
    public string? Status { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
}
