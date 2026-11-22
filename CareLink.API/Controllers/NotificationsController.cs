using System.Security.Claims;
using CareLink.Application.Common;
using CareLink.Application.DTOs;
using CareLink.Application.DTOs.Users;
using CareLink.Application.Interfaces;
using CareLink.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AdminDtos = CareLink.Application.DTOs.Notifications;

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
    /// Retrieves notifications. Supports paged coordinator queries and administrative queries.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedNotificationsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedListDto<AdminDtos.NotificationDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] NotificationQueryParameters query,
        [FromQuery] string? type,
        [FromQuery] int? pageNumber,
        [FromQuery] int? pageSize,
        [FromQuery] bool? paged,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(type) || pageNumber.HasValue || pageSize.HasValue)
        {
            var adminQuery = new AdminDtos.NotificationQueryDto
            {
                Search = query.Search,
                Type = type,
                UnreadOnly = query.UnreadOnly ?? false,
                PageNumber = pageNumber ?? query.Page,
                PageSize = pageSize ?? query.PageSize
            };

            var adminResult = await _service.GetNotificationsAsync(adminQuery);
            return Ok(adminResult);
        }

        var effectiveRole = DetermineRole(query.Role);
        query.Role = effectiveRole;

        if (paged == false)
        {
            var list = await _service.GetAllAsync(query, cancellationToken);
            return Ok(list);
        }

        var result = await _service.GetPagedAsync(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Aggregates and returns real-time notification operational metrics.
    /// </summary>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(NotificationMetricsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMetrics(
        [FromQuery] string? role,
        CancellationToken cancellationToken)
    {
        var effectiveRole = DetermineRole(role);
        var metrics = await _service.GetMetricsAsync(effectiveRole, cancellationToken);
        return Ok(metrics);
    }

    /// <summary>
    /// Retrieve notification count statistics across unread, critical, high-priority, and domain categories (Admin).
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(ApiResponse<AdminDtos.NotificationStatsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats()
    {
        var result = await _service.GetStatsAsync();
        return Ok(result);
    }

    /// <summary>
    /// Retrieves all unread notifications.
    /// </summary>
    [HttpGet("unread")]
    [ProducesResponseType(typeof(IReadOnlyList<NotificationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnread(
        [FromQuery] string? role,
        CancellationToken cancellationToken)
    {
        var effectiveRole = DetermineRole(role);
        var query = new NotificationQueryParameters { UnreadOnly = true, Role = effectiveRole };
        var unread = await _service.GetAllAsync(query, cancellationToken);
        return Ok(unread);
    }

    /// <summary>
    /// Retrieves user notification preferences.
    /// </summary>
    [HttpGet("preferences")]
    [ProducesResponseType(typeof(CareLink.Application.DTOs.NotificationPreferencesDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPreferences(CancellationToken cancellationToken)
    {
        var prefs = await _service.GetPreferencesAsync(cancellationToken);
        return Ok(prefs);
    }

    /// <summary>
    /// Updates user notification preferences.
    /// </summary>
    [HttpPut("preferences")]
    [ProducesResponseType(typeof(CareLink.Application.DTOs.NotificationPreferencesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdatePreferences(
        [FromBody] UpdateNotificationPreferencesDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updated = await _service.UpdatePreferencesAsync(dto, cancellationToken);
        _logger.LogInformation("Notification preferences updated.");
        return Ok(updated);
    }

    /// <summary>
    /// Retrieves a single notification by unique id.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(NotificationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        string id,
        CancellationToken cancellationToken)
    {
        var item = await _service.GetByIdAsync(id, cancellationToken);
        if (item == null)
        {
            return NotFound(new { message = $"Notification '{id}' not found." });
        }

        return Ok(item);
    }

    /// <summary>
    /// Marks a specific notification as read.
    /// </summary>
    [HttpPatch("{id}/read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkRead(
        string id,
        CancellationToken cancellationToken)
    {
        var success = await _service.MarkReadAsync(id, cancellationToken);
        if (!success)
        {
            return NotFound(new { message = $"Notification '{id}' not found." });
        }

        return Ok(new { id, unread = false, message = "Notification marked as read." });
    }

    /// <summary>
    /// Marks all notifications as read.
    /// </summary>
    [HttpPost("mark-all-read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAllRead(
        [FromQuery] string? role,
        CancellationToken cancellationToken)
    {
        var effectiveRole = DetermineRole(role);
        var updatedCount = await _service.MarkAllReadAsync(effectiveRole, cancellationToken);
        _logger.LogInformation("Marked all notifications as read ({Count} updated) for role {Role}.", updatedCount, effectiveRole ?? "all");
        return Ok(new { readCount = updatedCount, message = "All notifications marked as read." });
    }

    /// <summary>
    /// Creates and dispatches a new operational notification.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(NotificationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateNotificationDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var created = await _service.CreateAsync(dto, cancellationToken);
        _logger.LogInformation("Notification {Id} created for {Category}: {Title}",
            created.Id, created.Category, created.Title);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Deletes or archives a notification.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        string id,
        CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound(new { message = $"Notification '{id}' not found." });
        }

        return Ok(new { id, message = $"Notification '{id}' deleted successfully." });
    }

    /// <summary>
    /// Execute batch operation across multiple notifications (Admin).
    /// </summary>
    [HttpPost("batch")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BatchAction([FromBody] AdminDtos.BatchNotificationActionDto request)
    {
        var actor = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "System Administrator";
        var result = await _service.BatchActionAsync(request, actor);
        return Ok(result);
    }

    /// <summary>
    /// Resets all notifications and preferences back to default seed data.
    /// </summary>
    [HttpPost("reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ResetDefaultData(CancellationToken cancellationToken)
    {
        await _service.ResetDefaultDataAsync(cancellationToken);
        _logger.LogInformation("Notifications reset to default seed scenario.");
        return Ok(new { message = "Notifications reset to default seed scenario." });
    }

    private string? DetermineRole(string? explicitRole)
    {
        if (!string.IsNullOrWhiteSpace(explicitRole))
        {
            return explicitRole.Trim().ToLowerInvariant();
        }

        var path = Request.Path.Value ?? string.Empty;
        if (path.Contains("/partner/", StringComparison.OrdinalIgnoreCase))
        {
            return "partner";
        }

        return null;
    }
}
