using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Route("api/partner/notifications")]
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
    /// Retrieves paginated notifications with optional filtering by search text, category, urgent, unread status, or role.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedNotificationsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedNotificationsDto>> GetNotifications(
        [FromQuery] string? search,
        [FromQuery] string? category,
        [FromQuery] bool? unreadOnly,
        [FromQuery] bool? urgentOnly,
        [FromQuery] string? role,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var effectiveRole = DetermineRole(role);
        var query = new NotificationQueryParameters
        {
            Search = search,
            Category = category,
            UnreadOnly = unreadOnly,
            UrgentOnly = urgentOnly,
            Role = effectiveRole,
            Page = page,
            PageSize = pageSize
        };

        var result = await _service.GetPagedAsync(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves all notifications without pagination (optionally filtered).
    /// </summary>
    [HttpGet("all")]
    [ProducesResponseType(typeof(IReadOnlyList<NotificationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<NotificationDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? category,
        [FromQuery] bool? unreadOnly,
        [FromQuery] bool? urgentOnly,
        [FromQuery] string? role,
        CancellationToken cancellationToken = default)
    {
        var effectiveRole = DetermineRole(role);
        var query = new NotificationQueryParameters
        {
            Search = search,
            Category = category,
            UnreadOnly = unreadOnly,
            UrgentOnly = urgentOnly,
            Role = effectiveRole
        };

        var list = await _service.GetAllAsync(query, cancellationToken);
        return Ok(list);
    }

    /// <summary>
    /// Retrieves notification counts and category metrics.
    /// </summary>
    [HttpGet("metrics")]
    [HttpGet("counts")]
    [ProducesResponseType(typeof(NotificationMetricsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<NotificationMetricsDto>> GetMetrics(
        [FromQuery] string? role,
        CancellationToken cancellationToken)
    {
        var effectiveRole = DetermineRole(role);
        var metrics = await _service.GetMetricsAsync(effectiveRole, cancellationToken);
        return Ok(metrics);
    }

    /// <summary>
    /// Retrieves user notification preferences.
    /// </summary>
    [HttpGet("preferences")]
    [ProducesResponseType(typeof(NotificationPreferencesDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<NotificationPreferencesDto>> GetPreferences(CancellationToken cancellationToken)
    {
        var prefs = await _service.GetPreferencesAsync(cancellationToken);
        return Ok(prefs);
    }

    /// <summary>
    /// Updates user notification preferences.
    /// </summary>
    [HttpPut("preferences")]
    [ProducesResponseType(typeof(NotificationPreferencesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<NotificationPreferencesDto>> UpdatePreferences(
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
    /// Retrieves a single notification by unique id or reference.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(NotificationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotificationDto>> GetById(
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
    /// Marks all notifications as read (optionally filtered by role).
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
    public async Task<ActionResult<NotificationDto>> Create(
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
    /// Deletes a notification.
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
    /// Resets all notifications and preferences back to the default initial dataset.
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
