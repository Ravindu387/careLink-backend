using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class TeamMemberDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Sector { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string Contact { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string LastActive { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public class CreateTeamMemberDto
{
    [Required]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Role { get; set; } = "Field Team Lead";

    [Required]
    [StringLength(100)]
    public string Sector { get; set; } = "WASH";

    [Required]
    [StringLength(100)]
    public string Area { get; set; } = "North Ward";

    [Required]
    [StringLength(50)]
    public string Contact { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [StringLength(50)]
    public string LastActive { get; set; } = "Invited";

    [StringLength(50)]
    public string Status { get; set; } = "Active";

    [StringLength(500)]
    public string? Description { get; set; }
}

public class UpdateTeamMemberDto
{
    [StringLength(120)]
    public string? Name { get; set; }

    [StringLength(100)]
    public string? Role { get; set; }

    [StringLength(100)]
    public string? Sector { get; set; }

    [StringLength(100)]
    public string? Area { get; set; }

    [StringLength(50)]
    public string? Contact { get; set; }

    [EmailAddress]
    [StringLength(150)]
    public string? Email { get; set; }

    [StringLength(50)]
    public string? LastActive { get; set; }

    [StringLength(50)]
    public string? Status { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }
}

public class UpdateTeamMemberStatusDto
{
    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Active";
}

public class TeamContactsMetricsDto
{
    public int ActiveRecords { get; set; }
    public string ActiveRecordsSubtitle { get; set; } = string.Empty;
    public int NeedsReview { get; set; }
    public string NeedsReviewSubtitle { get; set; } = string.Empty;
    public string Verified { get; set; } = string.Empty;
    public string VerifiedSubtitle { get; set; } = string.Empty;
    public int DueSoon { get; set; }
    public string DueSoonSubtitle { get; set; } = string.Empty;

    public int TotalCount { get; set; }
    public int NeedsActionCount { get; set; }
    public int ActiveCount { get; set; }
    public int CompletedCount { get; set; }
    public int ArchivedCount { get; set; }
    public DateTimeOffset LastUpdated { get; set; }
}

public class TeamMemberQueryParameters
{
    public string? Search { get; set; }
    public string? Tab { get; set; }
    public string? Role { get; set; }
    public string? Sector { get; set; }
    public string? Location { get; set; }
    public string? Status { get; set; }
}
