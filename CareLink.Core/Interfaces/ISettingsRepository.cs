using CareLink.Core.Entities;

namespace CareLink.Core.Interfaces;

public interface ISettingsRepository
{
    // Emergency Coordinator Settings
    Task<CoordinatorSettings> GetCoordinatorSettingsAsync(CancellationToken cancellationToken = default);
    Task<CoordinatorProfileSettings> UpdateCoordinatorProfileAsync(CoordinatorProfileSettings profile, CancellationToken cancellationToken = default);
    Task<CoordinatorOperationalSettings> UpdateCoordinatorOperationalAsync(CoordinatorOperationalSettings operational, CancellationToken cancellationToken = default);
    Task<CoordinatorNotificationSettings> UpdateCoordinatorNotificationsAsync(CoordinatorNotificationSettings notifications, CancellationToken cancellationToken = default);
    Task<bool> VerifyCoordinatorPasswordAsync(string currentPassword, CancellationToken cancellationToken = default);
    Task<bool> ChangeCoordinatorPasswordAsync(string newPassword, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserSession>> GetCoordinatorSessionsAsync(CancellationToken cancellationToken = default);
    Task<bool> RevokeSessionAsync(string sessionId, CancellationToken cancellationToken = default);
    Task<CoordinatorSettings> ResetCoordinatorSettingsAsync(CancellationToken cancellationToken = default);

    // Partner Officer Settings
    Task<PartnerSettings> GetPartnerSettingsAsync(CancellationToken cancellationToken = default);
    Task<PartnerProfileSettings> UpdatePartnerProfileAsync(PartnerProfileSettings profile, CancellationToken cancellationToken = default);
    Task<PartnerOrganisationSettings> UpdatePartnerOrganisationAsync(PartnerOrganisationSettings organisation, CancellationToken cancellationToken = default);
    Task<PartnerSharingSettings> UpdatePartnerSharingAsync(PartnerSharingSettings sharing, CancellationToken cancellationToken = default);
    Task<PartnerNotificationSettings> UpdatePartnerNotificationsAsync(PartnerNotificationSettings notifications, CancellationToken cancellationToken = default);
    Task<PartnerSecuritySettings> UpdatePartnerSecurityAsync(PartnerSecuritySettings security, CancellationToken cancellationToken = default);
    Task<bool> VerifyPartnerPasswordAsync(string currentPassword, CancellationToken cancellationToken = default);
    Task<bool> ChangePartnerPasswordAsync(string newPassword, CancellationToken cancellationToken = default);
    Task<PartnerSettings> ResetPartnerSettingsAsync(CancellationToken cancellationToken = default);

    // Shelter Admin User Settings
    Task<UserSettings?> GetAsync();
    Task<UserSettings> UpsertAsync(UserSettings settings);
}
