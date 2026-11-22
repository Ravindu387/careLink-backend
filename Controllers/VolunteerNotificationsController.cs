using CareLink.Backend.Models;
using CareLink.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.Backend.Controllers;

[ApiController]
[Route("api/volunteer/notifications")]
[Route("api/volunteer-notifications")]
public class VolunteerNotificationsController : ControllerBase
{
    private readonly IVolunteerNotificationsService _service;
    private readonly ILogger<VolunteerNotificationsController> _logger;

    public VolunteerNotificationsController(IVolunteerNotificationsService service, ILogger<VolunteerNotificationsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<ApiResponse<VolunteerNotificationSummaryDto>>> GetSummary()
    {
        try
        {
            var summary = await _service.GetSummaryAsync();
            return Ok(ApiResponse<VolunteerNotificationSummaryDto>.Ok(summary, "Volunteer notifications summary retrieved successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get volunteer notifications summary");
            return StatusCode(500, ApiResponse<VolunteerNotificationSummaryDto>.Fail("Error retrieving summary."));
        }
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResult<VolunteerNotification>>>> GetNotifications([FromQuery] VolunteerNotificationQueryDto query)
    {
        try
        {
            var result = await _service.GetNotificationsAsync(query);
            return Ok(ApiResponse<PaginatedResult<VolunteerNotification>>.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get volunteer notifications");
            return StatusCode(500, ApiResponse<PaginatedResult<VolunteerNotification>>.Fail("Error retrieving notifications."));
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<VolunteerNotification>>> GetById(int id)
    {
        try
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null)
            {
                return NotFound(ApiResponse<VolunteerNotification>.Fail($"Notification {id} not found."));
            }
            return Ok(ApiResponse<VolunteerNotification>.Ok(item));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get notification {Id}", id);
            return StatusCode(500, ApiResponse<VolunteerNotification>.Fail("Error retrieving notification."));
        }
    }

    [HttpPatch("{id:int}/read")]
    public async Task<ActionResult<ApiResponse<bool>>> MarkAsRead(int id)
    {
        try
        {
            var success = await _service.MarkAsReadAsync(id);
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
            var count = await _service.MarkAllAsReadAsync();
            return Ok(ApiResponse<int>.Ok(count, $"{count} notifications marked as read."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark all volunteer notifications as read");
            return StatusCode(500, ApiResponse<int>.Fail("Error marking all notifications as read."));
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        try
        {
            var deleted = await _service.DeleteNotificationAsync(id);
            if (!deleted)
            {
                return NotFound(ApiResponse<bool>.Fail($"Notification {id} not found."));
            }
            return Ok(ApiResponse<bool>.Ok(true, $"Notification {id} deleted successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete notification {Id}", id);
            return StatusCode(500, ApiResponse<bool>.Fail("Error deleting notification."));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<VolunteerNotification>>> Create([FromBody] CreateVolunteerNotificationRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return BadRequest(ApiResponse<VolunteerNotification>.Fail("Title is required."));
            }

            var created = await _service.CreateNotificationAsync(request);
            return Ok(ApiResponse<VolunteerNotification>.Ok(created, "Notification created successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create notification");
            return StatusCode(500, ApiResponse<VolunteerNotification>.Fail("Error creating notification."));
        }
    }

    [HttpGet("preferences")]
    public async Task<ActionResult<ApiResponse<VolunteerNotificationPreferencesDto>>> GetPreferences()
    {
        try
        {
            var prefs = await _service.GetPreferencesAsync();
            return Ok(ApiResponse<VolunteerNotificationPreferencesDto>.Ok(prefs));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get volunteer notification preferences");
            return StatusCode(500, ApiResponse<VolunteerNotificationPreferencesDto>.Fail("Error retrieving preferences."));
        }
    }

    [HttpPut("preferences")]
    public async Task<ActionResult<ApiResponse<VolunteerNotificationPreferencesDto>>> UpdatePreferences([FromBody] VolunteerNotificationPreferencesDto preferences)
    {
        try
        {
            var updated = await _service.UpdatePreferencesAsync(preferences);
            return Ok(ApiResponse<VolunteerNotificationPreferencesDto>.Ok(updated, "Preferences updated successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update preferences");
            return StatusCode(500, ApiResponse<VolunteerNotificationPreferencesDto>.Fail("Error updating preferences."));
        }
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] VolunteerNotificationQueryDto query)
    {
        try
        {
            var bytes = await _service.ExportCsvAsync(query);
            var fileName = $"carelink-volunteer-notifications-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
            return File(bytes, "text/csv", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export notifications CSV");
            return StatusCode(500, ApiResponse<string>.Fail("Error exporting CSV."));
        }
    }
}
