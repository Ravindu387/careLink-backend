using System.Security.Claims;
using CareLink.Application.Common;
using CareLink.Application.DTOs.Notifications;
using CareLink.Application.DTOs.Users;
using CareLink.Application.Interfaces;
using CareLink.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize(Roles = $"{UserRoles.Admin},{UserRoles.SystemAdmin},{UserRoles.EmergencyCoordinator},{UserRoles.IncidentManager},{UserRoles.VolunteerCoordinator},{UserRoles.ShelterAdmin}")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <summary>
    /// Retrieve paginated notifications with optional type, priority, and text search filters.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedListDto<NotificationDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotifications([FromQuery] NotificationQueryDto query)
    {
        var result = await _notificationService.GetNotificationsAsync(query);
        return Ok(result);
    }

    /// <summary>
    /// Retrieve notification count statistics across unread, critical, high-priority, and domain categories.
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(ApiResponse<NotificationStatsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats()
    {
        var result = await _notificationService.GetStatsAsync();
        return Ok(result);
    }

    /// <summary>
    /// Retrieve a single notification by its identifier.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<NotificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<NotificationDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _notificationService.GetNotificationByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Broadcast or dispatch a new operational alert or emergency notification.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<NotificationDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<NotificationDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<NotificationDto>.FailureResult("Validation failed.", errors));
        }

        var actor = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "System Administrator";
        var result = await _notificationService.CreateNotificationAsync(request, actor);

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Mark a specific notification as read.
    /// </summary>
    [HttpPatch("{id:int}/read")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var result = await _notificationService.MarkAsReadAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Mark all active notifications as read.
    /// </summary>
    [HttpPost("mark-all-read")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var result = await _notificationService.MarkAllAsReadAsync();
        return Ok(result);
    }

    /// <summary>
    /// Archive a notification from active view.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ArchiveNotification(int id)
    {
        var result = await _notificationService.ArchiveNotificationAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Execute batch operation (mark read, unread, archive) across multiple notifications.
    /// </summary>
    [HttpPost("batch")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BatchAction([FromBody] BatchNotificationActionDto request)
    {
        var actor = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "System Administrator";
        var result = await _notificationService.BatchActionAsync(request, actor);
        return Ok(result);
    }

    /// <summary>
    /// Retrieve operational notification preferences for the current operator.
    /// </summary>
    [HttpGet("preferences")]
    [ProducesResponseType(typeof(ApiResponse<NotificationPreferencesDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPreferences()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "default";
        var result = await _notificationService.GetPreferencesAsync(userId);
        return Ok(result);
    }

    /// <summary>
    /// Update notification alert delivery preferences.
    /// </summary>
    [HttpPut("preferences")]
    [ProducesResponseType(typeof(ApiResponse<NotificationPreferencesDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdatePreferences([FromBody] NotificationPreferencesDto preferences)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "default";
        var result = await _notificationService.UpdatePreferencesAsync(userId, preferences);
        return Ok(result);
    }
}
