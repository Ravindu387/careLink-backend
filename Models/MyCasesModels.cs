namespace CareLink.Backend.Models;

public class MyCaseItem
{
    public string Id { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Applicant { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Priority { get; set; } = "High";
    public string Confidence { get; set; } = "90% confidence";
    public string Verification { get; set; } = "Verified";
    public string Assigned { get; set; } = "You";
    public string Age { get; set; } = "20 min";
    public string Status { get; set; } = "Assigned";
    public string Event { get; set; } = "Flood Response";
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class MyCasesStatsDto
{
    public int TotalCases { get; set; }
    public int CriticalCount { get; set; }
    public int DueSoonCount { get; set; }
    public int PendingInfoCount { get; set; }
    public int RecentlyUpdatedCount { get; set; }
}

public class MyCasesQueryDto
{
    public string? Tab { get; set; } = "All";
    public string? Search { get; set; }
    public string? Priority { get; set; }
    public string? Category { get; set; }
    public string? Status { get; set; }
    public string? Event { get; set; }
    public string? DateRange { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 5;
}

public class UpdateMyCaseStatusRequest
{
    public string Status { get; set; } = "Assigned";
    public string? Notes { get; set; }
    public string? Priority { get; set; }
}

public class CreateMyCaseRequest
{
    public string Category { get; set; } = "Emergency shelter";
    public string ApplicantName { get; set; } = string.Empty;
    public int PeopleCount { get; set; } = 1;
    public string Location { get; set; } = string.Empty;
    public string Priority { get; set; } = "High";
    public string Event { get; set; } = "Flood Response";
    public string? Description { get; set; }
}

public class BulkMyCasesActionRequest
{
    public List<string> CaseIds { get; set; } = [];
    public string Action { get; set; } = "Resolve"; // "Resolve", "Escalate", "Reassign", "UnderReview"
    public string? Notes { get; set; }
    public string? AssignTo { get; set; }
}
