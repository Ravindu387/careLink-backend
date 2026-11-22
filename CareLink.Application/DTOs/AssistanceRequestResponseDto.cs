namespace CareLink.Application.DTOs;

public class AssistanceRequestResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string CrisisEvent { get; set; } = string.Empty;
    public string Reported { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string PriorityLevel { get; set; } = string.Empty;
    public int Confidence { get; set; }
    public string Verification { get; set; } = string.Empty;
    public string Assigned { get; set; } = string.Empty;
    public string Age { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Organisation { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int AffectedPeopleCount { get; set; }
    public bool HasVulnerableMembers { get; set; }
    public bool IsLifeThreatening { get; set; }
    public bool IsOverdue { get; set; }
    public bool IsPotentialDuplicate { get; set; }
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
