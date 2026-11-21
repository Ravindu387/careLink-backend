namespace CareLink.Core.Entities;

public class TeamMember
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Sector { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string Contact { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string LastActive { get; set; } = "Online";
    public string Status { get; set; } = "Active";
    public string? Description { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class TeamContactsMetrics
{
    public int ActiveRecords { get; set; } = 48;
    public string ActiveRecordsSubtitle { get; set; } = "Organisation-scoped";
    public int NeedsReview { get; set; } = 3;
    public string NeedsReviewSubtitle { get; set; } = "Assigned to your team";
    public string Verified { get; set; } = "96%";
    public string VerifiedSubtitle { get; set; } = "Current assurance";
    public int DueSoon { get; set; } = 2;
    public string DueSoonSubtitle { get; set; } = "Within 30 days";

    public int TotalCount { get; set; }
    public int NeedsActionCount { get; set; }
    public int ActiveCount { get; set; }
    public int CompletedCount { get; set; }
    public int ArchivedCount { get; set; }
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
}

public class TeamMemberQuery
{
    public string? Search { get; set; }
    public string? Tab { get; set; }
    public string? Role { get; set; }
    public string? Sector { get; set; }
    public string? Location { get; set; }
    public string? Status { get; set; }
}
