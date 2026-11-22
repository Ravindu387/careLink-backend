namespace CareLink.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;

[ApiController]
[Route("api/citizen")]
[Route("api/citizen-requests")]
[Route("api/nearby-support")]
[Route("api/citizen/nearby-support")]
public class CitizenHomeController : ControllerBase
{
    private readonly ICitizenHomeService _citizenHomeService;

    public CitizenHomeController(ICitizenHomeService citizenHomeService)
    {
        _citizenHomeService = citizenHomeService;
    }

    [HttpGet("home/overview")]
    public async Task<IActionResult> GetHomeOverview([FromQuery] string? citizenName)
    {
        var result = await _citizenHomeService.GetHomeOverviewAsync(citizenName);
        return Ok(result);
    }

    [HttpGet("requests")]
    public async Task<IActionResult> GetRequests(
        [FromQuery] string? citizenName,
        [FromQuery] string? search,
        [FromQuery] string? status)
    {
        var result = await _citizenHomeService.GetRequestsAsync(citizenName, search, status);
        return Ok(result);
    }

    [HttpGet("requests/{id}")]
    public async Task<IActionResult> GetRequestById(string id)
    {
        var result = await _citizenHomeService.GetRequestByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpPost("requests")]
    public async Task<IActionResult> CreateRequest([FromBody] CreateCitizenRequest request)
    {
        var result = await _citizenHomeService.CreateRequestAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return CreatedAtAction(nameof(GetRequestById), new { id = result.Data!.Id }, result);
    }

    [HttpPatch("requests/{id}/status")]
    public async Task<IActionResult> UpdateRequestStatus(string id, [FromBody] UpdateCitizenRequestStatus request)
    {
        var result = await _citizenHomeService.UpdateRequestStatusAsync(id, request);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpGet("requests/summary")]
    public async Task<IActionResult> GetRequestsSummary([FromQuery] string? citizenName)
    {
        var result = await _citizenHomeService.GetRequestsSummaryAsync(citizenName);
        return Ok(result);
    }

    [HttpDelete("requests/{id}")]
    public async Task<IActionResult> DeleteRequest(string id)
    {
        var result = await _citizenHomeService.DeleteRequestAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpGet("facilities")]
    public async Task<IActionResult> GetFacilities(
        [FromQuery] string? search,
        [FromQuery] string? category,
        [FromQuery] string? availability,
        [FromQuery] string? distance)
    {
        var result = await _citizenHomeService.GetFacilitiesAsync(search, category, availability, distance);
        return Ok(result);
    }

    [HttpGet("facilities/summary")]
    public async Task<IActionResult> GetNearbySupportSummary()
    {
        var result = await _citizenHomeService.GetNearbySupportSummaryAsync();
        return Ok(result);
    }

    [HttpGet("facilities/{id}")]
    public async Task<IActionResult> GetFacilityById(string id)
    {
        var result = await _citizenHomeService.GetFacilityByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpPatch("facilities/{id}/status")]
    public async Task<IActionResult> UpdateFacilityStatus(string id, [FromBody] UpdateCitizenRequestStatus request)
    {
        var result = await _citizenHomeService.UpdateFacilityStatusAsync(id, request.Status);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpPost("facilities")]
    public async Task<IActionResult> CreateFacility([FromBody] CreateFacilityRequest request)
    {
        var result = await _citizenHomeService.CreateFacilityAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return CreatedAtAction(nameof(GetFacilityById), new { id = result.Data!.Id }, result);
    }

    [HttpGet("operation")]
    public async Task<IActionResult> GetActiveOperation()
    {
        var result = await _citizenHomeService.GetActiveOperationAsync();
        return Ok(result);
    }
}
