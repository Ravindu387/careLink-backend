namespace CareLink.Core.Entities;

public class ReportItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;
    public string Generated { get; set; } = string.Empty;
    public string Format { get; set; } = "PDF";
    public string? DownloadUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
