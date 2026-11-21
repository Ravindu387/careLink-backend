using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class CreateHealthSafetyIncidentDto
{
    [Required]
    public string Type { get; set; } = "Medical assistance";

    [Required]
    public string Location { get; set; } = string.Empty;

    [Required]
    public string ReportedBy { get; set; } = string.Empty;

    public string Severity { get; set; } = "Medium";

    public string AssignedStaff { get; set; } = "Nurse M. Rao";

    [Required]
    public string Description { get; set; } = string.Empty;
}
