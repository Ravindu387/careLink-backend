using Microsoft.AspNetCore.Authorization;
namespace CareLink.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;

[ApiController]
[Authorize]
[Route("api/household")]
[Route("api/citizen/household")]
public class HouseholdController : ControllerBase
{
    private readonly IHouseholdService _householdService;

    public HouseholdController(IHouseholdService householdService)
    {
        _householdService = householdService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProfile([FromQuery] string? citizenName)
    {
        var result = await _householdService.GetProfileAsync(citizenName);
        return Ok(result);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] string? citizenName)
    {
        var result = await _householdService.GetSummaryAsync(citizenName);
        return Ok(result);
    }

    [HttpGet("members")]
    public async Task<IActionResult> GetMembers([FromQuery] string? citizenName)
    {
        var result = await _householdService.GetMembersAsync(citizenName);
        return Ok(result);
    }

    [HttpGet("members/{id}")]
    public async Task<IActionResult> GetMemberById(string id, [FromQuery] string? citizenName)
    {
        var result = await _householdService.GetMemberByIdAsync(id, citizenName);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpPost("members")]
    public async Task<IActionResult> AddMember([FromBody] CreateHouseholdMemberRequest request, [FromQuery] string? citizenName)
    {
        var result = await _householdService.AddMemberAsync(request, citizenName);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return CreatedAtAction(nameof(GetMemberById), new { id = result.Data!.Id }, result);
    }

    [HttpPut("members/{id}")]
    public async Task<IActionResult> UpdateMember(string id, [FromBody] UpdateHouseholdMemberRequest request, [FromQuery] string? citizenName)
    {
        var result = await _householdService.UpdateMemberAsync(id, request, citizenName);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpDelete("members/{id}")]
    public async Task<IActionResult> DeleteMember(string id, [FromQuery] string? citizenName)
    {
        var result = await _householdService.DeleteMemberAsync(id, citizenName);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpPatch("address")]
    [HttpPut("address")]
    public async Task<IActionResult> UpdateAddress([FromBody] UpdateAddressRequest request, [FromQuery] string? citizenName)
    {
        var result = await _householdService.UpdateAddressAsync(request, citizenName);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPatch("emergency-contact")]
    [HttpPut("emergency-contact")]
    public async Task<IActionResult> UpdateEmergencyContact([FromBody] UpdateEmergencyContactRequest request, [FromQuery] string? citizenName)
    {
        var result = await _householdService.UpdateEmergencyContactAsync(request, citizenName);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPost("readiness/toggle")]
    [HttpPatch("readiness")]
    public async Task<IActionResult> ToggleReadiness([FromBody] ToggleReadinessRequest request, [FromQuery] string? citizenName)
    {
        var result = await _householdService.ToggleReadinessAsync(request, citizenName);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }
}
