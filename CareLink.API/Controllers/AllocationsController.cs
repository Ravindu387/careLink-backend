using Microsoft.AspNetCore.Authorization;
namespace CareLink.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AllocationsController : ControllerBase
{
    private readonly IAllocationService _allocationService;

    public AllocationsController(IAllocationService allocationService)
    {
        _allocationService = allocationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllocations(
        [FromQuery] string? tab,
        [FromQuery] string? search,
        [FromQuery] string? warehouse,
        [FromQuery] string? destination,
        [FromQuery] string? status)
    {
        var result = await _allocationService.GetAllocationsAsync(tab, search, warehouse, destination, status);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _allocationService.GetAllocationByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var result = await _allocationService.GetAllocationSummaryAsync();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAllocationRequest request)
    {
        var result = await _allocationService.CreateAllocationAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateAllocationStatusRequest request)
    {
        var result = await _allocationService.UpdateStatusAsync(id, request);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _allocationService.DeleteAllocationAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }
}
