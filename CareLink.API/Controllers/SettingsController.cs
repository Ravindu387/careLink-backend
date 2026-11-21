using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Route("api/settings")]
public class SettingsController : ControllerBase
{
    private readonly ISettingsService _service;

    public SettingsController(ISettingsService service)
    {
        _service = service;
    }

    // ── Profile ──────────────────────────────────────────
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var result = await _service.GetProfileAsync();
        return Ok(result);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] ProfileDto dto)
    {
        var result = await _service.UpdateProfileAsync(dto);
        return Ok(result);
    }

    // ── Shelter ───────────────────────────────────────────
    [HttpGet("shelter")]
    public async Task<IActionResult> GetShelter()
    {
        var result = await _service.GetShelterSettingsAsync();
        return Ok(result);
    }

    [HttpPut("shelter")]
    public async Task<IActionResult> UpdateShelter([FromBody] ShelterSettingsDto dto)
    {
        var result = await _service.UpdateShelterSettingsAsync(dto);
        return Ok(result);
    }

    // ── Security ──────────────────────────────────────────
    [HttpGet("security")]
    public async Task<IActionResult> GetSecurity()
    {
        var result = await _service.GetSecuritySettingsAsync();
        return Ok(result);
    }

    [HttpPut("security")]
    public async Task<IActionResult> UpdateSecurity([FromBody] SecuritySettingsDto dto)
    {
        await _service.UpdateSecuritySettingsAsync(dto);
        return Ok(new { message = "Security settings updated." });
    }

    // ── Notifications ─────────────────────────────────────
    [HttpGet("notifications")]
    public async Task<IActionResult> GetNotifications()
    {
        var result = await _service.GetNotificationSettingsAsync();
        return Ok(result);
    }

    [HttpPut("notifications")]
    public async Task<IActionResult> UpdateNotifications([FromBody] NotificationSettingsDto dto)
    {
        var result = await _service.UpdateNotificationSettingsAsync(dto);
        return Ok(result);
    }
}
