namespace CareLink.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;

[ApiController]
[Route("api/shelters")]
[Route("api/citizen/shelters")]
public class SheltersController : ControllerBase
{
    private readonly IShelterService _shelterService;

    public SheltersController(IShelterService shelterService)
    {
        _shelterService = shelterService;
    }

    [HttpGet]
    public async Task<IActionResult> GetShelters(
        [FromQuery] string? search,
        [FromQuery] string? area,
        [FromQuery] double? maxDistance,
        [FromQuery] string? availability,
        [FromQuery] bool? accessibleOnly,
        [FromQuery] string? service)
    {
        var result = await _shelterService.GetSheltersAsync(search, area, maxDistance, availability, accessibleOnly, service);
        return Ok(result);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] string? citizenLocation)
    {
        var result = await _shelterService.GetSummaryAsync(citizenLocation);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _shelterService.GetByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateShelterRequest request)
    {
        var result = await _shelterService.CreateAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateShelterRequest request)
    {
        var result = await _shelterService.UpdateAsync(id, request);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _shelterService.DeleteAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpPost("{id}/request-space")]
    [HttpPost("reserve")]
    public async Task<IActionResult> RequestSpace(string? id, [FromBody] RequestShelterSpacePayload request, [FromQuery] string? citizenName)
    {
        if (!string.IsNullOrWhiteSpace(id) && string.IsNullOrWhiteSpace(request.ShelterId))
        {
            request.ShelterId = id;
        }

        var result = await _shelterService.RequestSpaceAsync(request, citizenName);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpGet("my-bookings")]
    public async Task<IActionResult> GetMyBookings([FromQuery] string? citizenName)
    {
        var result = await _shelterService.GetCitizenBookingsAsync(citizenName);
        return Ok(result);
    }
}
