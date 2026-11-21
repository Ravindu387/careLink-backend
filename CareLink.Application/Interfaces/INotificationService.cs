using CareLink.Application.DTOs;

namespace CareLink.Application.Interfaces;

public interface INotificationService
{
    Task<PagedNotificationsDto> GetPagedAsync(
        NotificationQueryParameters query,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NotificationDto>> GetAllAsync(
        NotificationQueryParameters query,
        CancellationToken cancellationToken = default);

    Task<NotificationDto?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default);

    Task<NotificationDto> CreateAsync(
        CreateNotificationDto dto,
        CancellationToken cancellationToken = default);

    Task<bool> MarkReadAsync(
        string id,
        CancellationToken cancellationToken = default);

    Task<int> MarkAllReadAsync(
        string? role = null,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        string id,
        CancellationToken cancellationToken = default);

    Task<NotificationPreferencesDto> GetPreferencesAsync(
        CancellationToken cancellationToken = default);

    Task<NotificationPreferencesDto> UpdatePreferencesAsync(
        UpdateNotificationPreferencesDto dto,
        CancellationToken cancellationToken = default);

    Task<NotificationMetricsDto> GetMetricsAsync(
        string? role = null,
        CancellationToken cancellationToken = default);

    Task ResetDefaultDataAsync(
        CancellationToken cancellationToken = default);
}
