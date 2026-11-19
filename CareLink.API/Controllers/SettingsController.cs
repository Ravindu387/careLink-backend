using System.Security.Claims;
using CareLink.Application.Common;
using CareLink.Application.DTOs.Settings;
using CareLink.Application.Interfaces;
using CareLink.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Route("api/settings")]
[Authorize(Roles = $"{UserRoles.Admin},{UserRoles.SystemAdmin}")]
public class SettingsController : ControllerBase
{
    private readonly ISettingsService _settingsService;

    public SettingsController(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    /// <summary>
    /// Retrieve current platform system configuration, security thresholds, and retention policies.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<SystemSettingsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSettings()
    {
        var result = await _settingsService.GetSettingsAsync();
        return Ok(result);
    }

    /// <summary>
    /// Update system configuration and policy parameters.
    /// </summary>
    [HttpPut]
    [ProducesResponseType(typeof(ApiResponse<SystemSettingsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SystemSettingsDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateSystemSettingsDto request)
    {
        var actor = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "System Administrator";
        var result = await _settingsService.UpdateSettingsAsync(request, actor);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Reset all system settings to platform defaults.
    /// </summary>
    [HttpPost("reset")]
    [ProducesResponseType(typeof(ApiResponse<SystemSettingsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ResetDefaults()
    {
        var actor = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "System Administrator";
        var result = await _settingsService.ResetDefaultsAsync(actor);
        return Ok(result);
    }
}
