namespace CareLink.Application.Interfaces;

public interface IEmailService
{
    /// <summary>
    /// Sends a branded CareLink email verification message to a registered user.
    /// </summary>
    Task<bool> SendVerificationEmailAsync(string recipientEmail, string recipientName, string confirmationUrl);

    /// <summary>
    /// Sends a generic HTML/plain-text email to a specified recipient.
    /// </summary>
    Task<bool> SendEmailAsync(string toEmail, string subject, string htmlContent, string? plainTextContent = null);
}
