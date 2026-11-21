namespace CareLink.Backend.Models;

public class PriorityQueueItem
{
    public string Id { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Applicant { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Priority { get; set; } = "High";
    public string AiPriority { get; set; } = "High";
    public string Confidence { get; set; } = "90% confidence";
    public string Verification { get; set; } = "Verified";
    public string Assigned { get; set; } = "Unassigned";
    public string Age { get; set; } = "18 min";
    public string Status { get; set; } = "Under Review";
    public string Event { get; set; } = "Flood Response";
    public int Minutes { get; set; } = 18;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class PriorityQueueStatsDto
{
    public int TotalCases { get; set; }
    public int CriticalCount { get; set; }
    public int DueSoonCount { get; set; }
    public int PendingInfoCount { get; set; }
    public int RecentlyUpdatedCount { get; set; }
    public int OverdueCount { get; set; }
}

public class PriorityQueueQueryDto
{
    public string? Tab { get; set; } = "All";
    public string? Search { get; set; }
    public string? Event { get; set; }
    public string? Priority { get; set; }
    public string? Category { get; set; }
    public string? Status { get; set; }
    public string? DateRange { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 5;
}

public class ClaimPriorityCasesRequest
{
    public List<string> CaseIds { get; set; } = [];
    public string AssignTo { get; set; } = "You";
}

public class UpdatePriorityCaseRequest
{
    public string? Priority { get; set; }
    public string? Status { get; set; }
    public string? Assigned { get; set; }
    public string? Notes { get; set; }
}
