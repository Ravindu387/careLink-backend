using CareLink.Backend.Models;
using CareLink.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.Backend.Controllers;

[ApiController]
[Route("api/volunteer/settings")]
[Route("api/volunteer-settings")]
public class VolunteerSettingsController : ControllerBase
{
    private readonly IVolunteerSettingsService _service;
    private readonly ILogger<VolunteerSettingsController> _logger;

    public VolunteerSettingsController(
        IVolunteerSettingsService service,
        ILogger<VolunteerSettingsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<VolunteerSettingsDto>>> GetSettings()
    {
        try
        {
            var settings = await _service.GetSettingsAsync();
            return Ok(ApiResponse<VolunteerSettingsDto>.Ok(settings, "Volunteer coordinator settings retrieved successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get volunteer settings");
            return StatusCode(500, ApiResponse<VolunteerSettingsDto>.Fail("Error retrieving settings."));
        }
    }

    [HttpPut]
    public async Task<ActionResult<ApiResponse<VolunteerSettingsDto>>> UpdateSettings(
        [FromBody] VolunteerSettingsDto settings)
    {
        try
        {
            var updated = await _service.UpdateSettingsAsync(settings);
            return Ok(ApiResponse<VolunteerSettingsDto>.Ok(updated, "Volunteer coordinator settings saved successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update volunteer settings");
            return StatusCode(500, ApiResponse<VolunteerSettingsDto>.Fail("Error saving settings."));
        }
    }

    [HttpPost("change-password")]
    public async Task<ActionResult<ApiResponse<bool>>> ChangePassword(
        [FromBody] VolunteerChangePasswordRequest request)
    {
        try
        {
            var success = await _service.ChangePasswordAsync(request);
            if (!success)
            {
                return BadRequest(ApiResponse<bool>.Fail("Invalid password change request. Password must contain at least 8 characters."));
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
            var success = await _service.RevokeSessionAsync(id);
            if (!success)
            {
                return NotFound(ApiResponse<bool>.Fail($"Session {id} cannot be revoked or is the current active session."));
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
    public async Task<ActionResult<ApiResponse<VolunteerSettingsDto>>> ResetDefaults()
    {
        try
        {
            var defaults = await _service.ResetDefaultsAsync();
            return Ok(ApiResponse<VolunteerSettingsDto>.Ok(defaults, "Settings reset to default values."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reset volunteer settings");
            return StatusCode(500, ApiResponse<VolunteerSettingsDto>.Fail("Error resetting settings."));
        }
    }
}
