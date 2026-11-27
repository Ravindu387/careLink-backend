using System.Security.Claims;
using System.Text.Json;
using CareLink.Application.Common;
using CareLink.Application.DTOs;
using CareLink.Application.DTOs.Settings;
using CareLink.Application.Interfaces;
using CareLink.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Authorize]
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

        if (string.Equals(type, "resourcemanager", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(type, "resource-manager", StringComparison.OrdinalIgnoreCase) ||
            User.IsInRole(UserRoles.ResourceManager) ||
            User.IsInRole(UserRoles.LogisticsManager))
        {
            var rmSettings = await _settingsService.GetResourceManagerSettingsAsync();
            return Ok(rmSettings);
        }

        var systemSettings = await _settingsService.GetSettingsAsync();
        return Ok(systemSettings);
    }

    /// <summary>
    /// Update settings (supports both Platform System Settings and Resource Manager settings).
    /// </summary>
    [HttpPut]
    public async Task<IActionResult> UpdateSettings([FromBody] JsonElement raw)
    {
        var jsonText = raw.GetRawText();

        if (raw.TryGetProperty("settings", out _))
        {
            var sysReq = JsonSerializer.Deserialize<UpdateSystemSettingsDto>(jsonText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (sysReq != null)
            {
                var actor = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "System Administrator";
                var result = await _settingsService.UpdateSettingsAsync(sysReq, actor);
                if (!result.Success) return BadRequest(result);
                return Ok(result);
            }
        }

        var rmReq = JsonSerializer.Deserialize<UpdateSettingsRequest>(jsonText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (rmReq != null)
        {
            var result = await _settingsService.UpdateSettingsAsync(rmReq);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        return BadRequest(new { error = "Invalid settings update payload." });
    }

    #endregion

    #region Platform System Settings (Admin / System Admin)

    /// <summary>
    /// Retrieve current platform system configuration, security thresholds, and retention policies.
    /// </summary>
    [HttpGet("system")]
    [ProducesResponseType(typeof(ApiResponse<SystemSettingsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSystemSettings()
    {
        var result = await _settingsService.GetSettingsAsync();
        return Ok(result);
    }

    /// <summary>
    /// Update platform system configuration and policy parameters.
    /// </summary>
    [HttpPut("system")]
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
    [ProducesResponseType(typeof(ApiResponse<SystemSettingsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ResetSystemDefaults()
    {
        var actor = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "System Administrator";
        var result = await _settingsService.ResetDefaultsAsync(actor);
        return Ok(result);
    }

    #endregion

    #region Resource Manager Settings Endpoints (Oshadi)

    [HttpPost("reset")]
    public async Task<IActionResult> ResetSettings()
    {
        var result = await _settingsService.ResetSettingsAsync();
        return Ok(result);
    }

    [HttpPut("inventory")]
    public async Task<IActionResult> UpdateInventory([FromBody] InventoryRulesSettingsDto dto)
    {
        var result = await _settingsService.UpdateInventoryRulesAsync(dto);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPut("dispatch")]
    public async Task<IActionResult> UpdateDispatch([FromBody] DispatchPreferencesSettingsDto dto)
    {
        var result = await _settingsService.UpdateDispatchPreferencesAsync(dto);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    #endregion

    #region Shelter Admin Endpoints (Prabhani)

    // ── Profile ──────────────────────────────────────────
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var result = await _settingsService.GetProfileAsync();
        return Ok(result);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] JsonElement raw)
    {
        var jsonText = raw.GetRawText();

        if (raw.TryGetProperty("warehouse", out _))
        {
            var rmProfile = JsonSerializer.Deserialize<ProfileSettingsDto>(jsonText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (rmProfile != null)
            {
                var rmResult = await _settingsService.UpdateProfileAsync(rmProfile);
                if (!rmResult.Success) return BadRequest(rmResult);
                return Ok(rmResult);
            }
        }

        var dto = JsonSerializer.Deserialize<ProfileDto>(jsonText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (dto != null)
        {
            var result = await _settingsService.UpdateProfileAsync(dto);
            return Ok(result);
        }

        return BadRequest(new { error = "Invalid profile payload." });
    }

    // ── Shelter Settings ─────────────────────────────────
    [HttpGet("shelter")]
    public async Task<IActionResult> GetShelter()
    {
        var result = await _settingsService.GetShelterSettingsAsync();
        return Ok(result);
    }

    [HttpPut("shelter")]
    public async Task<IActionResult> UpdateShelter([FromBody] ShelterSettingsDto dto)
    {
        var result = await _settingsService.UpdateShelterSettingsAsync(dto);
        return Ok(result);
    }

    // ── Security Settings ────────────────────────────────
    [HttpGet("security")]
    public async Task<IActionResult> GetSecurity()
    {
        var result = await _settingsService.GetSecuritySettingsAsync();
        return Ok(result);
    }

    [HttpPut("security")]
    public async Task<IActionResult> UpdateSecurity([FromBody] SecuritySettingsDto dto)
    {
        var rmRes = await _settingsService.UpdateResourceManagerSecuritySettingsAsync(dto);
        await _settingsService.UpdateSecuritySettingsAsync(dto);
        if (rmRes != null && rmRes.Success)
        {
            return Ok(rmRes);
        }
        return Ok(new { message = "Security settings updated." });
    }

    // ── Notifications Settings ───────────────────────────
    [HttpGet("notifications")]
    public async Task<IActionResult> GetNotifications()
    {
        var result = await _settingsService.GetNotificationSettingsAsync();
        return Ok(result);
    }

    [HttpPut("notifications")]
    public async Task<IActionResult> UpdateNotifications([FromBody] JsonElement raw)
    {
        var jsonText = raw.GetRawText();

        if (raw.TryGetProperty("weekly", out _) || raw.TryGetProperty("lowStock", out _))
        {
            var rmNotif = JsonSerializer.Deserialize<NotificationPreferencesSettingsDto>(jsonText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (rmNotif != null)
            {
                var rmResult = await _settingsService.UpdateNotificationPreferencesAsync(rmNotif);
                if (!rmResult.Success) return BadRequest(rmResult);
                return Ok(rmResult);
            }
        }

        var dto = JsonSerializer.Deserialize<NotificationSettingsDto>(jsonText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (dto != null)
        {
            var result = await _settingsService.UpdateNotificationSettingsAsync(dto);
            return Ok(result);
        }

        return BadRequest(new { error = "Invalid notification settings payload." });
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
    [HttpPut("coordinator/profile")]
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
    [HttpPut("coordinator/operational")]
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
    [HttpPut("coordinator/notifications")]
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
    [HttpPost("coordinator/change-password")]
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
        return Ok(result);
    }

    /// <summary>
    /// Retrieves active login sessions for the Emergency Coordinator.
    /// </summary>
    [HttpGet("sessions")]
    [HttpGet("coordinator/sessions")]
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
    [HttpDelete("coordinator/sessions/{sessionId}")]
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
