using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class AssistanceReferralDto
{
    public string Id { get; set; } = string.Empty;
    public string Assistance { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int Affected { get; set; }
    public string Priority { get; set; } = "Medium";
    public string Received { get; set; } = string.Empty;
    public string Assigned { get; set; } = "Unassigned";
    public string Status { get; set; } = "Awaiting Acceptance";
    public string Crisis { get; set; } = string.Empty;
    public string Sector { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Organisation { get; set; }
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? AcceptedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
}

public class AssistanceReferralMetricsDto
{
    public int OpenReferrals { get; set; }
    public int AwaitingAction { get; set; }
    public int CriticalPriority { get; set; }
    public int AcceptedToday { get; set; }
    public int PeopleSupported { get; set; }
    public int AllCount { get; set; }
    public int NeedsActionCount { get; set; }
    public int ActiveCount { get; set; }
    public int CompletedCount { get; set; }
    public int ArchivedCount { get; set; }
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
}

public class CreateAssistanceReferralDto
{
    [Required]
    [StringLength(200)]
    public string Assistance { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Location { get; set; } = string.Empty;

    [Range(1, 100000)]
    public int Affected { get; set; } = 1;

    [Required]
    [StringLength(50)]
    public string Priority { get; set; } = "High";

    [Required]
    [StringLength(100)]
    public string Crisis { get; set; } = "Monsoon Floods 2025";

    [Required]
    [StringLength(100)]
    public string Sector { get; set; } = "Shelter";

    [StringLength(100)]
    public string? Assigned { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    [StringLength(150)]
    public string? Organisation { get; set; }

    [StringLength(100)]
    public string? ContactName { get; set; }

    [StringLength(50)]
    public string? ContactPhone { get; set; }
}

public class UpdateAssistanceReferralDto
{
    [StringLength(200)]
    public string? Assistance { get; set; }

    [StringLength(200)]
    public string? Location { get; set; }

    [Range(1, 100000)]
    public int? Affected { get; set; }

    [StringLength(50)]
    public string? Priority { get; set; }

    [StringLength(100)]
    public string? Status { get; set; }

    [StringLength(100)]
    public string? Assigned { get; set; }

    [StringLength(100)]
    public string? Crisis { get; set; }

    [StringLength(100)]
    public string? Sector { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    [StringLength(150)]
    public string? Organisation { get; set; }

    [StringLength(100)]
    public string? ContactName { get; set; }

    [StringLength(50)]
    public string? ContactPhone { get; set; }
}

public class UpdateReferralStatusDto
{
    [Required]
    [StringLength(100)]
    public string Status { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Reason { get; set; }
}

public class AssignReferralTeamDto
{
    [Required]
    [StringLength(100)]
    public string Assigned { get; set; } = string.Empty;
}

public class AssistanceReferralQueryParameters
{
    public string? Search { get; set; }
    public string? Tab { get; set; }
    public string? Crisis { get; set; }
    public string? Priority { get; set; }
    public string? Sector { get; set; }
    public string? Status { get; set; }
}
