namespace CareLink.Core.Entities;

public class CollaborationRequest
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Partner { get; set; } = string.Empty;
    public string Sector { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string Support { get; set; } = string.Empty;
    public string StartDate { get; set; } = string.Empty;
    public string Status { get; set; } = "Awaiting Response";
    public string? Description { get; set; }
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? AcceptedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
}
