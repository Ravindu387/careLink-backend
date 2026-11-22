using CareLink.Backend.Models;

namespace CareLink.Backend.Services;

public interface INotificationsService
{
    Task<NotificationSummaryDto> GetSummaryAsync();
    Task<PaginatedResult<NotificationItem>> GetNotificationsAsync(NotificationQueryDto query);
    Task<NotificationItem?> GetByIdAsync(int id);
    Task<bool> MarkAsReadAsync(int id);
    Task<int> MarkAllAsReadAsync();
    Task<bool> DeleteNotificationAsync(int id);
    Task<NotificationItem> CreateNotificationAsync(CreateNotificationRequest request);
    Task<NotificationPreferencesDto> GetPreferencesAsync();
    Task<NotificationPreferencesDto> UpdatePreferencesAsync(NotificationPreferencesDto preferences);
}
