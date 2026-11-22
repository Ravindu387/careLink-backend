namespace CareLink.Application.Common;

public class EmailSettings
{
    public const string SectionName = "EmailSettings";

    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string SenderEmail { get; set; } = "noreply@carelink.org";
    public string SenderName { get; set; } = "CareLink Humanitarian Platform";
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;
    public string VerificationBaseUrl { get; set; } = "http://localhost:5277/api/auth/verify-email";
}
