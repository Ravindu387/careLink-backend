namespace CareLink.Application.Common;

public class EmailSettings
{
    public const string SectionName = "EmailSettings";

    private string _smtpHost = string.Empty;

    public string SenderName { get; set; } = "CareLink Admin";
    public string SenderEmail { get; set; } = "carelink.platform.admin@gmail.com";
    public string SmtpServer { get; set; } = string.Empty;

    public string SmtpHost
    {
        get => !string.IsNullOrWhiteSpace(SmtpServer) ? SmtpServer : _smtpHost;
        set => _smtpHost = value;
    }

    public int SmtpPort { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;
    public int SetupTokenExpiryHours { get; set; } = 24;
    public string VerificationBaseUrl { get; set; } = "http://localhost:5277/api/auth/verify-email";
    public string FrontendBaseUrl { get; set; } = "http://localhost:5173";
    public string PasswordSetupBaseUrl { get; set; } = "http://localhost:5173/setup-account";
}
