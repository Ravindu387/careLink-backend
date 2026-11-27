using CareLink.Backend.Models;
using CareLink.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.Backend.Controllers;

[ApiController]
[Authorize]
[Route("api/case-manager/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationsService _notificationsService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(INotificationsService notificationsService, ILogger<NotificationsController> logger)
    {
        _notificationsService = notificationsService;
        _logger = logger;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<ApiResponse<NotificationSummaryDto>>> GetSummary()
    {
        try
        {
            var summary = await _notificationsService.GetSummaryAsync();
            return Ok(ApiResponse<NotificationSummaryDto>.Ok(summary, "Notifications summary retrieved successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get notifications summary");
            return StatusCode(500, ApiResponse<NotificationSummaryDto>.Fail("Error retrieving notifications summary."));
        }
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResult<NotificationItem>>>> GetNotifications([FromQuery] NotificationQueryDto query)
    {
        try
        {
            var paged = await _notificationsService.GetNotificationsAsync(query);
            return Ok(ApiResponse<PaginatedResult<NotificationItem>>.Ok(paged));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get notifications");
            return StatusCode(500, ApiResponse<PaginatedResult<NotificationItem>>.Fail("Error retrieving notifications."));
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<NotificationItem>>> GetById(int id)
    {
        try
        {
            var item = await _notificationsService.GetByIdAsync(id);
            if (item == null)
            {
                return NotFound(ApiResponse<NotificationItem>.Fail($"Notification {id} not found."));
            }
            return Ok(ApiResponse<NotificationItem>.Ok(item));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get notification {Id}", id);
            return StatusCode(500, ApiResponse<NotificationItem>.Fail("Error retrieving notification."));
        }
    }

    [HttpPatch("{id:int}/read")]
    public async Task<ActionResult<ApiResponse<bool>>> MarkAsRead(int id)
    {
        try
        {
            var success = await _notificationsService.MarkAsReadAsync(id);
            if (!success)
            {
                return NotFound(ApiResponse<bool>.Fail($"Notification {id} not found."));
            }
            return Ok(ApiResponse<bool>.Ok(true, $"Notification {id} marked as read."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark notification {Id} as read", id);
            return StatusCode(500, ApiResponse<bool>.Fail("Error marking notification as read."));
        }
    }

    [HttpPost("mark-all-read")]
    public async Task<ActionResult<ApiResponse<int>>> MarkAllAsRead()
    {
        try
        {
            var count = await _notificationsService.MarkAllAsReadAsync();
            return Ok(ApiResponse<int>.Ok(count, $"{count} notification(s) marked as read."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark all notifications as read");
            return StatusCode(500, ApiResponse<int>.Fail("Error marking all notifications as read."));
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        try
        {
            var deleted = await _notificationsService.DeleteNotificationAsync(id);
            if (!deleted)
            {
                return NotFound(ApiResponse<bool>.Fail($"Notification {id} not found."));
            }
            return Ok(ApiResponse<bool>.Ok(true, $"Notification {id} deleted."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete notification {Id}", id);
            return StatusCode(500, ApiResponse<bool>.Fail("Error deleting notification."));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<NotificationItem>>> Create([FromBody] CreateNotificationRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return BadRequest(ApiResponse<NotificationItem>.Fail("Title is required."));
            }

            var created = await _notificationsService.CreateNotificationAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<NotificationItem>.Ok(created, "Notification created successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create notification");
            return StatusCode(500, ApiResponse<NotificationItem>.Fail("Error creating notification."));
        }
    }

    [HttpGet("preferences")]
    public async Task<ActionResult<ApiResponse<NotificationPreferencesDto>>> GetPreferences()
    {
        try
        {
            var prefs = await _notificationsService.GetPreferencesAsync();
            return Ok(ApiResponse<NotificationPreferencesDto>.Ok(prefs));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get preferences");
            return StatusCode(500, ApiResponse<NotificationPreferencesDto>.Fail("Error retrieving preferences."));
        }
    }

    [HttpPut("preferences")]
    public async Task<ActionResult<ApiResponse<NotificationPreferencesDto>>> UpdatePreferences([FromBody] NotificationPreferencesDto preferences)
    {
        try
        {
            var updated = await _notificationsService.UpdatePreferencesAsync(preferences);
            return Ok(ApiResponse<NotificationPreferencesDto>.Ok(updated, "Notification preferences updated successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update preferences");
            return StatusCode(500, ApiResponse<NotificationPreferencesDto>.Fail("Error updating preferences."));
        }
    }
}
