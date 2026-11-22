namespace CareLink.Application.DTOs;

public class FlaggedTransactionDto
{
    public string Id { get; set; } = string.Empty;
    public string Record { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string AmountDisplay { get; set; } = string.Empty;
    public string Party { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Flagged { get; set; } = string.Empty;
    public DateTime FlaggedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Investigator { get; set; } = string.Empty;
    public string RecordType { get; set; } = string.Empty;
    public string ResolutionNotes { get; set; } = string.Empty;
    public DateTime? ResolvedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
