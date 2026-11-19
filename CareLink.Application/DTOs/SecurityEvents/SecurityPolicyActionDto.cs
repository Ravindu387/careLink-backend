namespace CareLink.Application.DTOs.SecurityEvents;

public class SecurityPolicyActionDto
{
    public string Id { get; set; } = string.Empty;
    public string PolicyTitle { get; set; } = string.Empty;
    public string Environment { get; set; } = "Production";
    public string Scope { get; set; } = "Selected record";
    public string ApprovalType { get; set; } = "Standard administrator approval";
    public string PolicyReason { get; set; } = string.Empty;
    public string PerformedBy { get; set; } = string.Empty;
    public string? RelatedEventId { get; set; }
    public DateTime CreatedAt { get; set; }
}
