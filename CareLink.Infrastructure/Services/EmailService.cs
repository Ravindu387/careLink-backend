using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using System.Text.Json;
using CareLink.Application.Common;
using CareLink.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CareLink.Infrastructure.Services;

public class EmailService : IEmailService
{
    private const string UserSecretsId = "d67494ae-8e76-4d56-865c-56b0e66af492";

    private readonly IOptionsMonitor<EmailSettings>? _settingsMonitor;
    private readonly EmailSettings _staticSettings;
    private readonly ILogger<EmailService> _logger;
    private readonly bool _allowAmbientSecretFallback;

    public EmailService(
        IOptions<EmailSettings> settings,
        ILogger<EmailService> logger,
        bool allowAmbientSecretFallback = true)
    {
        _staticSettings = settings.Value;
        _logger = logger;
        _allowAmbientSecretFallback = allowAmbientSecretFallback;
    }

    public EmailService(
        IOptionsMonitor<EmailSettings> settingsMonitor,
        IOptions<EmailSettings> settings,
        ILogger<EmailService> logger,
        bool allowAmbientSecretFallback = true)
    {
        _settingsMonitor = settingsMonitor;
        _staticSettings = settings.Value;
        _logger = logger;
        _allowAmbientSecretFallback = allowAmbientSecretFallback;
    }

    private EmailSettings CurrentSettings => _settingsMonitor?.CurrentValue ?? _staticSettings;

    public async Task<bool> SendVerificationEmailAsync(string recipientEmail, string recipientName, string confirmationUrl)
    {
        var subject = EmailTemplates.GetVerificationEmailSubject();
        var htmlBody = EmailTemplates.GetVerificationEmailHtml(recipientName, confirmationUrl);
        var plainTextBody = EmailTemplates.GetVerificationEmailPlainText(recipientName, confirmationUrl);

        return await SendEmailAsync(recipientEmail, subject, htmlBody, plainTextBody);
    }

    public async Task<bool> SendCitizenWelcomeEmailAsync(string recipientEmail, string recipientName)
    {
        var subject = EmailTemplates.GetCitizenWelcomeSubject();
        var htmlBody = EmailTemplates.GetCitizenWelcomeHtml(recipientName, recipientEmail);
        var plainTextBody = EmailTemplates.GetCitizenWelcomePlainText(recipientName, recipientEmail);

        return await SendEmailAsync(recipientEmail, subject, htmlBody, plainTextBody);
    }

    public async Task<bool> SendVolunteerWelcomeEmailAsync(string recipientEmail, string recipientName, string? skills = null, string? availability = null)
    {
        var subject = EmailTemplates.GetVolunteerWelcomeSubject();
        var htmlBody = EmailTemplates.GetVolunteerWelcomeHtml(recipientName, recipientEmail, skills, availability);
        var plainTextBody = EmailTemplates.GetVolunteerWelcomePlainText(recipientName, recipientEmail, skills, availability);

        return await SendEmailAsync(recipientEmail, subject, htmlBody, plainTextBody);
    }

    public async Task<bool> SendPartnerPendingApprovalEmailAsync(string recipientEmail, string recipientName, string organizationName)
    {
        var subject = EmailTemplates.GetPartnerPendingSubject();
        var htmlBody = EmailTemplates.GetPartnerPendingHtml(recipientName, recipientEmail, organizationName);
        var plainTextBody = EmailTemplates.GetPartnerPendingPlainText(recipientName, recipientEmail, organizationName);

        return await SendEmailAsync(recipientEmail, subject, htmlBody, plainTextBody);
    }

    public async Task<bool> SendPartnerApprovalEmailAsync(string recipientEmail, string recipientName, string organizationName, string loginUrl)
    {
        var subject = EmailTemplates.GetPartnerApprovedSubject();
        var htmlBody = EmailTemplates.GetPartnerApprovedHtml(recipientName, recipientEmail, organizationName, loginUrl);
        var plainTextBody = EmailTemplates.GetPartnerApprovedPlainText(recipientName, recipientEmail, organizationName, loginUrl);

        return await SendEmailAsync(recipientEmail, subject, htmlBody, plainTextBody);
    }

    public async Task<bool> SendPartnerRejectionEmailAsync(string recipientEmail, string recipientName, string organizationName, string? rejectionReason)
    {
        var subject = EmailTemplates.GetPartnerRejectedSubject();
        var htmlBody = EmailTemplates.GetPartnerRejectedHtml(recipientName, recipientEmail, organizationName, rejectionReason);
        var plainTextBody = EmailTemplates.GetPartnerRejectedPlainText(recipientName, recipientEmail, organizationName, rejectionReason);

        return await SendEmailAsync(recipientEmail, subject, htmlBody, plainTextBody);
    }

    public async Task<bool> SendPartnerAdditionalInfoEmailAsync(string recipientEmail, string recipientName, string organizationName, string additionalInfoRequest)
    {
        var subject = EmailTemplates.GetPartnerAdditionalInfoSubject();
        var htmlBody = EmailTemplates.GetPartnerAdditionalInfoHtml(recipientName, recipientEmail, organizationName, additionalInfoRequest);
        var plainTextBody = EmailTemplates.GetPartnerAdditionalInfoPlainText(recipientName, recipientEmail, organizationName, additionalInfoRequest);

        return await SendEmailAsync(recipientEmail, subject, htmlBody, plainTextBody);
    }

    public async Task<bool> SendAdminNewPartnerNotificationEmailAsync(
        string adminEmail,
        string organizationName,
        string organizationType,
        string representativeName,
        string representativeEmail)
    {
        var subject = EmailTemplates.GetAdminNewPartnerSubject(organizationName);
        var htmlBody = EmailTemplates.GetAdminNewPartnerNotificationHtml(organizationName, organizationType, representativeName, representativeEmail);
        var plainTextBody = EmailTemplates.GetAdminNewPartnerNotificationPlainText(organizationName, organizationType, representativeName, representativeEmail);

        return await SendEmailAsync(adminEmail, subject, htmlBody, plainTextBody);
    }

    public async Task<bool> SendAdminCreatedUserEmailAsync(string recipientEmail, string recipientName, string assignedRole, string setupPasswordUrl)
    {
        var result = await SendAdminCreatedUserEmailWithResultAsync(recipientEmail, recipientName, assignedRole, setupPasswordUrl);
        return result.Success;
    }

    public async Task<EmailDeliveryResult> SendAdminCreatedUserEmailWithResultAsync(
        string recipientEmail,
        string recipientName,
        string assignedRole,
        string setupPasswordUrl,
        CancellationToken cancellationToken = default)
    {
        var subject = EmailTemplates.GetAdminCreatedUserSubject();
        var htmlBody = EmailTemplates.GetAdminCreatedUserHtml(recipientName, recipientEmail, assignedRole, setupPasswordUrl);
        var plainTextBody = EmailTemplates.GetAdminCreatedUserPlainText(recipientName, recipientEmail, assignedRole, setupPasswordUrl);

        return await SendEmailWithResultAsync(recipientEmail, subject, htmlBody, plainTextBody, cancellationToken);
    }

    public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlContent, string? plainTextContent = null)
    {
        var result = await SendEmailWithResultAsync(toEmail, subject, htmlContent, plainTextContent);
        return result.Success;
    }

    public async Task<EmailDeliveryResult> SendTestEmailAsync(string recipientEmail, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(recipientEmail))
        {
            return EmailDeliveryResult.Failed(
                EmailFailureCategories.InvalidRecipient,
                "Recipient email address is required for SMTP test.");
        }

        var settings = CurrentSettings;
        var subject = "CareLink SMTP Configuration Test";
        var htmlContent = $@"<!DOCTYPE html>
<html>
<body style=""font-family: sans-serif; color: #123B5C; padding: 24px;"">
  <h2>CareLink Gmail SMTP Test Successful</h2>
  <p>This test message confirms that CareLink is properly configured to send emails via <strong>{WebUtility.HtmlEncode(settings.SmtpHost)}:{settings.SmtpPort}</strong> using TLS/STARTTLS.</p>
  <p><strong>Sender:</strong> {WebUtility.HtmlEncode(settings.SenderName)} &lt;{WebUtility.HtmlEncode(settings.SenderEmail)}&gt;</p>
  <p><strong>Timestamp (UTC):</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>
</body>
</html>";
        var plainText = $"CareLink Gmail SMTP Test Successful. Sent via {settings.SmtpHost}:{settings.SmtpPort} at {DateTime.UtcNow:O}.";

        return await SendEmailWithResultAsync(recipientEmail.Trim(), subject, htmlContent, plainText, cancellationToken);
    }

    public EmailDiagnosticsDto GetDiagnostics()
    {
        var settings = CurrentSettings;
        var smtpServer = settings.SmtpHost?.Trim() ?? string.Empty;
        var senderEmail = settings.SenderEmail?.Trim() ?? string.Empty;
        var username = settings.Username?.Trim() ?? string.Empty;
        var normalizedPassword = ResolveNormalizedPassword(settings);

        var isPasswordConfigured = !string.IsNullOrWhiteSpace(normalizedPassword);
        var isAppPasswordFormatValid = isPasswordConfigured &&
                                       normalizedPassword.Length == 16 &&
                                       normalizedPassword.All(char.IsLetterOrDigit);
        var senderMatchesUsername = !string.IsNullOrWhiteSpace(senderEmail) &&
                                    string.Equals(senderEmail, username, StringComparison.OrdinalIgnoreCase);

        string summary;
        if (string.IsNullOrWhiteSpace(smtpServer))
        {
            summary = "Missing Configuration: EmailSettings:SmtpServer is not configured.";
        }
        else if (!isPasswordConfigured)
        {
            summary = "Missing Secret: EmailSettings:Password (Gmail App Password) is not configured in .NET User Secrets or Environment Variables.";
        }
        else if (!isAppPasswordFormatValid)
        {
            summary = $"Warning: Configured password length is {normalizedPassword.Length} characters (expected a 16-character Gmail App Password).";
        }
        else if (!senderMatchesUsername)
        {
            summary = "Warning: SenderEmail does not match SMTP Username for Gmail authentication.";
        }
        else
        {
            summary = "Ready: Gmail SMTP server, sender, username, TLS, and 16-character App Password are configured.";
        }

        return new EmailDiagnosticsDto
        {
            SenderName = settings.SenderName?.Trim() ?? string.Empty,
            SenderEmail = senderEmail,
            SmtpServer = smtpServer,
            SmtpPort = settings.SmtpPort,
            Username = username,
            EnableSsl = settings.EnableSsl,
            IsPasswordConfigured = isPasswordConfigured,
            IsAppPasswordFormatValid = isAppPasswordFormatValid,
            ConfiguredPasswordLength = normalizedPassword.Length,
            SenderMatchesUsername = senderMatchesUsername,
            StatusSummary = summary
        };
    }

    public async Task<EmailDeliveryResult> SendEmailWithResultAsync(
        string toEmail,
        string subject,
        string htmlContent,
        string? plainTextContent = null,
        CancellationToken cancellationToken = default)
    {
        var settings = CurrentSettings;

        // 1. Validate Recipient
        if (string.IsNullOrWhiteSpace(toEmail))
        {
            _logger.LogWarning("[EMAIL_DIAGNOSTIC] Category={Category} | Recipient email is empty.", EmailFailureCategories.InvalidRecipient);
            return EmailDeliveryResult.Failed(
                EmailFailureCategories.InvalidRecipient,
                "Recipient email address is required.");
        }

        MailAddress recipientAddress;
        try
        {
            recipientAddress = new MailAddress(toEmail.Trim());
        }
        catch (FormatException)
        {
            _logger.LogWarning(
                "[EMAIL_DIAGNOSTIC] Category={Category} | Recipient email '{ToEmail}' has an invalid format.",
                EmailFailureCategories.InvalidRecipient,
                toEmail);
            return EmailDeliveryResult.Failed(
                EmailFailureCategories.InvalidRecipient,
                $"Recipient email address '{toEmail}' is not a valid email format.");
        }

        // 2. Validate SMTP Host & Port Configuration
        var smtpHost = settings.SmtpHost?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(smtpHost))
        {
            _logger.LogWarning(
                "[EMAIL_DIAGNOSTIC] Category={Category} | Cannot send email to '{ToEmail}' because EmailSettings:SmtpServer is not configured.",
                EmailFailureCategories.ConfigurationMissing,
                recipientAddress.Address);
            return EmailDeliveryResult.Failed(
                EmailFailureCategories.ConfigurationMissing,
                "SMTP server (EmailSettings:SmtpServer) is not configured.");
        }

        if (settings.SmtpPort <= 0 || settings.SmtpPort > 65535)
        {
            return EmailDeliveryResult.Failed(
                EmailFailureCategories.ConfigurationMissing,
                $"Invalid SMTP port configured: {settings.SmtpPort}. Expected port 587 for Gmail STARTTLS.");
        }

        // 3. Validate Sender & Username
        var rawSenderEmail = string.IsNullOrWhiteSpace(settings.SenderEmail)
            ? "carelink.platform.admin@gmail.com"
            : settings.SenderEmail.Trim();
        var senderName = string.IsNullOrWhiteSpace(settings.SenderName)
            ? "CareLink Admin"
            : settings.SenderName.Trim();
        var smtpUsername = string.IsNullOrWhiteSpace(settings.Username)
            ? rawSenderEmail
            : settings.Username.Trim();

        MailAddress senderAddress;
        try
        {
            senderAddress = new MailAddress(rawSenderEmail, senderName, Encoding.UTF8);
        }
        catch (FormatException)
        {
            _logger.LogWarning(
                "[EMAIL_DIAGNOSTIC] Category={Category} | Configured SenderEmail '{SenderEmail}' is invalid.",
                EmailFailureCategories.InvalidSender,
                rawSenderEmail);
            return EmailDeliveryResult.Failed(
                EmailFailureCategories.InvalidSender,
                $"Configured SenderEmail '{rawSenderEmail}' is not a valid email address.");
        }

        if (smtpHost.Contains("gmail.com", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(senderAddress.Address, smtpUsername, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(
                "[EMAIL_DIAGNOSTIC] Category={Category} | Gmail SMTP requires SenderEmail ('{SenderEmail}') to match authenticated Username ('{Username}').",
                EmailFailureCategories.InvalidSender,
                senderAddress.Address,
                smtpUsername);
            return EmailDeliveryResult.Failed(
                EmailFailureCategories.InvalidSender,
                $"SenderEmail ('{senderAddress.Address}') does not match SMTP Username ('{smtpUsername}').");
        }

        // 4. Resolve & Normalize App Password (strip spaces accidentally copied from Google's 4x4 App Password display)
        var normalizedPassword = ResolveNormalizedPassword(settings);
        if (string.IsNullOrWhiteSpace(normalizedPassword))
        {
            _logger.LogWarning(
                "[EMAIL_DIAGNOSTIC] Category={Category} | Cannot send email to '{ToEmail}' via '{SmtpHost}:{SmtpPort}' because EmailSettings:Password (Gmail App Password) is missing in .NET User Secrets and Environment Variables.",
                EmailFailureCategories.ConfigurationMissing,
                recipientAddress.Address,
                smtpHost,
                settings.SmtpPort);
            return EmailDeliveryResult.Failed(
                EmailFailureCategories.ConfigurationMissing,
                "EmailSettings:Password (Gmail App Password) is not configured in .NET User Secrets. Run 'dotnet user-secrets set \"EmailSettings:Password\" \"<16-char-app-password>\"' in CareLink.API.");
        }

        try
        {
            using var message = new MailMessage
            {
                From = senderAddress,
                Subject = subject,
                SubjectEncoding = Encoding.UTF8,
                Body = htmlContent,
                BodyEncoding = Encoding.UTF8,
                HeadersEncoding = Encoding.UTF8,
                IsBodyHtml = true
            };

            message.To.Add(recipientAddress);

            if (!string.IsNullOrWhiteSpace(plainTextContent))
            {
                var plainView = AlternateView.CreateAlternateViewFromString(plainTextContent, Encoding.UTF8, "text/plain");
                var htmlView = AlternateView.CreateAlternateViewFromString(htmlContent, Encoding.UTF8, "text/html");
                message.AlternateViews.Add(plainView);
                message.AlternateViews.Add(htmlView);
            }

            using var client = new SmtpClient(smtpHost, settings.SmtpPort);
            client.EnableSsl = settings.EnableSsl;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(smtpUsername, normalizedPassword);
            client.Timeout = 15000;

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(15));

            await client.SendMailAsync(message, timeoutCts.Token);

            _logger.LogInformation(
                "[EMAIL_DELIVERED] Successfully delivered email to '{ToEmail}' via '{SmtpHost}:{SmtpPort}' (TLS={EnableSsl}) | Subject: '{Subject}'",
                recipientAddress.Address,
                smtpHost,
                settings.SmtpPort,
                settings.EnableSsl,
                subject);

            return EmailDeliveryResult.Succeeded(
                $"Email successfully delivered to '{recipientAddress.Address}' via '{smtpHost}:{settings.SmtpPort}'.");
        }
        catch (SmtpFailedRecipientException recipientEx)
        {
            _logger.LogError(
                "[EMAIL_DIAGNOSTIC] Category={Category} | SMTP server rejected recipient '{ToEmail}'. StatusCode={StatusCode}",
                EmailFailureCategories.ServerRejection,
                recipientAddress.Address,
                recipientEx.StatusCode);

            return EmailDeliveryResult.Failed(
                EmailFailureCategories.ServerRejection,
                $"SMTP server rejected recipient '{recipientAddress.Address}' ({recipientEx.StatusCode}).",
                recipientEx.StatusCode.ToString());
        }
        catch (SmtpException smtpEx)
        {
            var (category, detail) = ClassifySmtpException(smtpEx, smtpHost, settings.SmtpPort, smtpUsername);
            _logger.LogError(
                "[EMAIL_DIAGNOSTIC] Category={Category} | Failed to deliver email to '{ToEmail}' via '{SmtpHost}:{SmtpPort}'. StatusCode={StatusCode} | Detail={Detail}",
                category,
                recipientAddress.Address,
                smtpHost,
                settings.SmtpPort,
                smtpEx.StatusCode,
                detail);

            return EmailDeliveryResult.Failed(category, detail, smtpEx.StatusCode.ToString());
        }
        catch (AuthenticationException)
        {
            _logger.LogError(
                "[EMAIL_DIAGNOSTIC] Category={Category} | TLS/STARTTLS handshake failed with '{SmtpHost}:{SmtpPort}'.",
                EmailFailureCategories.TlsFailure,
                smtpHost,
                settings.SmtpPort);

            return EmailDeliveryResult.Failed(
                EmailFailureCategories.TlsFailure,
                $"TLS/STARTTLS negotiation failed with '{smtpHost}:{settings.SmtpPort}'. Ensure port 587 and EnableSsl=true are configured.");
        }
        catch (SocketException sockEx)
        {
            _logger.LogError(
                "[EMAIL_DIAGNOSTIC] Category={Category} | Network connection failed to '{SmtpHost}:{SmtpPort}'. SocketError={SocketError}",
                EmailFailureCategories.SmtpConnectionFailure,
                smtpHost,
                settings.SmtpPort,
                sockEx.SocketErrorCode);

            return EmailDeliveryResult.Failed(
                EmailFailureCategories.SmtpConnectionFailure,
                $"Could not establish a network connection to '{smtpHost}:{settings.SmtpPort}' ({sockEx.SocketErrorCode}).");
        }
        catch (OperationCanceledException)
        {
            _logger.LogError(
                "[EMAIL_DIAGNOSTIC] Category={Category} | SMTP operation timed out or was cancelled for '{ToEmail}'.",
                EmailFailureCategories.Timeout,
                recipientAddress.Address);

            return EmailDeliveryResult.Failed(
                EmailFailureCategories.Timeout,
                $"SMTP request to '{smtpHost}:{settings.SmtpPort}' timed out.");
        }
        catch (TimeoutException)
        {
            _logger.LogError(
                "[EMAIL_DIAGNOSTIC] Category={Category} | SMTP connection timed out for '{ToEmail}'.",
                EmailFailureCategories.Timeout,
                recipientAddress.Address);

            return EmailDeliveryResult.Failed(
                EmailFailureCategories.Timeout,
                $"Connection to '{smtpHost}:{settings.SmtpPort}' timed out after 15 seconds.");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                "[EMAIL_DIAGNOSTIC] Category={Category} | Unexpected error while sending email to '{ToEmail}': {ErrorType}",
                EmailFailureCategories.UnexpectedError,
                recipientAddress.Address,
                ex.GetType().Name);

            return EmailDeliveryResult.Failed(
                EmailFailureCategories.UnexpectedError,
                $"Unexpected email delivery error ({ex.GetType().Name}).");
        }
    }

    private string ResolveNormalizedPassword(EmailSettings settings)
    {
        var raw = settings.Password;

        if (string.IsNullOrWhiteSpace(raw) && _allowAmbientSecretFallback)
        {
            raw = Environment.GetEnvironmentVariable("EmailSettings__Password")
                  ?? Environment.GetEnvironmentVariable("EmailSettings:Password")
                  ?? ReadPasswordFromUserSecretsFile();
        }

        if (string.IsNullOrWhiteSpace(raw))
        {
            return string.Empty;
        }

        // Strip spaces (Google displays 16-char App Passwords in 4 groups of 4 chars with spaces) and trim
        return raw.Replace(" ", string.Empty).Trim();
    }

    private static string? ReadPasswordFromUserSecretsFile()
    {
        try
        {
            var candidatePaths = new List<string>();
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (!string.IsNullOrWhiteSpace(appData))
            {
                candidatePaths.Add(Path.Combine(appData, "Microsoft", "UserSecrets", UserSecretsId, "secrets.json"));
            }

            candidatePaths.Add(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.local.json"));
            candidatePaths.Add(Path.Combine(AppContext.BaseDirectory, "appsettings.local.json"));

            foreach (var secretsPath in candidatePaths)
            {
                if (!File.Exists(secretsPath)) continue;

                using var doc = JsonDocument.Parse(File.ReadAllText(secretsPath));
                var root = doc.RootElement;

                if (root.TryGetProperty("EmailSettings:Password", out var flatProp) && flatProp.ValueKind == JsonValueKind.String)
                {
                    var val = flatProp.GetString();
                    if (!string.IsNullOrWhiteSpace(val)) return val;
                }

                if (root.TryGetProperty("EmailSettings", out var nestedObj) &&
                    nestedObj.ValueKind == JsonValueKind.Object &&
                    nestedObj.TryGetProperty("Password", out var nestedProp) &&
                    nestedProp.ValueKind == JsonValueKind.String)
                {
                    var val = nestedProp.GetString();
                    if (!string.IsNullOrWhiteSpace(val)) return val;
                }
            }
        }
        catch
        {
            // Ignore file read errors
        }

        return null;
    }

    private static (string Category, string Detail) ClassifySmtpException(
        SmtpException smtpEx,
        string smtpHost,
        int smtpPort,
        string username)
    {
        var msg = smtpEx.Message ?? string.Empty;

        // Traverse inner exceptions to accurately identify transport, DNS, and TLS errors
        var curr = smtpEx.InnerException;
        while (curr != null)
        {
            if (curr is AuthenticationException authEx)
            {
                return (
                    EmailFailureCategories.TlsFailure,
                    $"TLS/SSL negotiation with '{smtpHost}:{smtpPort}' failed: {authEx.Message}");
            }

            if (curr is SocketException sockEx)
            {
                return (
                    EmailFailureCategories.SmtpConnectionFailure,
                    $"Unable to connect to '{smtpHost}:{smtpPort}' ({sockEx.SocketErrorCode}): {sockEx.Message}");
            }

            if (curr is WebException webEx)
            {
                return (
                    EmailFailureCategories.SmtpConnectionFailure,
                    $"Network or DNS resolution error reaching '{smtpHost}:{smtpPort}': {webEx.Message}");
            }

            curr = curr.InnerException;
        }

        if (msg.Contains("5.7.8", StringComparison.OrdinalIgnoreCase) ||
            msg.Contains("5.7.9", StringComparison.OrdinalIgnoreCase) ||
            msg.Contains("5.7.0", StringComparison.OrdinalIgnoreCase) ||
            msg.Contains("535", StringComparison.OrdinalIgnoreCase) ||
            msg.Contains("Authentication", StringComparison.OrdinalIgnoreCase) ||
            msg.Contains("Username and Password not accepted", StringComparison.OrdinalIgnoreCase) ||
            msg.Contains("Application-specific password required", StringComparison.OrdinalIgnoreCase) ||
            smtpEx.StatusCode == SmtpStatusCode.ClientNotPermitted)
        {
            return (
                EmailFailureCategories.AuthenticationFailure,
                $"Gmail SMTP authentication failed for '{username}'. Ensure 2-Step Verification is enabled on the Google account and a valid 16-character Gmail App Password is configured in EmailSettings:Password.");
        }

        if (msg.Contains("timed out", StringComparison.OrdinalIgnoreCase) ||
            msg.Contains("timeout", StringComparison.OrdinalIgnoreCase))
        {
            return (
                EmailFailureCategories.Timeout,
                $"SMTP operation to '{smtpHost}:{smtpPort}' timed out.");
        }

        if (smtpEx.StatusCode == SmtpStatusCode.GeneralFailure)
        {
            var rootMsg = smtpEx.InnerException?.Message ?? smtpEx.Message;
            return (
                EmailFailureCategories.SmtpConnectionFailure,
                $"Connection to '{smtpHost}:{smtpPort}' failed: {rootMsg}");
        }

        return (
            EmailFailureCategories.ServerRejection,
            $"SMTP server '{smtpHost}:{smtpPort}' rejected the message (StatusCode: {smtpEx.StatusCode}). Message: {smtpEx.Message}");
    }
}
