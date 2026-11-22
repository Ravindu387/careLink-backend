using CareLink.Core.Entities;
using CareLink.Core.Models;

namespace CareLink.Core.Interfaces;

public class NotificationFilterParams
{
    public string? Search { get; set; }
    public string? Category { get; set; }
    public bool? UnreadOnly { get; set; }
    public bool? UrgentOnly { get; set; }
    public string? Role { get; set; } // "all", "coordinator", "partner", "ShelterAdmin", "FundingOfficer"
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public interface INotificationRepository
{
    Task<IReadOnlyList<NotificationItem>> GetAllAsync(
        NotificationFilterParams? filter,
        CancellationToken cancellationToken = default);

    Task<int> GetCountAsync(
        NotificationFilterParams? filter = null,
        CancellationToken cancellationToken = default);

    Task<NotificationItem?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default);

    Task<NotificationItem?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<NotificationItem> AddAsync(
        NotificationItem notification,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(
        NotificationItem item,
        CancellationToken cancellationToken = default);

    Task<bool> MarkReadAsync(
        string id,
        CancellationToken cancellationToken = default);

    Task<bool> MarkAsReadAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<int> MarkAllReadAsync(
        string? role = null,
        CancellationToken cancellationToken = default);

    Task<int> MarkAllAsReadAsync(
        string? role = null,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        string id,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<NotificationPreferences> GetPreferencesAsync(
        CancellationToken cancellationToken = default);

    Task<NotificationPreferences> UpdatePreferencesAsync(
        NotificationPreferences preferences,
        CancellationToken cancellationToken = default);

    Task<NotificationMetrics> GetMetricsAsync(
        string? role = null,
        CancellationToken cancellationToken = default);

    Task ResetDefaultDataAsync(
        CancellationToken cancellationToken = default);

    Task<(IEnumerable<NotificationItem> Items, int TotalCount)> GetPagedAsync(
        NotificationQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<NotificationItem>> GetAllAsync(
        NotificationQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<int> GetUnreadCountAsync(
        string? role = null,
        CancellationToken cancellationToken = default);

    Task<Dictionary<string, int>> GetTypeCountsAsync(
        string? role = null,
        CancellationToken cancellationToken = default);

    Task<NotificationPreference?> GetPreferencesAsync(
        string role,
        CancellationToken cancellationToken = default);

    Task<NotificationPreference> SavePreferencesAsync(
        NotificationPreference preference,
        CancellationToken cancellationToken = default);
}
