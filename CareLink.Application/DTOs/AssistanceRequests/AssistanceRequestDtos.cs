namespace CareLink.Application.DTOs.AssistanceRequests;

public class AssistanceRequestDto
{
    public string Id { get; set; } = string.Empty;
    public string TimeAgo { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string ReportedPriority { get; set; } = "High";
    public string Recommendation { get; set; } = "High priority";
    public string Confidence { get; set; } = "88%";
    public string AssignedTo { get; set; } = "Unassigned";
    public string Status { get; set; } = "Awaiting review";
    public int AffectedPeople { get; set; } = 1;
    public string Contact { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
    public string? FieldVerificationNote { get; set; }
    public string? CrisisName { get; set; } = "Monsoon Floods 2025";
}

public class AssistanceRequestStatsDto
{
    public int TotalRequests { get; set; }
    public int AwaitingReviewCount { get; set; }
    public int VerifiedCount { get; set; }
    public int AssignedCount { get; set; }
    public int InProgressCount { get; set; }
    public int CompletedCount { get; set; }
    public int CriticalCount { get; set; }
}

public class AssistanceRequestQueryDto
{
    public string? Search { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public string? Category { get; set; }
    public string? Crisis { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class CreateAssistanceRequestDto
{
    public string Category { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string ReportedPriority { get; set; } = "High";
    public int AffectedPeople { get; set; } = 1;
    public string Contact { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? CrisisName { get; set; } = "Monsoon Floods 2025";
}

public class UpdateAssistanceRequestDto
{
    public string Category { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string ReportedPriority { get; set; } = "High";
    public int AffectedPeople { get; set; } = 1;
    public string Contact { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class UpdateRequestStatusDto
{
    public string Status { get; set; } = "Verified"; // Awaiting review | Verified | Assigned | In progress | Completed | Rejected
    public string? Reason { get; set; }
    public string? Note { get; set; }
}

public class AssignRequestDto
{
    public string AssignedTo { get; set; } = string.Empty;
    public string? Instructions { get; set; }
}

public class UpdateRequestPriorityDto
{
    public string Priority { get; set; } = "Critical"; // Critical | High | Medium | Low
    public string? Rationale { get; set; }
}
