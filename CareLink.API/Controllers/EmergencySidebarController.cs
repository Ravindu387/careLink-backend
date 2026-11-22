using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Route("api/emergency-sidebar")]
[Route("api/sidebar")]
public class EmergencySidebarController : ControllerBase
{
    private readonly IEmergencySidebarService _sidebarService;
    private readonly ILogger<EmergencySidebarController> _logger;

    public EmergencySidebarController(
        IEmergencySidebarService sidebarService,
        ILogger<EmergencySidebarController> logger)
    {
        _sidebarService = sidebarService;
        _logger = logger;
    }

    /// <summary>
    /// Gets the consolidated emergency sidebar state (coordinator profile, active crisis response, live badge counts, and system status).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(EmergencySidebarStateDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<EmergencySidebarStateDto>> GetSidebarState(CancellationToken cancellationToken)
    {
        var state = await _sidebarService.GetSidebarStateAsync(cancellationToken);
        return Ok(state);
    }

    /// <summary>
    /// Updates coordinator duty status (e.g., "On Duty", "Incident Command", "On Call", "Standby").
    /// </summary>
    [HttpPatch("duty-status")]
    [ProducesResponseType(typeof(CoordinatorProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CoordinatorProfileDto>> UpdateDutyStatus(
        [FromBody] UpdateDutyStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request?.DutyStatus))
        {
            return BadRequest(new { message = "Duty status is required." });
        }

        var updated = await _sidebarService.UpdateDutyStatusAsync(request.DutyStatus.Trim(), cancellationToken);
        _logger.LogInformation("Coordinator duty status updated to: {DutyStatus}", updated.DutyStatus);
        return Ok(updated);
    }

    /// <summary>
    /// Switches the currently focused active crisis incident for coordination.
    /// </summary>
    [HttpPost("active-crisis/{crisisId}")]
    [ProducesResponseType(typeof(ActiveCrisisSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ActiveCrisisSummaryDto>> SetActiveCrisis(
        string crisisId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(crisisId))
        {
            return BadRequest(new { message = "Crisis ID is required." });
        }

        var summary = await _sidebarService.SetActiveCrisisAsync(crisisId.Trim(), cancellationToken);
        _logger.LogInformation("Active crisis switched to: {CrisisId} - {Name}", summary.Id, summary.Name);
        return Ok(summary);
    }

    /// <summary>
    /// Marks a notification as read and decrements the unread alert count.
    /// </summary>
    [HttpPost("notifications/read")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkNotificationRead(CancellationToken cancellationToken)
    {
        int remaining = await _sidebarService.MarkNotificationReadAsync(cancellationToken);
        return Ok(new { remainingCount = remaining, message = "Notification acknowledged." });
    }

    /// <summary>
    /// Resets the emergency sidebar state and coordinator profile back to default fixtures.
    /// </summary>
    [HttpPost("reset")]
    [ProducesResponseType(typeof(EmergencySidebarStateDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<EmergencySidebarStateDto>> ResetState(CancellationToken cancellationToken)
    {
        var state = await _sidebarService.ResetSidebarStateAsync(cancellationToken);
        _logger.LogInformation("Emergency sidebar state reset to defaults.");
        return Ok(state);
    }
}
