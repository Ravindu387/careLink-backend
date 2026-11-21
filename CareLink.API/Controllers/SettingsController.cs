using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Route("api/settings")]
[Route("api/emergency-settings")]
public class SettingsController : ControllerBase
{
    private readonly ISettingsService _settingsService;
    private readonly ILogger<SettingsController> _logger;

    public SettingsController(
        ISettingsService settingsService,
        ILogger<SettingsController> logger)
    {
        _settingsService = settingsService;
        _logger = logger;
    }

    #region Coordinator Settings Endpoints

    /// <summary>
    /// Retrieves the current Emergency Coordinator settings (profile, operational, notifications, security, and sessions).
    /// </summary>
    [HttpGet]
    [HttpGet("coordinator")]
    [ProducesResponseType(typeof(CoordinatorSettingsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CoordinatorSettingsDto>> GetCoordinatorSettings(CancellationToken cancellationToken)
    {
        var settings = await _settingsService.GetCoordinatorSettingsAsync(cancellationToken);
        return Ok(settings);
    }

    /// <summary>
    /// Updates the Emergency Coordinator profile (full name, email, phone, operational area, role).
    /// </summary>
    [HttpPut("profile")]
    [ProducesResponseType(typeof(CoordinatorProfileSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CoordinatorProfileSettingsDto>> UpdateCoordinatorProfile(
        [FromBody] UpdateCoordinatorProfileSettingsDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _settingsService.UpdateCoordinatorProfileAsync(dto, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Updates the Emergency Coordinator operational preferences (auto refresh, AI recommendations, critical alerts, live map, default priority, refresh interval).
    /// </summary>
    [HttpPut("operational")]
    [ProducesResponseType(typeof(CoordinatorOperationalSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CoordinatorOperationalSettingsDto>> UpdateCoordinatorOperational(
        [FromBody] UpdateCoordinatorOperationalSettingsDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _settingsService.UpdateCoordinatorOperationalAsync(dto, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Updates the Emergency Coordinator notification preferences (categories and delivery channels).
    /// </summary>
    [HttpPut("notifications")]
    [ProducesResponseType(typeof(CoordinatorNotificationSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CoordinatorNotificationSettingsDto>> UpdateCoordinatorNotifications(
        [FromBody] UpdateCoordinatorNotificationSettingsDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _settingsService.UpdateCoordinatorNotificationsAsync(dto, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Changes the Emergency Coordinator account password after verifying the current password.
    /// </summary>
    [HttpPost("security/change-password")]
    [HttpPost("change-password")]
    [ProducesResponseType(typeof(ChangePasswordResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ChangePasswordResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ChangePasswordResponseDto>> ChangeCoordinatorPassword(
        [FromBody] ChangePasswordRequestDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _settingsService.ChangeCoordinatorPasswordAsync(dto, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Retrieves active login sessions for the coordinator.
    /// </summary>
    [HttpGet("sessions")]
    [ProducesResponseType(typeof(IReadOnlyList<UserSessionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UserSessionDto>>> GetCoordinatorSessions(CancellationToken cancellationToken)
    {
        var sessions = await _settingsService.GetCoordinatorSessionsAsync(cancellationToken);
        return Ok(sessions);
    }

    /// <summary>
    /// Revokes an active session by its identifier.
    /// </summary>
    [HttpDelete("sessions/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeSession(string id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest(new { message = "Session ID is required." });
        }

        bool revoked = await _settingsService.RevokeSessionAsync(id.Trim(), cancellationToken);
        if (!revoked)
        {
            return BadRequest(new { message = "Cannot terminate current active session or session not found." });
        }

        return Ok(new { message = "Session revoked successfully.", sessionId = id });
    }

    /// <summary>
    /// Resets all Emergency Coordinator settings back to factory defaults.
    /// </summary>
    [HttpPost("reset")]
    [ProducesResponseType(typeof(CoordinatorSettingsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CoordinatorSettingsDto>> ResetCoordinatorSettings(CancellationToken cancellationToken)
    {
        var result = await _settingsService.ResetCoordinatorSettingsAsync(cancellationToken);
        return Ok(result);
    }

    #endregion

    #region Partner Officer Settings Endpoints

    /// <summary>
    /// Retrieves Partner Officer settings (profile, organisation defaults, data sharing, notifications, and security).
    /// </summary>
    [HttpGet("partner")]
    [ProducesResponseType(typeof(PartnerSettingsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PartnerSettingsDto>> GetPartnerSettings(CancellationToken cancellationToken)
    {
        var settings = await _settingsService.GetPartnerSettingsAsync(cancellationToken);
        return Ok(settings);
    }

    /// <summary>
    /// Updates Partner Officer profile information.
    /// </summary>
    [HttpPut("partner/profile")]
    [ProducesResponseType(typeof(PartnerProfileSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PartnerProfileSettingsDto>> UpdatePartnerProfile(
        [FromBody] UpdatePartnerProfileSettingsDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _settingsService.UpdatePartnerProfileAsync(dto, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Updates Partner Organisation defaults.
    /// </summary>
    [HttpPut("partner/organisation")]
    [ProducesResponseType(typeof(PartnerOrganisationSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PartnerOrganisationSettingsDto>> UpdatePartnerOrganisation(
        [FromBody] UpdatePartnerOrganisationSettingsDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _settingsService.UpdatePartnerOrganisationAsync(dto, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Updates Partner data sharing preferences.
    /// </summary>
    [HttpPut("partner/sharing")]
    [ProducesResponseType(typeof(PartnerSharingSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PartnerSharingSettingsDto>> UpdatePartnerSharing(
        [FromBody] UpdatePartnerSharingSettingsDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _settingsService.UpdatePartnerSharingAsync(dto, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Updates Partner notification preferences.
    /// </summary>
    [HttpPut("partner/notifications")]
    [ProducesResponseType(typeof(PartnerNotificationSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PartnerNotificationSettingsDto>> UpdatePartnerNotifications(
        [FromBody] UpdatePartnerNotificationSettingsDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _settingsService.UpdatePartnerNotificationsAsync(dto, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Updates Partner security preferences.
    /// </summary>
    [HttpPut("partner/security")]
    [ProducesResponseType(typeof(PartnerSecuritySettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PartnerSecuritySettingsDto>> UpdatePartnerSecurity(
        [FromBody] UpdatePartnerSecuritySettingsDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _settingsService.UpdatePartnerSecurityAsync(dto, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Changes the Partner Officer password.
    /// </summary>
    [HttpPost("partner/change-password")]
    [ProducesResponseType(typeof(ChangePasswordResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ChangePasswordResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ChangePasswordResponseDto>> ChangePartnerPassword(
        [FromBody] ChangePasswordRequestDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _settingsService.ChangePartnerPasswordAsync(dto, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Resets all Partner Officer settings back to defaults.
    /// </summary>
    [HttpPost("partner/reset")]
    [ProducesResponseType(typeof(PartnerSettingsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PartnerSettingsDto>> ResetPartnerSettings(CancellationToken cancellationToken)
    {
        var result = await _settingsService.ResetPartnerSettingsAsync(cancellationToken);
        return Ok(result);
    }

    #endregion
}
