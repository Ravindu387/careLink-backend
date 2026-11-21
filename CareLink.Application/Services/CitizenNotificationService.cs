namespace CareLink.Application.Services;

using CareLink.Application.Common;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;

public class CitizenNotificationService : ICitizenNotificationService
{
    private readonly ICitizenNotificationRepository _repository;

    public CitizenNotificationService(ICitizenNotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<IEnumerable<CitizenNotificationDto>>> GetNotificationsAsync(
        string? citizenName = null,
        string? category = null,
        bool? unreadOnly = null,
        string? search = null)
    {
        var notifications = await _repository.GetAllAsync(citizenName, category, unreadOnly, search);
        var dtos = notifications.Select(MapToDto).ToList();
        return ApiResponse<IEnumerable<CitizenNotificationDto>>.SuccessResponse(dtos, "Notifications retrieved successfully");
    }

    public async Task<ApiResponse<CitizenNotificationDto>> GetNotificationByIdAsync(string id)
    {
        var notification = await _repository.GetByIdAsync(id);
        if (notification == null)
        {
            return ApiResponse<CitizenNotificationDto>.ErrorResponse($"Notification with ID '{id}' was not found.");
        }

        return ApiResponse<CitizenNotificationDto>.SuccessResponse(MapToDto(notification), "Notification retrieved successfully");
    }

    public async Task<ApiResponse<CitizenNotificationDto>> CreateNotificationAsync(CreateNotificationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return ApiResponse<CitizenNotificationDto>.ErrorResponse("Title is required.");
        }

        var notification = new CitizenNotification
        {
            Type = string.IsNullOrWhiteSpace(request.Type) ? "alert" : request.Type.Trim().ToLowerInvariant(),
            Title = request.Title.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            Category = string.IsNullOrWhiteSpace(request.Category) ? "Safety alerts" : request.Category.Trim(),
            ActionUrl = request.ActionUrl?.Trim(),
            CitizenName = string.IsNullOrWhiteSpace(request.CitizenName) ? "Samira Khan" : request.CitizenName.Trim(),
            CitizenId = "CIT-001",
            Unread = true,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(notification);
        return ApiResponse<CitizenNotificationDto>.SuccessResponse(MapToDto(created), "Notification created successfully");
    }

    public async Task<ApiResponse<bool>> MarkAsReadAsync(string id)
    {
        var success = await _repository.MarkAsReadAsync(id);
        if (!success)
        {
            return ApiResponse<bool>.ErrorResponse($"Notification with ID '{id}' was not found.");
        }

        return ApiResponse<bool>.SuccessResponse(true, "Notification marked as read");
    }

    public async Task<ApiResponse<int>> MarkAllAsReadAsync(string? citizenName = null)
    {
        var count = await _repository.MarkAllAsReadAsync(citizenName);
        return ApiResponse<int>.SuccessResponse(count, $"{count} notifications marked as read");
    }

    public async Task<ApiResponse<bool>> DeleteNotificationAsync(string id)
    {
        var deleted = await _repository.DeleteAsync(id);
        if (!deleted)
        {
            return ApiResponse<bool>.ErrorResponse($"Notification with ID '{id}' was not found.");
        }

        return ApiResponse<bool>.SuccessResponse(true, "Notification deleted successfully");
    }

    public async Task<ApiResponse<NotificationSummaryDto>> GetSummaryAsync(string? citizenName = null)
    {
        var all = (await _repository.GetAllAsync(citizenName)).ToList();
        var summary = new NotificationSummaryDto
        {
            TotalCount = all.Count,
            UnreadCount = all.Count(n => n.Unread),
            SafetyAlertsCount = all.Count(n => n.Category.Equals("Safety alerts", StringComparison.OrdinalIgnoreCase)),
            MyRequestsCount = all.Count(n => n.Category.Equals("My requests", StringComparison.OrdinalIgnoreCase))
        };

        return ApiResponse<NotificationSummaryDto>.SuccessResponse(summary, "Notification summary retrieved successfully");
    }

    private static CitizenNotificationDto MapToDto(CitizenNotification entity)
    {
        return new CitizenNotificationDto
        {
            Id = entity.Id,
            Type = entity.Type,
            Title = entity.Title,
            Description = entity.Description,
            Time = FormatTimeAgo(entity.CreatedAt),
            Unread = entity.Unread,
            Category = entity.Category,
            ActionUrl = entity.ActionUrl,
            CitizenName = entity.CitizenName,
            CreatedAt = entity.CreatedAt,
            ReadAt = entity.ReadAt
        };
    }

    private static string FormatTimeAgo(DateTime dateTime)
    {
        var diff = DateTime.UtcNow - dateTime;
        if (diff.TotalMinutes < 1) return "Just now";
        if (diff.TotalMinutes < 60) return $"{Math.Max(1, (int)diff.TotalMinutes)} min ago";
        if (diff.TotalHours < 2) return "1 hour ago";
        if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} hours ago";
        if (diff.TotalDays < 2) return "1 day ago";
        return $"{dateTime:dd MMM}";
    }
}
