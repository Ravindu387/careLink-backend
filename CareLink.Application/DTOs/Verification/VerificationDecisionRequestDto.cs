namespace CareLink.Application.DTOs.Verification;

public class VerificationDecisionRequestDto
{
    /// <summary>
    /// Accepts Status ("Approved", "Rejected", "Additional Info") or Decision ("Approve", "Reject", "RequestInfo").
    /// </summary>
    public string? Status { get; set; }

    public string? Decision { get; set; }

    public string? Rationale { get; set; }

    public string? DecisionNote { get; set; }

    public string ResolvedStatus =>
        !string.IsNullOrWhiteSpace(Status) ? Status.Trim() : (Decision?.Trim() ?? "Approved");

    public string ResolvedRationale =>
        !string.IsNullOrWhiteSpace(Rationale) ? Rationale.Trim() : (DecisionNote?.Trim() ?? string.Empty);
}
