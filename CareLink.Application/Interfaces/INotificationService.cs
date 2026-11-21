using CareLink.Application.Common;
using CareLink.Application.DTOs.Notifications;
using CareLink.Application.DTOs.Users;

namespace CareLink.Application.Interfaces;

public interface INotificationService
{
    Task<ApiResponse<PaginatedListDto<NotificationDto>>> GetNotificationsAsync(NotificationQueryDto query);
    Task<ApiResponse<NotificationStatsDto>> GetStatsAsync();
    Task<ApiResponse<NotificationDto>> GetNotificationByIdAsync(int id);
    Task<ApiResponse<NotificationDto>> CreateNotificationAsync(CreateNotificationRequestDto request, string actor);
    Task<ApiResponse<bool>> MarkAsReadAsync(int id);
    Task<ApiResponse<bool>> MarkAllAsReadAsync();
    Task<ApiResponse<bool>> ArchiveNotificationAsync(int id);
    Task<ApiResponse<int>> BatchActionAsync(BatchNotificationActionDto request, string actor);
    Task<ApiResponse<NotificationPreferencesDto>> GetPreferencesAsync(string userId);
    Task<ApiResponse<NotificationPreferencesDto>> UpdatePreferencesAsync(string userId, NotificationPreferencesDto preferences);
}
