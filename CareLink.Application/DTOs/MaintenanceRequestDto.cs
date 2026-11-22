namespace CareLink.Application.DTOs;

public class MaintenanceRequestDto
{
    public string Id { get; set; } = string.Empty;
    public string Issue { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Shelter { get; set; } = string.Empty;
    public string Room { get; set; } = string.Empty;
    public string Reported { get; set; } = string.Empty;
    public DateTime ReportedAtUtc { get; set; }
    public string Priority { get; set; } = string.Empty;
    public string Technician { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
