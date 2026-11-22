namespace CareLink.Application.DTOs.Verification;

public class VerificationDocumentDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "PDF";
    public string Size { get; set; } = "1.2 MB";
    public string Status { get; set; } = "Ready";
    public DateTime UploadedAt { get; set; }
}
