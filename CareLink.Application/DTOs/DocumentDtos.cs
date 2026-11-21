namespace CareLink.Application.DTOs;

public class CitizenDocumentDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = "Identity";
    public string Description { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Status { get; set; } = "Saved";
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string FileSizeFormatted { get; set; } = string.Empty;
    public string? FileUrl { get; set; }
    public string? FileData { get; set; }
    public string? VerificationNotes { get; set; }
    public string CitizenName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateDocumentRequest
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = "Identity";
    public string? Description { get; set; }
    public string? FileName { get; set; }
    public string? FileType { get; set; }
    public long? FileSizeBytes { get; set; }
    public string? FileData { get; set; } // Base64 data if uploaded
    public string? CitizenName { get; set; }
}

public class UpdateDocumentStatusRequest
{
    public string Status { get; set; } = "Saved"; // "Verified", "Saved", "Under review"
    public string? VerificationNotes { get; set; }
}

public class DocumentSummaryDto
{
    public int TotalDocuments { get; set; }
    public int VerifiedCount { get; set; }
    public int UnderReviewCount { get; set; }
    public int SavedCount { get; set; }
}
