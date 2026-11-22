namespace CareLink.Core.Entities;

public class PriorityQueueItem
{
    public string Id { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string CrisisEvent { get; set; } = "Monsoon Floods 2025";
    public string PriorityLevel { get; set; } = "High"; // Critical, High, Medium, Low
    public int Confidence { get; set; } = 85;
    public string PriorityDisplay { get; set; } = string.Empty; // e.g. "Critical · 94%"
    public string Verification { get; set; } = "Pending verification"; // Verified, Contact verified, Evidence pending, Pending verification
    public string Assigned { get; set; } = "Unassigned";
    public string Status { get; set; } = "Submitted"; // Under Review, Assigned, Submitted, In Progress, Escalated, Completed
    public string Organisation { get; set; } = "National Disaster Authority";
    public string? Description { get; set; }
    public int AffectedPeopleCount { get; set; } = 1;
    public bool HasVulnerableMembers { get; set; }
    public bool IsLifeThreatening { get; set; }
    public bool IsOverdue { get; set; }
    public bool IsPotentialDuplicate { get; set; }
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? AssignedAt { get; set; }
}
