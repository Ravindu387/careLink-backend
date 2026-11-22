using System.Net;
using System.Net.Mail;
using CareLink.Application.Common;
using CareLink.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CareLink.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<bool> SendVerificationEmailAsync(string recipientEmail, string recipientName, string confirmationUrl)
    {
        var subject = EmailTemplates.GetVerificationEmailSubject();
        var htmlBody = EmailTemplates.GetVerificationEmailHtml(recipientName, confirmationUrl);
        var plainTextBody = EmailTemplates.GetVerificationEmailPlainText(recipientName, confirmationUrl);

        return await SendEmailAsync(recipientEmail, subject, htmlBody, plainTextBody);
    }

    public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlContent, string? plainTextContent = null)
    {
        // 1. Guard against empty/unconfigured SMTP settings
        if (string.IsNullOrWhiteSpace(_settings.SmtpHost))
        {
            _logger.LogWarning(
                "[SMTP_UNCONFIGURED] Cannot dispatch email to '{ToEmail}' because EmailSettings:SmtpHost is not configured. Subject: '{Subject}'",
                toEmail,
                subject);
            return false;
        }

        try
        {
            using var message = new MailMessage
            {
                From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                Subject = subject,
                Body = htmlContent,
                IsBodyHtml = true
            };

            message.To.Add(new MailAddress(toEmail));

            if (!string.IsNullOrWhiteSpace(plainTextContent))
            {
                var plainView = AlternateView.CreateAlternateViewFromString(plainTextContent, null, "text/plain");
                var htmlView = AlternateView.CreateAlternateViewFromString(htmlContent, null, "text/html");
                message.AlternateViews.Add(plainView);
                message.AlternateViews.Add(htmlView);
            }

            using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
            {
                EnableSsl = _settings.EnableSsl,
                Timeout = 15000 // 15 seconds timeout
            };

            if (!string.IsNullOrWhiteSpace(_settings.Username) && !string.IsNullOrWhiteSpace(_settings.Password))
            {
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(_settings.Username, _settings.Password);
            }
            else
            {
                client.UseDefaultCredentials = true;
            }

            await client.SendMailAsync(message);

            _logger.LogInformation("Successfully dispatched email to '{ToEmail}' with subject '{Subject}'", toEmail, subject);
            return true;
        }
        catch (SmtpException smtpEx)
        {
            _logger.LogError(
                smtpEx,
                "[SMTP_ERROR] Failed to send email to '{ToEmail}'. StatusCode: {StatusCode}, Message: {ErrorMessage}",
                toEmail,
                smtpEx.StatusCode,
                smtpEx.Message);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "[EMAIL_ERROR] Unexpected error while sending email to '{ToEmail}'. Message: {ErrorMessage}",
                toEmail,
                ex.Message);
            return false;
        }
    }
}
