using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs.SecurityEvents;

public class CreateSecurityEventRequestDto
{
    [Required]
    public string Type { get; set; } = string.Empty;

    [Required]
    public string User { get; set; } = string.Empty;

    [Required]
    public string Source { get; set; } = string.Empty;

    public string Severity { get; set; } = "Low";

    public string Status { get; set; } = "Investigating";

    public string? Description { get; set; }

    public string Environment { get; set; } = "Production";
}
