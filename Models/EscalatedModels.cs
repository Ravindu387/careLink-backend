namespace CareLink.Backend.Models;

public class EscalatedCaseItem
{
    public string Id { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Applicant { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Priority { get; set; } = "Critical";
    public string AiPriority { get; set; } = "Critical";
    public string Confidence { get; set; } = "94% confidence";
    public string Verification { get; set; } = "Evidence pending";
    public string Assigned { get; set; } = "You";
    public string Age { get; set; } = "18 min";
    public string Status { get; set; } = "Escalated";
    public string Event { get; set; } = "Flood Response";
    public int Minutes { get; set; } = 18;
    public string? EscalationReason { get; set; }
    public string? TargetOrg { get; set; }
    public string? CoordinatorNotes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class EscalationStatsDto
{
    public int TotalEscalated { get; set; }
    public int CriticalCount { get; set; }
    public int DueSoonCount { get; set; }
    public int PendingInfoCount { get; set; }
    public int RecentlyUpdatedCount { get; set; }
}

public class EscalatedQueryDto
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

public class BulkEscalationActionRequest
{
    public List<string> CaseIds { get; set; } = [];
    public string Action { get; set; } = "Escalate"; // "Escalate", "De-escalate", "AssignCoordinator", "Resolve"
    public string? Notes { get; set; }
    public string? AssignTo { get; set; }
}

public class EscalationDecisionRequest
{
    public string Action { get; set; } = "Resolve"; // "Resolve", "Reassign", "RequestCrossOrg", "UpdateNotes"
    public string? Notes { get; set; }
    public string? TargetOrg { get; set; }
    public string? AssignTo { get; set; }
}
