using CareLink.Application.DTOs;
using CareLink.Core.Models;

namespace CareLink.Application.Interfaces;

public interface INotificationService
{
    Task<PagedResultDto<NotificationItemDto>> GetPagedNotificationsAsync(
        NotificationQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<NotificationItemDto>> GetAllNotificationsAsync(
        NotificationQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<NotificationItemDto?> GetNotificationByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<NotificationSummaryDto> GetSummaryAsync(string? role = "ShelterAdmin", CancellationToken cancellationToken = default);

    Task<int> GetUnreadCountAsync(string? role = "ShelterAdmin", CancellationToken cancellationToken = default);

    Task<NotificationItemDto> CreateNotificationAsync(CreateNotificationDto dto, CancellationToken cancellationToken = default);

    Task<bool> MarkAsReadAsync(int id, CancellationToken cancellationToken = default);

    Task<int> MarkAllAsReadAsync(string? role = "ShelterAdmin", CancellationToken cancellationToken = default);

    Task<bool> DeleteNotificationAsync(int id, CancellationToken cancellationToken = default);

    Task<NotificationPreferenceDto> GetPreferencesAsync(string role = "ShelterAdmin", CancellationToken cancellationToken = default);

    Task<NotificationPreferenceDto> SavePreferencesAsync(string role, UpdateNotificationPreferenceDto dto, CancellationToken cancellationToken = default);
}
