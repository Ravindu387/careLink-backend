namespace CareLink.Application.DTOs;

public class UpdateAssistanceRequestDto
{
    public string? Category { get; set; }
    public string? Location { get; set; }
    public string? Priority { get; set; }
    public string? Verification { get; set; }
    public string? Assigned { get; set; }
    public string? Status { get; set; }
    public string? Organisation { get; set; }
    public string? Description { get; set; }
    public int? AffectedPeopleCount { get; set; }
    public bool? HasVulnerableMembers { get; set; }
    public bool? IsLifeThreatening { get; set; }
}
