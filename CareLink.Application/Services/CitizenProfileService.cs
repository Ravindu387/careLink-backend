namespace CareLink.Application.Services;

using System.Text.RegularExpressions;
using CareLink.Application.Common;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;

public class CitizenProfileService : ICitizenProfileService
{
    private readonly ICitizenProfileRepository _repository;

    public CitizenProfileService(ICitizenProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<CitizenProfileSettingsDto>> GetProfileAsync(string? citizenName = null)
    {
        var profile = await _repository.GetProfileAsync(citizenName);
        return ApiResponse<CitizenProfileSettingsDto>.SuccessResponse(MapToDto(profile), "Profile settings retrieved successfully.");
    }

    public async Task<ApiResponse<CitizenProfileSettingsDto>> UpdateFullProfileAsync(UpdateFullProfileRequest request, string? citizenName = null)
    {
        var profile = await _repository.GetProfileAsync(citizenName);

        if (!string.IsNullOrWhiteSpace(request.FullName))
        {
            profile.CitizenName = request.FullName.Trim();
        }

        if (request.PreferredName != null)
        {
            profile.PreferredName = request.PreferredName.Trim();
        }

        if (request.DateOfBirth != null)
        {
            profile.DateOfBirth = request.DateOfBirth.Trim();
        }

        if (request.NationalId != null)
        {
            profile.NationalId = request.NationalId.Trim();
        }

        if (request.Gender != null)
        {
            profile.Gender = request.Gender.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            profile.Email = request.Email.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.Phone))
        {
            profile.Phone = request.Phone.Trim();
        }

        if (request.Address != null)
        {
            profile.Address = request.Address.Trim();
        }

        if (request.District != null)
        {
            profile.District = request.District.Trim();
        }

        if (request.City != null)
        {
            profile.City = request.City.Trim();
        }

        if (request.PostalCode != null)
        {
            profile.PostalCode = request.PostalCode.Trim();
        }

        if (request.EmergencyContactName != null)
        {
            profile.EmergencyContactName = request.EmergencyContactName.Trim();
        }

        if (request.EmergencyContactPhone != null)
        {
            profile.EmergencyContactPhone = request.EmergencyContactPhone.Trim();
        }

        if (request.EmergencyContactRelation != null)
        {
            profile.EmergencyContactRelation = request.EmergencyContactRelation.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.Language))
        {
            profile.Language = request.Language.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.Accessibility))
        {
            profile.Accessibility = request.Accessibility.Trim();
        }

        if (request.HighContrast.HasValue) profile.HighContrast = request.HighContrast.Value;
        if (request.LargeText.HasValue) profile.LargeText = request.LargeText.Value;
        if (request.ScreenReaderSupport.HasValue) profile.ScreenReaderSupport = request.ScreenReaderSupport.Value;
        if (request.AudioAssistance.HasValue) profile.AudioAssistance = request.AudioAssistance.Value;

        if (request.EmergencyAlerts.HasValue) profile.EmergencyAlerts = request.EmergencyAlerts.Value;
        if (request.RequestStatusUpdates.HasValue) profile.RequestStatusUpdates = request.RequestStatusUpdates.Value;
        if (request.SafetyNotifications.HasValue) profile.SafetyNotifications = request.SafetyNotifications.Value;
        if (request.MessagesFromSupportTeams.HasValue) profile.MessagesFromSupportTeams = request.MessagesFromSupportTeams.Value;
        if (request.ShelterAvailabilityUpdates.HasValue) profile.ShelterAvailabilityUpdates = request.ShelterAvailabilityUpdates.Value;
        if (request.SmsAlerts.HasValue) profile.SmsAlerts = request.SmsAlerts.Value;
        if (request.EmailAlerts.HasValue) profile.EmailAlerts = request.EmailAlerts.Value;

        if (request.ShareLocationDuringDisaster.HasValue) profile.ShareLocationDuringDisaster = request.ShareLocationDuringDisaster.Value;
        if (request.ShareMedicalWithRescuers.HasValue) profile.ShareMedicalWithRescuers = request.ShareMedicalWithRescuers.Value;
        if (request.AllowVolunteersContact.HasValue) profile.AllowVolunteersContact = request.AllowVolunteersContact.Value;
        if (request.DataRetentionConsent.HasValue) profile.DataRetentionConsent = request.DataRetentionConsent.Value;

        var saved = await _repository.SaveProfileAsync(profile);
        return ApiResponse<CitizenProfileSettingsDto>.SuccessResponse(MapToDto(saved), "Profile settings updated successfully.");
    }

    public async Task<ApiResponse<CitizenProfileSettingsDto>> UpdatePersonalDetailsAsync(UpdatePersonalDetailsRequest request, string? citizenName = null)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            return ApiResponse<CitizenProfileSettingsDto>.ErrorResponse("Full name cannot be empty.");
        }

        var profile = await _repository.GetProfileAsync(citizenName);
        profile.CitizenName = request.FullName.Trim();
        if (request.PreferredName != null) profile.PreferredName = request.PreferredName.Trim();
        if (request.DateOfBirth != null) profile.DateOfBirth = request.DateOfBirth.Trim();
        if (request.NationalId != null) profile.NationalId = request.NationalId.Trim();
        if (request.Gender != null) profile.Gender = request.Gender.Trim();

        var saved = await _repository.SaveProfileAsync(profile);
        return ApiResponse<CitizenProfileSettingsDto>.SuccessResponse(MapToDto(saved), "Personal details updated successfully.");
    }

    public async Task<ApiResponse<CitizenProfileSettingsDto>> UpdateContactLocationAsync(UpdateContactLocationRequest request, string? citizenName = null)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return ApiResponse<CitizenProfileSettingsDto>.ErrorResponse("Email address cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(request.Phone))
        {
            return ApiResponse<CitizenProfileSettingsDto>.ErrorResponse("Contact phone number cannot be empty.");
        }

        var profile = await _repository.GetProfileAsync(citizenName);
        profile.Email = request.Email.Trim();
        profile.Phone = request.Phone.Trim();
        profile.Address = request.Address.Trim();
        profile.District = request.District.Trim();
        if (request.City != null) profile.City = request.City.Trim();
        if (request.PostalCode != null) profile.PostalCode = request.PostalCode.Trim();
        if (request.EmergencyContactName != null) profile.EmergencyContactName = request.EmergencyContactName.Trim();
        if (request.EmergencyContactPhone != null) profile.EmergencyContactPhone = request.EmergencyContactPhone.Trim();
        if (request.EmergencyContactRelation != null) profile.EmergencyContactRelation = request.EmergencyContactRelation.Trim();

        var saved = await _repository.SaveProfileAsync(profile);
        return ApiResponse<CitizenProfileSettingsDto>.SuccessResponse(MapToDto(saved), "Contact and location details updated successfully.");
    }

    public async Task<ApiResponse<CitizenProfileSettingsDto>> UpdateLanguageAccessibilityAsync(UpdateLanguageAccessibilityRequest request, string? citizenName = null)
    {
        var profile = await _repository.GetProfileAsync(citizenName);
        if (!string.IsNullOrWhiteSpace(request.Language)) profile.Language = request.Language.Trim();
        if (!string.IsNullOrWhiteSpace(request.Accessibility)) profile.Accessibility = request.Accessibility.Trim();
        profile.HighContrast = request.HighContrast;
        profile.LargeText = request.LargeText;
        profile.ScreenReaderSupport = request.ScreenReaderSupport;
        profile.AudioAssistance = request.AudioAssistance;

        var saved = await _repository.SaveProfileAsync(profile);
        return ApiResponse<CitizenProfileSettingsDto>.SuccessResponse(MapToDto(saved), "Language & accessibility preferences updated successfully.");
    }

    public async Task<ApiResponse<CitizenProfileSettingsDto>> UpdateNotificationPreferencesAsync(UpdateNotificationPreferencesRequest request, string? citizenName = null)
    {
        var profile = await _repository.GetProfileAsync(citizenName);
        profile.EmergencyAlerts = request.EmergencyAlerts;
        profile.RequestStatusUpdates = request.RequestStatusUpdates;
        profile.SafetyNotifications = request.SafetyNotifications;
        profile.MessagesFromSupportTeams = request.MessagesFromSupportTeams;
        profile.ShelterAvailabilityUpdates = request.ShelterAvailabilityUpdates;
        profile.SmsAlerts = request.SmsAlerts;
        profile.EmailAlerts = request.EmailAlerts;

        var saved = await _repository.SaveProfileAsync(profile);
        return ApiResponse<CitizenProfileSettingsDto>.SuccessResponse(MapToDto(saved), "Notification preferences updated successfully.");
    }

    public async Task<ApiResponse<CitizenProfileSettingsDto>> UpdatePrivacyDataAsync(UpdatePrivacyDataRequest request, string? citizenName = null)
    {
        var profile = await _repository.GetProfileAsync(citizenName);
        profile.ShareLocationDuringDisaster = request.ShareLocationDuringDisaster;
        profile.ShareMedicalWithRescuers = request.ShareMedicalWithRescuers;
        profile.AllowVolunteersContact = request.AllowVolunteersContact;
        profile.DataRetentionConsent = request.DataRetentionConsent;

        var saved = await _repository.SaveProfileAsync(profile);
        return ApiResponse<CitizenProfileSettingsDto>.SuccessResponse(MapToDto(saved), "Privacy preferences updated successfully.");
    }

    public async Task<ApiResponse<bool>> ChangePasswordAsync(ChangePasswordRequest request, string? citizenName = null)
    {
        var targetCitizen = string.IsNullOrWhiteSpace(citizenName) ? "Samira Khan" : citizenName.Trim();

        if (string.IsNullOrWhiteSpace(request.CurrentPassword))
        {
            return ApiResponse<bool>.ErrorResponse("Current password is required.");
        }

        if (string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return ApiResponse<bool>.ErrorResponse("New password cannot be empty.");
        }

        if (request.NewPassword.Length < 6)
        {
            return ApiResponse<bool>.ErrorResponse("New password must be at least 6 characters long.");
        }

        if (request.NewPassword != request.ConfirmPassword)
        {
            return ApiResponse<bool>.ErrorResponse("New password and confirm password do not match.");
        }

        var isCorrect = await _repository.VerifyPasswordAsync(targetCitizen, request.CurrentPassword);
        if (!isCorrect)
        {
            return ApiResponse<bool>.ErrorResponse("Current password is incorrect.");
        }

        var changed = await _repository.ChangePasswordAsync(targetCitizen, request.NewPassword);
        if (!changed)
        {
            return ApiResponse<bool>.ErrorResponse("Failed to update password. Please try again.");
        }

        return ApiResponse<bool>.SuccessResponse(true, "Password changed successfully.");
    }

    public async Task<ApiResponse<CitizenProfileSettingsDto>> ToggleTwoFactorAsync(bool enabled, string? citizenName = null)
    {
        var profile = await _repository.GetProfileAsync(citizenName);
        profile.TwoFactorEnabled = enabled;

        var saved = await _repository.SaveProfileAsync(profile);
        var msg = enabled ? "Two-factor authentication has been enabled." : "Two-factor authentication has been disabled.";
        return ApiResponse<CitizenProfileSettingsDto>.SuccessResponse(MapToDto(saved), msg);
    }

    public async Task<ApiResponse<CitizenProfileSettingsDto>> ResetProfileAsync(string? citizenName = null)
    {
        var profile = await _repository.ResetToDefaultAsync(citizenName);
        return ApiResponse<CitizenProfileSettingsDto>.SuccessResponse(MapToDto(profile), "Profile reset to default demo values.");
    }

    private static CitizenProfileSettingsDto MapToDto(CitizenProfileSettings entity)
    {
        return new CitizenProfileSettingsDto
        {
            Id = entity.Id,
            CitizenReference = entity.CitizenReference,
            FullName = entity.CitizenName,
            PreferredName = entity.PreferredName,
            DateOfBirth = entity.DateOfBirth,
            NationalId = entity.NationalId,
            Gender = entity.Gender,
            Email = entity.Email,
            Phone = entity.Phone,
            Address = entity.Address,
            District = entity.District,
            City = entity.City,
            PostalCode = entity.PostalCode,
            EmergencyContactName = entity.EmergencyContactName,
            EmergencyContactPhone = entity.EmergencyContactPhone,
            EmergencyContactRelation = entity.EmergencyContactRelation,
            Language = entity.Language,
            Accessibility = entity.Accessibility,
            HighContrast = entity.HighContrast,
            LargeText = entity.LargeText,
            ScreenReaderSupport = entity.ScreenReaderSupport,
            AudioAssistance = entity.AudioAssistance,
            EmergencyAlerts = entity.EmergencyAlerts,
            RequestStatusUpdates = entity.RequestStatusUpdates,
            SafetyNotifications = entity.SafetyNotifications,
            MessagesFromSupportTeams = entity.MessagesFromSupportTeams,
            ShelterAvailabilityUpdates = entity.ShelterAvailabilityUpdates,
            SmsAlerts = entity.SmsAlerts,
            EmailAlerts = entity.EmailAlerts,
            ShareLocationDuringDisaster = entity.ShareLocationDuringDisaster,
            ShareMedicalWithRescuers = entity.ShareMedicalWithRescuers,
            AllowVolunteersContact = entity.AllowVolunteersContact,
            DataRetentionConsent = entity.DataRetentionConsent,
            PersonalInformationPolicy = entity.PersonalInformationPolicy,
            DataSharingPolicy = entity.DataSharingPolicy,
            TwoFactorEnabled = entity.TwoFactorEnabled,
            LastPasswordChange = entity.LastPasswordChange,
            LastLoginAt = entity.LastLoginAt,
            ActiveSessions = entity.ActiveSessions.Select(s => new SecuritySessionDto
            {
                Device = s.Device,
                Location = s.Location,
                LastActive = s.LastActive,
                IsCurrent = s.IsCurrent
            }).ToList(),
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
