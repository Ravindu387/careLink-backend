using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Route("api/emergency-topbar")]
[Route("api/topbar")]
public class EmergencyTopbarController : ControllerBase
{
    private readonly IEmergencyTopbarService _topbarService;
    private readonly ILogger<EmergencyTopbarController> _logger;

    public EmergencyTopbarController(
        IEmergencyTopbarService topbarService,
        ILogger<EmergencyTopbarController> logger)
    {
        _topbarService = topbarService;
        _logger = logger;
    }

    /// <summary>
    /// Gets the consolidated emergency topbar state (operational area, language, unread notifications, helplines, coordinator profile).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(EmergencyTopbarStateDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<EmergencyTopbarStateDto>> GetTopbarState(CancellationToken cancellationToken)
    {
        var state = await _topbarService.GetTopbarStateAsync(cancellationToken);
        return Ok(state);
    }

    /// <summary>
    /// Retrieves the list of notifications, optionally filtering to unread only.
    /// </summary>
    [HttpGet("notifications")]
    [ProducesResponseType(typeof(IReadOnlyList<EmergencyNotificationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<EmergencyNotificationDto>>> GetNotifications(
        [FromQuery] bool unreadOnly = false,
        CancellationToken cancellationToken = default)
    {
        var list = await _topbarService.GetNotificationsAsync(unreadOnly, cancellationToken);
        return Ok(list);
    }

    /// <summary>
    /// Marks a single notification as read.
    /// </summary>
    [HttpPatch("notifications/{id}/read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkNotificationRead(
        string id,
        CancellationToken cancellationToken)
    {
        bool success = await _topbarService.MarkNotificationReadAsync(id, cancellationToken);
        if (!success)
        {
            return NotFound(new { message = $"Notification '{id}' not found." });
        }
        return Ok(new { id, isRead = true, message = "Notification marked as read." });
    }

    /// <summary>
    /// Marks all notifications as read.
    /// </summary>
    [HttpPost("notifications/mark-all-read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAllNotificationsRead(CancellationToken cancellationToken)
    {
        int remaining = await _topbarService.MarkAllNotificationsReadAsync(cancellationToken);
        return Ok(new { remainingCount = remaining, message = "All notifications marked as read." });
    }

    /// <summary>
    /// Dispatches a new notification / alert into the topbar feed.
    /// </summary>
    [HttpPost("notifications")]
    [ProducesResponseType(typeof(EmergencyNotificationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EmergencyNotificationDto>> CreateNotification(
        [FromBody] CreateNotificationRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request?.Title) || string.IsNullOrWhiteSpace(request?.Message))
        {
            return BadRequest(new { message = "Notification title and message are required." });
        }

        var created = await _topbarService.AddNotificationAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetNotifications), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates the active operational area sector.
    /// </summary>
    [HttpPatch("area")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateOperationalArea(
        [FromBody] UpdateAreaRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request?.OperationalArea))
        {
            return BadRequest(new { message = "Operational area cannot be empty." });
        }

        var updated = await _topbarService.UpdateOperationalAreaAsync(request.OperationalArea.Trim(), cancellationToken);
        _logger.LogInformation("Operational area updated to: {Area}", updated);
        return Ok(new { operationalArea = updated });
    }

    /// <summary>
    /// Updates the topbar UI language preference (EN, SI, TA).
    /// </summary>
    [HttpPatch("language")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateLanguage(
        [FromBody] UpdateLanguageRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request?.Language))
        {
            return BadRequest(new { message = "Language code is required." });
        }

        var updated = await _topbarService.UpdateLanguageAsync(request.Language.Trim(), cancellationToken);
        return Ok(new { selectedLanguage = updated });
    }

    /// <summary>
    /// Performs unified global search across incidents, requests, sectors, and coordinator teams.
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IReadOnlyList<GlobalSearchResultDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<GlobalSearchResultDto>>> GlobalSearch(
        [FromQuery] string? q,
        CancellationToken cancellationToken)
    {
        var results = await _topbarService.SearchGlobalAsync(q ?? string.Empty, cancellationToken);
        return Ok(results);
    }

    /// <summary>
    /// Resets the topbar state and notification feed to baseline defaults.
    /// </summary>
    [HttpPost("reset")]
    [ProducesResponseType(typeof(EmergencyTopbarStateDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<EmergencyTopbarStateDto>> ResetState(CancellationToken cancellationToken)
    {
        var state = await _topbarService.ResetTopbarStateAsync(cancellationToken);
        return Ok(state);
    }
}
