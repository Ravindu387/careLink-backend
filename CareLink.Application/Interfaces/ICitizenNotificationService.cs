namespace CareLink.Application.Interfaces;

using CareLink.Application.Common;
using CareLink.Application.DTOs;

public interface ICitizenNotificationService
{
    Task<ApiResponse<IEnumerable<CitizenNotificationDto>>> GetNotificationsAsync(
        string? citizenName = null,
        string? category = null,
        bool? unreadOnly = null,
        string? search = null);

    Task<ApiResponse<CitizenNotificationDto>> GetNotificationByIdAsync(string id);
    Task<ApiResponse<CitizenNotificationDto>> CreateNotificationAsync(CreateNotificationRequest request);
    Task<ApiResponse<bool>> MarkAsReadAsync(string id);
    Task<ApiResponse<int>> MarkAllAsReadAsync(string? citizenName = null);
    Task<ApiResponse<bool>> DeleteNotificationAsync(string id);
    Task<ApiResponse<NotificationSummaryDto>> GetSummaryAsync(string? citizenName = null);
}
