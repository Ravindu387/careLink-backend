using Microsoft.AspNetCore.Authorization;
using CareLink.Backend.Models;
using CareLink.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.Backend.Controllers;

[ApiController]
[Authorize]
[Route("api/case-manager-settings")]
public class CaseManagerSettingsController : ControllerBase
{
    private readonly ICaseManagerSettingsService _settingsService;
    private readonly ILogger<CaseManagerSettingsController> _logger;

    public CaseManagerSettingsController(ICaseManagerSettingsService settingsService, ILogger<CaseManagerSettingsController> logger)
    {
        _settingsService = settingsService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<CaseManagerSettingsDto>>> GetSettings()
    {
        try
        {
            var settings = await _settingsService.GetSettingsAsync();
            return Ok(ApiResponse<CaseManagerSettingsDto>.Ok(settings, "Settings retrieved successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get settings");
            return StatusCode(500, ApiResponse<CaseManagerSettingsDto>.Fail("Error retrieving settings."));
        }
    }

    [HttpPut]
    public async Task<ActionResult<ApiResponse<CaseManagerSettingsDto>>> UpdateSettings([FromBody] CaseManagerSettingsDto settings)
    {
        try
        {
            var updated = await _settingsService.UpdateSettingsAsync(settings);
            return Ok(ApiResponse<CaseManagerSettingsDto>.Ok(updated, "Settings saved successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update settings");
            return StatusCode(500, ApiResponse<CaseManagerSettingsDto>.Fail("Error updating settings."));
        }
    }

    [HttpPost("change-password")]
    public async Task<ActionResult<ApiResponse<bool>>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            var success = await _settingsService.ChangePasswordAsync(request);
            if (!success)
            {
                return BadRequest(ApiResponse<bool>.Fail("Invalid password change request. Ensure new password has at least 8 characters."));
            }
            return Ok(ApiResponse<bool>.Ok(true, "Password changed successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to change password");
            return StatusCode(500, ApiResponse<bool>.Fail("Error changing password."));
        }
    }

    [HttpDelete("sessions/{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> RevokeSession(string id)
    {
        try
        {
            var revoked = await _settingsService.RevokeSessionAsync(id);
            if (!revoked)
            {
                return NotFound(ApiResponse<bool>.Fail($"Session {id} cannot be revoked."));
            }
            return Ok(ApiResponse<bool>.Ok(true, $"Session {id} revoked successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to revoke session {Id}", id);
            return StatusCode(500, ApiResponse<bool>.Fail("Error revoking session."));
        }
    }

    [HttpPost("reset")]
    public async Task<ActionResult<ApiResponse<CaseManagerSettingsDto>>> ResetDefaults()
    {
        try
        {
            var reset = await _settingsService.ResetDefaultsAsync();
            return Ok(ApiResponse<CaseManagerSettingsDto>.Ok(reset, "Settings reset to defaults successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reset settings");
            return StatusCode(500, ApiResponse<CaseManagerSettingsDto>.Fail("Error resetting settings."));
        }
    }
}
