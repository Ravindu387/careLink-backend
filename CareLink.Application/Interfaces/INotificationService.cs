using CareLink.Application.Common;
using CareLink.Application.DTOs;
using CareLink.Application.DTOs.Users;
using AdminDtos = CareLink.Application.DTOs.Notifications;

namespace CareLink.Application.Interfaces;

public interface INotificationService
{
    // Operational & Coordinator Methods (Safna)
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

    Task<CareLink.Application.DTOs.NotificationPreferencesDto> GetPreferencesAsync(
        CancellationToken cancellationToken = default);

    Task<CareLink.Application.DTOs.NotificationPreferencesDto> UpdatePreferencesAsync(
        UpdateNotificationPreferencesDto dto,
        CancellationToken cancellationToken = default);

    Task<NotificationMetricsDto> GetMetricsAsync(
        string? role = null,
        CancellationToken cancellationToken = default);

    Task ResetDefaultDataAsync(
        CancellationToken cancellationToken = default);

    // Administrative Methods (dev)
    Task<ApiResponse<PaginatedListDto<AdminDtos.NotificationDto>>> GetNotificationsAsync(
        AdminDtos.NotificationQueryDto query);

    Task<ApiResponse<AdminDtos.NotificationStatsDto>> GetStatsAsync();

    Task<ApiResponse<AdminDtos.NotificationDto>> GetNotificationByIdAsync(int id);

    Task<ApiResponse<AdminDtos.NotificationDto>> CreateNotificationAsync(
        AdminDtos.CreateNotificationRequestDto request,
        string actor);

    Task<ApiResponse<bool>> MarkAsReadAsync(int id);

    Task<ApiResponse<bool>> MarkAllAsReadAsync();

    Task<ApiResponse<bool>> ArchiveNotificationAsync(int id);

    Task<ApiResponse<int>> BatchActionAsync(
        AdminDtos.BatchNotificationActionDto request,
        string actor);

    Task<ApiResponse<AdminDtos.NotificationPreferencesDto>> GetAdminPreferencesAsync(string userId);

    Task<ApiResponse<AdminDtos.NotificationPreferencesDto>> UpdateAdminPreferencesAsync(
        string userId,
        AdminDtos.NotificationPreferencesDto preferences);
}
