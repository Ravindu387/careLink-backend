using System.Security.Claims;
using CareLink.Application.Common;
using CareLink.Application.DTOs;
using CareLink.Application.DTOs.Settings;
using CareLink.Application.Interfaces;
using CareLink.Core.Constants;
using Microsoft.AspNetCore.Authorization;
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

    #region Root Settings Resolver

    /// <summary>
    /// Retrieve settings appropriate to the requester's role or the requested setting domain.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetSettings([FromQuery] string? type, CancellationToken cancellationToken)
    {
        var isEmergencyPath = Request.Path.Value?.Contains("emergency-settings", StringComparison.OrdinalIgnoreCase) == true;

        if (string.Equals(type, "coordinator", StringComparison.OrdinalIgnoreCase) ||
            isEmergencyPath ||
            User.IsInRole(UserRoles.EmergencyCoordinator))
        {
            var coordinatorSettings = await _settingsService.GetCoordinatorSettingsAsync(cancellationToken);
            return Ok(coordinatorSettings);
        }

        if (string.Equals(type, "partner", StringComparison.OrdinalIgnoreCase) ||
            User.IsInRole(UserRoles.PartnerOfficer))
        {
            var partnerSettings = await _settingsService.GetPartnerSettingsAsync(cancellationToken);
            return Ok(partnerSettings);
        }

        var systemSettings = await _settingsService.GetSettingsAsync();
        return Ok(systemSettings);
    }

    #endregion

    #region Platform System Settings (Admin / System Admin)

    /// <summary>
    /// Retrieve current platform system configuration, security thresholds, and retention policies.
    /// </summary>
    [HttpGet("system")]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.SystemAdmin}")]
    [ProducesResponseType(typeof(ApiResponse<SystemSettingsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSystemSettings()
    {
        var result = await _settingsService.GetSettingsAsync();
        return Ok(result);
    }

    /// <summary>
    /// Update platform system configuration and policy parameters.
    /// </summary>
    [HttpPut]
    [HttpPut("system")]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.SystemAdmin}")]
    [ProducesResponseType(typeof(ApiResponse<SystemSettingsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SystemSettingsDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateSystemSettings([FromBody] UpdateSystemSettingsDto request)
    {
        var actor = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "System Administrator";
        var result = await _settingsService.UpdateSettingsAsync(request, actor);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Reset platform system settings to platform defaults.
    /// </summary>
    [HttpPost("system/reset")]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.SystemAdmin}")]
    [ProducesResponseType(typeof(ApiResponse<SystemSettingsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ResetSystemDefaults()
    {
        var actor = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "System Administrator";
        var result = await _settingsService.ResetDefaultsAsync(actor);
        return Ok(result);
    }

    #endregion

    #region Coordinator Settings Endpoints

    /// <summary>
    /// Retrieves the current Emergency Coordinator settings.
    /// </summary>
    [HttpGet("coordinator")]
    [ProducesResponseType(typeof(CoordinatorSettingsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CoordinatorSettingsDto>> GetCoordinatorSettings(CancellationToken cancellationToken)
    {
        var settings = await _settingsService.GetCoordinatorSettingsAsync(cancellationToken);
        return Ok(settings);
    }

    /// <summary>
    /// Updates the Emergency Coordinator profile.
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
    /// Updates the Emergency Coordinator operational preferences.
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
    /// Updates Emergency Coordinator notification preferences.
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
    /// Changes the Emergency Coordinator password.
    /// </summary>
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
    /// Retrieves active login sessions for the Emergency Coordinator.
    /// </summary>
    [HttpGet("sessions")]
    [ProducesResponseType(typeof(IReadOnlyList<UserSessionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UserSessionDto>>> GetCoordinatorSessions(CancellationToken cancellationToken)
    {
        var sessions = await _settingsService.GetCoordinatorSessionsAsync(cancellationToken);
        return Ok(sessions);
    }

    /// <summary>
    /// Revokes an active session.
    /// </summary>
    [HttpDelete("sessions/{sessionId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeSession(string sessionId, CancellationToken cancellationToken)
    {
        var success = await _settingsService.RevokeSessionAsync(sessionId, cancellationToken);
        if (!success)
        {
            return NotFound(new { error = $"Session '{sessionId}' not found or could not be revoked." });
        }

        return Ok(new { sessionId, message = "Session revoked successfully." });
    }

    /// <summary>
    /// Resets Coordinator settings to default baseline.
    /// </summary>
    [HttpPost("reset")]
    [HttpPost("coordinator/reset")]
    [ProducesResponseType(typeof(CoordinatorSettingsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CoordinatorSettingsDto>> ResetCoordinatorSettings(CancellationToken cancellationToken)
    {
        var result = await _settingsService.ResetCoordinatorSettingsAsync(cancellationToken);
        return Ok(result);
    }

    #endregion

    #region Partner Officer Settings Endpoints

    /// <summary>
    /// Retrieves Partner Officer settings.
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
