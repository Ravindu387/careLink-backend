namespace CareLink.Core.Entities;

public class VerificationDocument
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "PDF";
    public string Size { get; set; } = "1.2 MB";
    public string Status { get; set; } = "Ready"; // Ready, Verified, Under Review
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}

public class VerificationRequest
{
    public string Id { get; set; } = string.Empty; // e.g. VER-2048
    public string Applicant { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Partner Organisation, Partner Officer, Medical Partner, Resource Manager, Volunteer
    public string Evidence { get; set; } = string.Empty; // Summary string e.g. "Registration · Tax · Safeguarding"
    public string Priority { get; set; } = "Standard"; // High, Standard
    public string Status { get; set; } = "Pending Review"; // Pending Review, Additional Info, Approved, Rejected
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public List<VerificationDocument> Documents { get; set; } = new();

    public string? ApplicantEmail { get; set; }
    public string? ApplicantPhone { get; set; }
    public string? OrganizationName { get; set; }
    public string? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? DecisionRationale { get; set; }
}
