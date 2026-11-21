using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs.SecurityEvents;

public class UpdateSecurityEventStatusRequestDto
{
    [Required]
    public string Status { get; set; } = "Resolved";

    public string? ResolutionNote { get; set; }
}
