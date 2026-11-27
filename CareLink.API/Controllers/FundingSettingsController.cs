using Microsoft.AspNetCore.Authorization;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Authorize]
[Route("api/funding-settings")]
public class FundingSettingsController : ControllerBase
{
    private readonly IFundingSettingsService _service;

    public FundingSettingsController(IFundingSettingsService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetSettings(CancellationToken cancellationToken)
    {
        var settings = await _service.GetSettingsAsync(cancellationToken);
        return Ok(settings);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateSettings([FromBody] FundingSettingsDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updated = await _service.UpdateSettingsAsync(dto, cancellationToken);
        return Ok(updated);
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangeFundingPasswordDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var success = await _service.ChangePasswordAsync(dto, cancellationToken);
        if (!success)
        {
            return BadRequest(new { message = "Invalid current password or update failed." });
        }

        return Ok(new { message = "Password updated successfully." });
    }
}
