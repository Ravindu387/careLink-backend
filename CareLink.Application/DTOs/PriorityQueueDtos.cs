using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class PriorityQueueItemDto
{
    public string Id { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string CrisisEvent { get; set; } = "Monsoon Floods 2025";
    public string Reported { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string PriorityLevel { get; set; } = "High";
    public int Confidence { get; set; } = 85;
    public string Verification { get; set; } = "Pending verification";
    public string Assigned { get; set; } = "Unassigned";
    public string Age { get; set; } = string.Empty;
    public string Status { get; set; } = "Submitted";
    public bool Overdue { get; set; }
    public string Organisation { get; set; } = "National Disaster Authority";
    public string? Description { get; set; }
    public int AffectedPeopleCount { get; set; } = 1;
    public bool HasVulnerableMembers { get; set; }
    public bool IsLifeThreatening { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? AssignedAt { get; set; }
}

public class PriorityQueueMetricsDto
{
    public int TotalCount { get; set; }
    public int UnassignedCount { get; set; }
    public int OverdueCount { get; set; }
    public int PotentialDuplicatesCount { get; set; }
    public int CriticalOrOverdueRequiringAttentionCount { get; set; }
    public int CriticalCount { get; set; }
    public int HighCount { get; set; }
    public int MediumCount { get; set; }
    public int LowCount { get; set; }
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
}

public class SlaPolicyDto
{
    public string Level { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public int TargetMinutes { get; set; }
    public string Color { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class CreatePriorityRequestDto
{
    [Required]
    [StringLength(100)]
    public string Category { get; set; } = "Emergency shelter";

    [Required]
    [StringLength(200)]
    public string Location { get; set; } = string.Empty;

    [StringLength(150)]
    public string? CrisisEvent { get; set; } = "Monsoon Floods 2025";

    [StringLength(100)]
    public string? Priority { get; set; } = "Critical · 94%";

    [RegularExpression("^(Critical|High|Medium|Low)$", ErrorMessage = "PriorityLevel must be Critical, High, Medium, or Low.")]
    public string? PriorityLevel { get; set; } = "Critical";

    [Range(0, 100)]
    public int? Confidence { get; set; } = 94;

    [StringLength(100)]
    public string? Assigned { get; set; } = "Unassigned";

    [StringLength(100)]
    public string? Verification { get; set; } = "Pending verification";

    [StringLength(1000)]
    public string? Description { get; set; }

    [Range(1, 1000)]
    public int? AffectedPeopleCount { get; set; } = 1;
}

public class AssignTeamDto
{
    [Required]
    [StringLength(100)]
    public string AssignedTeam { get; set; } = string.Empty;
}

public class UpdatePriorityStatusDto
{
    [Required]
    [RegularExpression("^(Under Review|Assigned|Submitted|In Progress|Escalated|Completed)$", ErrorMessage = "Invalid status value.")]
    public string Status { get; set; } = "Under Review";
}

public class UpdatePriorityLevelDto
{
    [Required]
    [RegularExpression("^(Critical|High|Medium|Low)$", ErrorMessage = "PriorityLevel must be Critical, High, Medium, or Low.")]
    public string PriorityLevel { get; set; } = "Critical";

    [Range(0, 100)]
    public int? Confidence { get; set; }

    [StringLength(500)]
    public string? Reason { get; set; }
}

public class PriorityQueueQueryParameters
{
    public string? Search { get; set; }
    public string? Tab { get; set; }
    public string? CrisisEvent { get; set; }
    public string? Priority { get; set; }
    public string? Category { get; set; }
    public string? Status { get; set; }
    public string? Organisation { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public class PagedPriorityQueueDto
{
    public IReadOnlyList<PriorityQueueItemDto> Items { get; set; } = new List<PriorityQueueItemDto>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
