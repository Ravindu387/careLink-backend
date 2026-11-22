using CareLink.Core.Entities;

namespace CareLink.Core.Interfaces;

public interface IEmergencyTopbarRepository
{
    Task<EmergencyTopbarState> GetStateAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EmergencyNotification>> GetNotificationsAsync(
        bool unreadOnly = false,
        CancellationToken cancellationToken = default);

    Task<bool> MarkNotificationReadAsync(
        string notificationId,
        CancellationToken cancellationToken = default);

    Task<int> MarkAllNotificationsReadAsync(CancellationToken cancellationToken = default);

    Task<EmergencyNotification> AddNotificationAsync(
        EmergencyNotification notification,
        CancellationToken cancellationToken = default);

    Task<string> UpdateOperationalAreaAsync(
        string area,
        CancellationToken cancellationToken = default);

    Task<string> UpdateLanguageAsync(
        string language,
        CancellationToken cancellationToken = default);

    Task ResetStateAsync(CancellationToken cancellationToken = default);
}
