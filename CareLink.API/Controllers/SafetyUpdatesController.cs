using Microsoft.AspNetCore.Authorization;
namespace CareLink.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;

[ApiController]
[Authorize]
[Route("api/safety-updates")]
[Route("api/citizen/safety-updates")]
public class SafetyUpdatesController : ControllerBase
{
    private readonly ISafetyUpdateService _service;

    public SafetyUpdatesController(ISafetyUpdateService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetUpdates(
        [FromQuery] string? area,
        [FromQuery] string? level,
        [FromQuery] string? search)
    {
        var result = await _service.GetUpdatesAsync(area, level, search);
        return Ok(result);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] string? area)
    {
        var result = await _service.GetSummaryAsync(area);
        return Ok(result);
    }

    [HttpGet("areas")]
    public async Task<IActionResult> GetAreas()
    {
        var result = await _service.GetMonitoredAreasAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _service.GetByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSafetyUpdateRequest request)
    {
        var result = await _service.CreateAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateSafetyUpdateRequest request)
    {
        var result = await _service.UpdateAsync(id, request);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _service.DeleteAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }
}
