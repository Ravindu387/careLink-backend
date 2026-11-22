namespace CareLink.Core.Models;

public class FlaggedTransactionQueryParameters
{
    public string? Search { get; set; }
    public string? RecordType { get; set; }    // All record types | Donations | Expenditure | Allocations
    public string? Severity { get; set; }      // All severities | High | Medium | Low
    public string? Status { get; set; }        // All statuses | Investigation Required | Under Review | Deferred | Resolved
    public string? DateSort { get; set; }      // Date flagged | Newest first | Oldest first
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
