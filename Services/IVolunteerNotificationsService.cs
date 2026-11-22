using CareLink.Backend.Models;

namespace CareLink.Backend.Services;

public interface IVolunteerNotificationsService
{
    Task<VolunteerNotificationSummaryDto> GetSummaryAsync();
    Task<PaginatedResult<VolunteerNotification>> GetNotificationsAsync(VolunteerNotificationQueryDto query);
    Task<VolunteerNotification?> GetByIdAsync(int id);
    Task<bool> MarkAsReadAsync(int id);
    Task<int> MarkAllAsReadAsync();
    Task<bool> DeleteNotificationAsync(int id);
    Task<VolunteerNotification> CreateNotificationAsync(CreateVolunteerNotificationRequest request);
    Task<VolunteerNotificationPreferencesDto> GetPreferencesAsync();
    Task<VolunteerNotificationPreferencesDto> UpdatePreferencesAsync(VolunteerNotificationPreferencesDto preferences);
    Task<byte[]> ExportCsvAsync(VolunteerNotificationQueryDto query);
}
