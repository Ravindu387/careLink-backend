namespace CareLink.Application.DTOs.Verification;

public class VerificationDocumentDto
{
    public string Id { get; set; } = string.Empty;
    public Guid? DocumentId { get; set; }
    public Guid? OrganizationId { get; set; }
    public string Category { get; set; } = "RegistrationCertificate";
    public string Name { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string Type { get; set; } = "PDF";
    public string ContentType { get; set; } = "application/pdf";
    public string Size { get; set; } = "1.2 MB";
    public long FileSizeBytes { get; set; }
    public string Status { get; set; } = "Uploaded";
    public bool CanDownload { get; set; }
    public DateTime UploadedAt { get; set; }
}
