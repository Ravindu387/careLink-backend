namespace CareLink.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class SettingsController : ControllerBase
{
    private readonly ISettingsService _settingsService;

    public SettingsController(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    [HttpGet]
    public async Task<IActionResult> GetSettings()
    {
        var result = await _settingsService.GetSettingsAsync();
        return Ok(result);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateSettingsRequest request)
    {
        var result = await _settingsService.UpdateSettingsAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPost("reset")]
    public async Task<IActionResult> ResetSettings()
    {
        var result = await _settingsService.ResetSettingsAsync();
        return Ok(result);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] ProfileSettingsDto dto)
    {
        var result = await _settingsService.UpdateProfileAsync(dto);
        if (!result.Success)
        {
            return BadRequest(result);
        }
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

    [HttpPut("notifications")]
    public async Task<IActionResult> UpdateNotifications([FromBody] NotificationPreferencesSettingsDto dto)
    {
        var result = await _settingsService.UpdateNotificationPreferencesAsync(dto);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPut("security")]
    public async Task<IActionResult> UpdateSecurity([FromBody] SecuritySettingsDto dto)
    {
        var result = await _settingsService.UpdateSecuritySettingsAsync(dto);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }
}
