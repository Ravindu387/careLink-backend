namespace CareLink.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;

[ApiController]
[Route("api/citizen/profile")]
[Route("api/citizen/settings")]
[Route("api/profile/settings")]
public class CitizenProfileController : ControllerBase
{
    private readonly ICitizenProfileService _profileService;

    public CitizenProfileController(ICitizenProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProfile([FromQuery] string? citizenName)
    {
        var result = await _profileService.GetProfileAsync(citizenName);
        return Ok(result);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateFullProfile([FromBody] UpdateFullProfileRequest request, [FromQuery] string? citizenName)
    {
        var result = await _profileService.UpdateFullProfileAsync(request, citizenName);
        return Ok(result);
    }

    [HttpPut("personal")]
    public async Task<IActionResult> UpdatePersonalDetails([FromBody] UpdatePersonalDetailsRequest request, [FromQuery] string? citizenName)
    {
        var result = await _profileService.UpdatePersonalDetailsAsync(request, citizenName);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPut("contact")]
    public async Task<IActionResult> UpdateContactLocation([FromBody] UpdateContactLocationRequest request, [FromQuery] string? citizenName)
    {
        var result = await _profileService.UpdateContactLocationAsync(request, citizenName);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPut("preferences")]
    public async Task<IActionResult> UpdateLanguageAccessibility([FromBody] UpdateLanguageAccessibilityRequest request, [FromQuery] string? citizenName)
    {
        var result = await _profileService.UpdateLanguageAccessibilityAsync(request, citizenName);
        return Ok(result);
    }

    [HttpPut("notifications")]
    public async Task<IActionResult> UpdateNotificationPreferences([FromBody] UpdateNotificationPreferencesRequest request, [FromQuery] string? citizenName)
    {
        var result = await _profileService.UpdateNotificationPreferencesAsync(request, citizenName);
        return Ok(result);
    }

    [HttpPut("privacy")]
    public async Task<IActionResult> UpdatePrivacyData([FromBody] UpdatePrivacyDataRequest request, [FromQuery] string? citizenName)
    {
        var result = await _profileService.UpdatePrivacyDataAsync(request, citizenName);
        return Ok(result);
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, [FromQuery] string? citizenName)
    {
        var result = await _profileService.ChangePasswordAsync(request, citizenName);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPost("two-factor")]
    public async Task<IActionResult> ToggleTwoFactor([FromBody] ToggleTwoFactorRequest request, [FromQuery] string? citizenName)
    {
        var result = await _profileService.ToggleTwoFactorAsync(request.Enabled, citizenName);
        return Ok(result);
    }

    [HttpPost("reset")]
    public async Task<IActionResult> ResetProfile([FromQuery] string? citizenName)
    {
        var result = await _profileService.ResetProfileAsync(citizenName);
        return Ok(result);
    }
}
