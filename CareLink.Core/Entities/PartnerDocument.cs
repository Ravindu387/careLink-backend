namespace CareLink.Core.Entities;

public class PartnerDocument
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "Operational Plan";
    public string Owner { get; set; } = "Sara Khan";
    public string Updated { get; set; } = string.Empty;
    public string Version { get; set; } = "v1.0";
    public string Visibility { get; set; } = "Internal";
    public string Status { get; set; } = "Under Review";
    public string? FileName { get; set; }
    public string? FileUrl { get; set; }
    public long? FileSizeBytes { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
