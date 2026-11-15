using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs.Integrations;

public class IntegrationActionRequestDto
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string ActionTitle { get; set; } = string.Empty;

    public string Environment { get; set; } = "Production";

    public string Scope { get; set; } = "Selected record";

    public string ApprovalType { get; set; } = "Standard administrator approval";

    [Required]
    [StringLength(1000, MinimumLength = 5)]
    public string AdministrativeRationale { get; set; } = string.Empty;
}
