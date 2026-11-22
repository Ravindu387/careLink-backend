namespace CareLink.Backend.Models;

public class DuplicateCaseItem
{
    public string Id { get; set; } = string.Empty;
    public string PrimaryCase { get; set; } = string.Empty;
    public string DuplicateCase { get; set; } = string.Empty;
    public string Factors { get; set; } = string.Empty;
    public int Score { get; set; } = 85;
    public string Confidence { get; set; } = "High"; // "High", "Medium", "Low"
    public string Status { get; set; } = "Awaiting Review"; // "Awaiting Review", "Deferred", "Confirmed", "Not Duplicate"
    public string Category { get; set; } = string.Empty;
    public string Detected { get; set; } = "10 min ago";
    public string Description { get; set; } = string.Empty;
    public string? ReviewNotes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class DuplicatesStatsDto
{
    public int TotalMatches { get; set; }
    public int AwaitingReviewCount { get; set; }
    public int HighConfidenceCount { get; set; }
    public int ConfirmedCount { get; set; }
    public int DeferredCount { get; set; }
    public int NotDuplicateCount { get; set; }
}

public class DuplicateQueryDto
{
    public string? Search { get; set; }
    public string? Similarity { get; set; } = "All similarity levels";
    public string? Status { get; set; } = "All review states";
    public string? Category { get; set; } = "All categories";
    public string? DateRange { get; set; } = "Date detected";
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 4;
}

public class UpdateDuplicateStatusRequest
{
    public string Status { get; set; } = "Awaiting Review";
    public string? Notes { get; set; }
}

public class MergeCasesRequest
{
    public string MatchId { get; set; } = string.Empty;
    public string PrimaryCaseId { get; set; } = string.Empty;
    public string DuplicateCaseId { get; set; } = string.Empty;
    public string? ResolutionNotes { get; set; }
}
