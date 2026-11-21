namespace CareLink.Backend.Models;

public class VerificationCaseItem
{
    public string Id { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Applicant { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium";
    public string AiPriority { get; set; } = "Medium";
    public string Confidence { get; set; } = "85% confidence";
    public string Verification { get; set; } = "Evidence pending";
    public string Assigned { get; set; } = "Unassigned";
    public string Age { get; set; } = "15 min";
    public string Status { get; set; } = "Under Review";
    public string SubmissionAge { get; set; } = "15 min";
    public string? EvidenceNotes { get; set; }
    public List<string> Documents { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class VerificationStatistics
{
    public int AwaitingReview { get; set; } = 14;
    public string AwaitingReviewDesc { get; set; } = "3 critical priority";
    public int VerifiedToday { get; set; } = 92;
    public string VerifiedTodayDesc { get; set; } = "96% passed";
    public int InformationRequested { get; set; } = 18;
    public string InformationRequestedDesc { get; set; } = "Awaiting response";
    public int RejectedToday { get; set; } = 4;
    public string RejectedTodayDesc { get; set; } = "All with rationale";
    public string OldestPendingAge { get; set; } = "42 min";
}

public class VerificationAnalyticsResponse
{
    public VerificationStatistics Statistics { get; set; } = new();
    public int TotalCases { get; set; }
    public List<VerificationCaseItem> Cases { get; set; } = [];
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

public class VerificationDecisionRequest
{
    public string Decision { get; set; } = "Verify"; // "Verify", "RequestInfo", "Reject"
    public string? Rationale { get; set; }
    public string? VerifiedBy { get; set; } = "You";
}

public class VerificationQueryDto
{
    public string? Search { get; set; }
    public string? EvidenceState { get; set; }
    public string? Priority { get; set; }
    public string? Category { get; set; }
    public string? AgeFilter { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class PaginatedResult<T>
{
    public List<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
}
