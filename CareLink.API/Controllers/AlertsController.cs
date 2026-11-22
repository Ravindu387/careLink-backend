namespace CareLink.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class AlertsController : ControllerBase
{
    private readonly IAlertService _alertService;

    public AlertsController(IAlertService alertService)
    {
        _alertService = alertService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAlerts([FromQuery] string? tab, [FromQuery] string? search)
    {
        var result = await _alertService.GetAlertsAsync(tab, search);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetAlertById(int id)
    {
        var result = await _alertService.GetAlertByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var result = await _alertService.GetAlertSummaryAsync();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAlert([FromBody] CreateAlertRequest request)
    {
        var result = await _alertService.CreateAlertAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return CreatedAtAction(nameof(GetAlertById), new { id = result.Data!.Id }, result);
    }

    [HttpPatch("{id:int}/acknowledge")]
    public async Task<IActionResult> AcknowledgeAlert(int id)
    {
        var result = await _alertService.AcknowledgeAlertAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpPatch("{id:int}/resolve")]
    public async Task<IActionResult> ResolveAlert(int id)
    {
        var result = await _alertService.ResolveAlertAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpPost("mark-all-reviewed")]
    public async Task<IActionResult> MarkAllReviewed()
    {
        var result = await _alertService.MarkAllReviewedAsync();
        return Ok(result);
    }

    [HttpGet("rules")]
    public async Task<IActionResult> GetRules()
    {
        var result = await _alertService.GetRulesAsync();
        return Ok(result);
    }

    [HttpPut("rules")]
    public async Task<IActionResult> UpdateRules([FromBody] AlertRulesDto rulesDto)
    {
        var result = await _alertService.UpdateRulesAsync(rulesDto);
        return Ok(result);
    }
}
