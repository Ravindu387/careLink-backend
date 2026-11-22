namespace CareLink.Application.Interfaces;

using CareLink.Core.Entities;

public interface ICitizenProfileRepository
{
    Task<CitizenProfileSettings> GetProfileAsync(string? citizenName = null);
    Task<CitizenProfileSettings> SaveProfileAsync(CitizenProfileSettings profile);
    Task<bool> VerifyPasswordAsync(string citizenName, string currentPassword);
    Task<bool> ChangePasswordAsync(string citizenName, string newPassword);
    Task<CitizenProfileSettings> ResetToDefaultAsync(string? citizenName = null);
}
