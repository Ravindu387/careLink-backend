using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _service;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        INotificationService service,
        ILogger<NotificationsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Gets notifications with filtering by type, unread, role, search term, and pagination.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResultDto<NotificationItemDto>>> GetPaged(
        [FromQuery] NotificationQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetPagedNotificationsAsync(parameters, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets aggregated summary counts for notifications.
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult<NotificationSummaryDto>> GetSummary(
        [FromQuery] string? role = "ShelterAdmin",
        CancellationToken cancellationToken = default)
    {
        var summary = await _service.GetSummaryAsync(role, cancellationToken);
        return Ok(summary);
    }

    /// <summary>
    /// Gets quick unread count for badge indicators.
    /// </summary>
    [HttpGet("unread-count")]
    public async Task<ActionResult<object>> GetUnreadCount(
        [FromQuery] string? role = "ShelterAdmin",
        CancellationToken cancellationToken = default)
    {
        var count = await _service.GetUnreadCountAsync(role, cancellationToken);
        return Ok(new { count });
    }

    /// <summary>
    /// Gets notification preferences for a role.
    /// </summary>
    [HttpGet("preferences")]
    public async Task<ActionResult<NotificationPreferenceDto>> GetPreferences(
        [FromQuery] string role = "ShelterAdmin",
        CancellationToken cancellationToken = default)
    {
        var preferences = await _service.GetPreferencesAsync(role, cancellationToken);
        return Ok(preferences);
    }

    /// <summary>
    /// Saves or updates notification preferences for a role.
    /// </summary>
    [HttpPut("preferences")]
    public async Task<ActionResult<NotificationPreferenceDto>> SavePreferences(
        [FromBody] UpdateNotificationPreferenceDto dto,
        [FromQuery] string role = "ShelterAdmin",
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var saved = await _service.SavePreferencesAsync(role, dto, cancellationToken);
        return Ok(saved);
    }

    /// <summary>
    /// Gets a single notification by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<NotificationItemDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var item = await _service.GetNotificationByIdAsync(id, cancellationToken);
        if (item == null)
        {
            return NotFound(new { message = $"Notification #{id} was not found." });
        }
        return Ok(item);
    }

    /// <summary>
    /// Creates a new notification.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<NotificationItemDto>> Create(
        [FromBody] CreateNotificationDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var created = await _service.CreateNotificationAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating notification");
            return StatusCode(500, new { message = "An error occurred while creating the notification." });
        }
    }

    /// <summary>
    /// Marks a notification as read.
    /// </summary>
    [HttpPatch("{id:int}/read")]
    [HttpPost("{id:int}/read")]
    public async Task<IActionResult> MarkAsRead(
        int id,
        CancellationToken cancellationToken)
    {
        var success = await _service.MarkAsReadAsync(id, cancellationToken);
        if (!success)
        {
            return NotFound(new { message = $"Notification #{id} was not found." });
        }

        return Ok(new { success = true, message = $"Notification #{id} marked as read." });
    }

    /// <summary>
    /// Marks all unread notifications as read.
    /// </summary>
    [HttpPost("mark-all-read")]
    public async Task<IActionResult> MarkAllAsRead(
        [FromQuery] string? role = "ShelterAdmin",
        CancellationToken cancellationToken = default)
    {
        var count = await _service.MarkAllAsReadAsync(role, cancellationToken);
        return Ok(new { count, message = $"{count} notifications marked as read." });
    }

    /// <summary>
    /// Deletes a notification by ID.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var success = await _service.DeleteNotificationAsync(id, cancellationToken);
        if (!success)
        {
            return NotFound(new { message = $"Notification #{id} was not found." });
        }

        return NoContent();
    }
}
