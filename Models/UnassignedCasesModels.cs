namespace CareLink.Backend.Models;

public class UnassignedCase
{
    public string Id { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Applicant { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Priority { get; set; } = "High";
    public string AiPriority { get; set; } = "High";
    public string Confidence { get; set; } = "85% confidence";
    public string Verification { get; set; } = "Contact verified";
    public string Assigned { get; set; } = "Unassigned";
    public string Age { get; set; } = "49 min";
    public string Status { get; set; } = "Submitted";
    public string Event { get; set; } = "Flood Response";
    public int Minutes { get; set; } = 49;
    public string ContactNumber { get; set; } = "+94 77 412 8041";
    public int HouseholdSize { get; set; } = 3;
    public string Description { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class UnassignedCasesStatsDto
{
    public int TotalCases { get; set; }
    public int CriticalCount { get; set; }
    public int DueSoonCount { get; set; }
    public int PendingInfoCount { get; set; }
    public int RecentlyUpdatedCount { get; set; }
}

public class UnassignedCasesQueryDto
{
    public string? Tab { get; set; } = "All";
    public string? Search { get; set; }
    public string? Event { get; set; }
    public string? Priority { get; set; }
    public string? Category { get; set; }
    public string? Status { get; set; }
    public string? DateRange { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class ClaimUnassignedCasesRequest
{
    public List<string> CaseIds { get; set; } = [];
    public string AssignTo { get; set; } = "You";
}

public class AssignUnassignedCaseRequest
{
    public string CaseId { get; set; } = string.Empty;
    public string AssignedTo { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class UpdateUnassignedCaseRequest
{
    public string? Priority { get; set; }
    public string? Status { get; set; }
    public string? Assigned { get; set; }
    public string? Notes { get; set; }
}
