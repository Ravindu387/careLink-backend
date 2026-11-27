namespace CareLink.Core.Entities;

public class OrganizationDocument
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrganizationId { get; set; }
    public Organization? Organization { get; set; }

    /// <summary>
    /// Document category: RegistrationCertificate, OrganisationProfile, AuthorisationLetter, SupportingDocument
    /// </summary>
    public string DocumentCategory { get; set; } = "RegistrationCertificate";

    /// <summary>
    /// Human-readable label for the document category.
    /// </summary>
    public string DocumentLabel { get; set; } = "Organisation Registration Certificate";

    /// <summary>
    /// Sanitized original filename uploaded by the representative.
    /// </summary>
    public string OriginalFileName { get; set; } = string.Empty;

    /// <summary>
    /// Unique randomized storage filename on disk.
    /// </summary>
    public string StoredFileName { get; set; } = string.Empty;

    /// <summary>
    /// Validated MIME content type (application/pdf, image/jpeg, image/png).
    /// </summary>
    public string ContentType { get; set; } = "application/pdf";

    /// <summary>
    /// File size in bytes.
    /// </summary>
    public long FileSizeBytes { get; set; }

    /// <summary>
    /// Formatted file size (e.g., "245 KB", "1.4 MB").
    /// </summary>
    public string FileSizeDisplay { get; set; } = string.Empty;

    /// <summary>
    /// Private non-public storage path inside App_Data/OrganizationDocuments.
    /// </summary>
    public string StoragePath { get; set; } = string.Empty;

    /// <summary>
    /// Binary payload fallback so documents remain accessible across environments and tests.
    /// </summary>
    public byte[]? FileContent { get; set; }

    public string? UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
