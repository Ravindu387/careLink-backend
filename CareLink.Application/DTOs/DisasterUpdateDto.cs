namespace CareLink.Application.DTOs;

public class DisasterUpdateDto
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string PublishedBy { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public bool Archived { get; set; }
    public bool IsDraft { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
