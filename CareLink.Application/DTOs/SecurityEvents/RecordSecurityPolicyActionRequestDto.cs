using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs.SecurityEvents;

public class RecordSecurityPolicyActionRequestDto
{
    [Required]
    public string PolicyTitle { get; set; } = string.Empty;

    public string Environment { get; set; } = "Production";

    public string Scope { get; set; } = "Selected record";

    public string ApprovalType { get; set; } = "Standard administrator approval";

    [Required]
    public string PolicyReason { get; set; } = string.Empty;

    public string? RelatedEventId { get; set; }
}
