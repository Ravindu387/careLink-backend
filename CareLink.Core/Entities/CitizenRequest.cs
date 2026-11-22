namespace CareLink.Core.Entities;

public class CitizenRequest
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Under review";
    public string Priority { get; set; } = "Normal";
    public string CitizenName { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? District { get; set; }
    public string Color { get; set; } = "blue";
    public bool ImmediateThreat { get; set; } = false;
    public string? AdditionalInfo { get; set; }
    public int PeopleCount { get; set; } = 1;
    public int Progress { get; set; } = 1; // 1: Sent, 2: Under review, 3: Help arranged/on the way, 4: Completed
    public string? DispatchTrackingId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
