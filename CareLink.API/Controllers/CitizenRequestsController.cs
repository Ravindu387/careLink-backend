namespace CareLink.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;

[ApiController]
[Route("api/citizen/requests")]
[Route("api/requests")]
public class CitizenRequestsController : ControllerBase
{
    private readonly ICitizenRequestService _requestService;

    public CitizenRequestsController(ICitizenRequestService requestService)
    {
        _requestService = requestService;
    }

    [HttpGet]
    public async Task<IActionResult> GetRequests(
        [FromQuery] string? citizenName,
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] string? category)
    {
        var result = await _requestService.GetRequestsAsync(citizenName, search, status, category);
        return Ok(result);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] string? citizenName)
    {
        var result = await _requestService.GetSummaryAsync(citizenName);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _requestService.GetByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRequest([FromBody] CreateCitizenHelpRequestPayload request, [FromQuery] string? citizenName)
    {
        var result = await _requestService.CreateRequestAsync(request, citizenName);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [HttpPatch("{id}/status")]
    [HttpPost("{id}/status")]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateRequestStatusPayload request)
    {
        var result = await _requestService.UpdateStatusAsync(id, request.Status, request.Progress);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRequest(string id)
    {
        var result = await _requestService.DeleteRequestAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }
}
