using CareLink.Application.DTOs;

namespace CareLink.Application.Interfaces;

public interface IEmergencyTopbarService
{
    Task<EmergencyTopbarStateDto> GetTopbarStateAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EmergencyNotificationDto>> GetNotificationsAsync(
        bool unreadOnly = false,
        CancellationToken cancellationToken = default);

    Task<bool> MarkNotificationReadAsync(
        string notificationId,
        CancellationToken cancellationToken = default);

    Task<int> MarkAllNotificationsReadAsync(CancellationToken cancellationToken = default);

    Task<EmergencyNotificationDto> AddNotificationAsync(
        CreateNotificationRequest request,
        CancellationToken cancellationToken = default);

    Task<string> UpdateOperationalAreaAsync(
        string area,
        CancellationToken cancellationToken = default);

    Task<string> UpdateLanguageAsync(
        string language,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GlobalSearchResultDto>> SearchGlobalAsync(
        string query,
        CancellationToken cancellationToken = default);

    Task<EmergencyTopbarStateDto> ResetTopbarStateAsync(CancellationToken cancellationToken = default);
}
