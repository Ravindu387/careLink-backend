using CareLink.Application.Common;

namespace CareLink.Application.Interfaces;

public interface IEmailService
{
    Task<bool> SendVerificationEmailAsync(string recipientEmail, string recipientName, string confirmationUrl);

    Task<bool> SendCitizenWelcomeEmailAsync(string recipientEmail, string recipientName);

    Task<bool> SendVolunteerWelcomeEmailAsync(string recipientEmail, string recipientName, string? skills = null, string? availability = null);

    Task<bool> SendPartnerPendingApprovalEmailAsync(string recipientEmail, string recipientName, string organizationName);

    Task<bool> SendPartnerApprovalEmailAsync(string recipientEmail, string recipientName, string organizationName, string loginUrl);

    Task<bool> SendPartnerRejectionEmailAsync(string recipientEmail, string recipientName, string organizationName, string? rejectionReason);

    Task<bool> SendPartnerAdditionalInfoEmailAsync(string recipientEmail, string recipientName, string organizationName, string additionalInfoRequest);

    Task<bool> SendAdminNewPartnerNotificationEmailAsync(string adminEmail, string organizationName, string organizationType, string representativeName, string representativeEmail);

    Task<bool> SendAdminCreatedUserEmailAsync(string recipientEmail, string recipientName, string assignedRole, string setupPasswordUrl);

    Task<EmailDeliveryResult> SendAdminCreatedUserEmailWithResultAsync(
        string recipientEmail,
        string recipientName,
        string assignedRole,
        string setupPasswordUrl,
        CancellationToken cancellationToken = default);

    Task<bool> SendEmailAsync(string toEmail, string subject, string htmlContent, string? plainTextContent = null);

    Task<EmailDeliveryResult> SendEmailWithResultAsync(
        string toEmail,
        string subject,
        string htmlContent,
        string? plainTextContent = null,
        CancellationToken cancellationToken = default);

    Task<EmailDeliveryResult> SendTestEmailAsync(string recipientEmail, CancellationToken cancellationToken = default);

    EmailDiagnosticsDto GetDiagnostics();
}
