namespace CareLink.Application.Common;

public static class EmailFailureCategories
{
    public const string None = "None";
    public const string ConfigurationMissing = "ConfigurationMissing";
    public const string InvalidSender = "InvalidSender";
    public const string InvalidRecipient = "InvalidRecipient";
    public const string AuthenticationFailure = "AuthenticationFailure";
    public const string TlsFailure = "TlsFailure";
    public const string SmtpConnectionFailure = "SmtpConnectionFailure";
    public const string ServerRejection = "ServerRejection";
    public const string Timeout = "Timeout";
    public const string UnexpectedError = "UnexpectedError";
}

public class EmailDeliveryResult
{
    public bool Success { get; set; }
    public string FailureCategory { get; set; } = EmailFailureCategories.None;
    public string DiagnosticMessage { get; set; } = string.Empty;
    public string? SmtpStatusCode { get; set; }
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

    public static EmailDeliveryResult Succeeded(string message = "Email accepted by SMTP server.") => new()
    {
        Success = true,
        FailureCategory = EmailFailureCategories.None,
        DiagnosticMessage = message,
        TimestampUtc = DateTime.UtcNow
    };

    public static EmailDeliveryResult Failed(string category, string diagnosticMessage, string? smtpStatusCode = null) => new()
    {
        Success = false,
        FailureCategory = category,
        DiagnosticMessage = diagnosticMessage,
        SmtpStatusCode = smtpStatusCode,
        TimestampUtc = DateTime.UtcNow
    };
}

public class EmailDiagnosticsDto
{
    public string SenderName { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string Username { get; set; } = string.Empty;
    public bool EnableSsl { get; set; }
    public bool IsPasswordConfigured { get; set; }
    public bool IsAppPasswordFormatValid { get; set; }
    public int ConfiguredPasswordLength { get; set; }
    public bool SenderMatchesUsername { get; set; }
    public string StatusSummary { get; set; } = string.Empty;
}

public class TestEmailRequestDto
{
    public string RecipientEmail { get; set; } = string.Empty;
}

public class ConfigureSmtpPasswordRequestDto
{
    public string AppPassword { get; set; } = string.Empty;
}
