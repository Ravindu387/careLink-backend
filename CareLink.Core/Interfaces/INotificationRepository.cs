using CareLink.Core.Entities;
using CareLink.Core.Models;

namespace CareLink.Core.Interfaces;

public interface INotificationRepository
{
    Task<(IEnumerable<NotificationItem> Items, int TotalCount)> GetPagedAsync(
        NotificationQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<NotificationItem>> GetAllAsync(
        NotificationQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<NotificationItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<NotificationItem> AddAsync(NotificationItem item, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(NotificationItem item, CancellationToken cancellationToken = default);

    Task<bool> MarkAsReadAsync(int id, CancellationToken cancellationToken = default);

    Task<int> MarkAllAsReadAsync(string? role = null, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<int> GetUnreadCountAsync(string? role = null, CancellationToken cancellationToken = default);

    Task<Dictionary<string, int>> GetTypeCountsAsync(string? role = null, CancellationToken cancellationToken = default);

    Task<NotificationPreference?> GetPreferencesAsync(string role, CancellationToken cancellationToken = default);

    Task<NotificationPreference> SavePreferencesAsync(NotificationPreference preference, CancellationToken cancellationToken = default);
}
