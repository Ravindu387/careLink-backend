namespace CareLink.Core.Entities;

public class AssistanceReferral
{
    public string Id { get; set; } = string.Empty;
    public string Assistance { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int Affected { get; set; } = 1;
    public string Priority { get; set; } = "Medium"; // Critical, High, Medium, Low
    public string Received { get; set; } = string.Empty;
    public string Assigned { get; set; } = "Unassigned";
    public string Status { get; set; } = "Awaiting Acceptance"; // Awaiting Acceptance, Accepted, In Progress, Completed, Archived
    public string Crisis { get; set; } = "Monsoon Floods 2025";
    public string Sector { get; set; } = "Shelter"; // Shelter, Food, Health, WASH, Logistics, Protection
    public string? Description { get; set; }
    public string? Organisation { get; set; } = "CareLink Partner Office";
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? AcceptedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
}
