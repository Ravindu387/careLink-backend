namespace CareLink.Core.Entities;

public class AssistanceRequest
{
    public string Id { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string CrisisEvent { get; set; } = "Monsoon Floods 2025";
    public string PriorityLevel { get; set; } = "High";
    public int Confidence { get; set; } = 85;
    public string PriorityDisplay { get; set; } = string.Empty;
    public string Verification { get; set; } = "Pending verification";
    public string Assigned { get; set; } = "Unassigned";
    public string Status { get; set; } = "Submitted";
    public string Organisation { get; set; } = "National Disaster Authority";
    public string? Description { get; set; }
    public int AffectedPeopleCount { get; set; } = 1;
    public bool HasVulnerableMembers { get; set; }
    public bool IsLifeThreatening { get; set; }
    public bool IsOverdue { get; set; }
    public bool IsPotentialDuplicate { get; set; }
    public string? DuplicateOfId { get; set; }
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? AssignedAt { get; set; }
}
