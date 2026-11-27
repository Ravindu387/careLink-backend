using CareLink.Core.Constants;

namespace CareLink.Application.DTOs.Auth;

public class RegisterResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string AccountStatus { get; set; } = AccountStatuses.Active;
    public bool IsActive { get; set; } = true;
    public bool EmailConfirmed { get; set; } = true;
    public bool RequiresEmailVerification { get; set; } = false;
    public bool RequiresAdminApproval { get; set; } = false;
    public Guid? OrganizationId { get; set; }
    public string? OrganizationName { get; set; }
    public string? VerificationRequestId { get; set; }
    public bool EmailSent { get; set; }
    public bool AdminNotificationSent { get; set; }
    public int UploadedDocumentsCount { get; set; }
    public string? Token { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string Message { get; set; } = "Registration completed successfully.";
    public UserDto? User { get; set; }
}
