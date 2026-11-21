namespace CareLink.Core.Entities;

public class CitizenDocument
{
    public string Id { get; set; } = string.Empty;
    public string CitizenName { get; set; } = "Samira Khan";
    public string CitizenId { get; set; } = "CIT-001";
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = "Identity"; // "Identity", "Medical", "Address", "Other"
    public string Description { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = "application/pdf";
    public long FileSizeBytes { get; set; } = 0;
    public string FileSizeFormatted { get; set; } = "1.2 MB";
    public string? FileUrl { get; set; }
    public string? FileData { get; set; } // Base64 data or content representation
    public string Status { get; set; } = "Saved"; // "Verified", "Saved", "Under review"
    public string? VerificationNotes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
