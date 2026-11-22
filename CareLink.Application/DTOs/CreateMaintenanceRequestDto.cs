using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class CreateMaintenanceRequestDto
{
    [Required]
    public string Issue { get; set; } = string.Empty;

    [Required]
    public string Category { get; set; } = "Plumbing";

    [Required]
    public string Shelter { get; set; } = "Riverton Central";

    public string Room { get; set; } = string.Empty;

    public string Priority { get; set; } = "Medium";

    public string Technician { get; set; } = "Unassigned";

    [Required]
    public string Description { get; set; } = string.Empty;
}
