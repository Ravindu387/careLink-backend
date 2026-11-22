namespace CareLink.Core.Entities;

public class FlaggedTransaction
{
    public string Id { get; set; } = string.Empty;        // FLG-XXX
    public string Record { get; set; } = string.Empty;   // DON-XXXXX | EXP-XXXX | ALL-XXXX
    public string Reason { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string AmountDisplay { get; set; } = string.Empty; // "$18,420"
    public string Party { get; set; } = string.Empty;
    public string Severity { get; set; } = "Medium";     // High | Medium | Low
    public string Flagged { get; set; } = string.Empty;  // "22 Jun · 11:42"
    public DateTime FlaggedAt { get; set; }
    public string Status { get; set; } = "Investigation Required"; // Investigation Required | Under Review | Deferred | Resolved
    public string Description { get; set; } = string.Empty;
    public string Investigator { get; set; } = "Not assigned";
    public string RecordType { get; set; } = string.Empty; // Donations | Expenditure | Allocations
    public string ResolutionNotes { get; set; } = string.Empty;
    public DateTime? ResolvedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
