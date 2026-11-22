namespace CareLink.Core.Entities;

public class DisasterUpdate
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = "Announcement"; // Critical, Warning, Announcement, Update
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string PublishedBy { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public bool Archived { get; set; } = false;
    public bool IsDraft { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
