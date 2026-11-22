using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Route("api/staff")]
public class StaffController : ControllerBase
{
    private readonly IStaffService _staffService;
    private readonly ILogger<StaffController> _logger;

    public StaffController(IStaffService staffService, ILogger<StaffController> logger)
    {
        _staffService = staffService;
        _logger = logger;
    }

    /// <summary>
    /// Gets paged list of staff members with search, filters, and pagination.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResultDto<StaffMemberDto>>> GetPaged(
        [FromQuery] StaffQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = await _staffService.GetPagedStaffAsync(parameters, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets all staff members without pagination.
    /// </summary>
    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<StaffMemberDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _staffService.GetAllStaffAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets summary counts for staff KPI metrics cards.
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult<StaffSummaryDto>> GetSummary(CancellationToken cancellationToken)
    {
        var summary = await _staffService.GetSummaryAsync(cancellationToken);
        return Ok(summary);
    }

    /// <summary>
    /// Gets weekly shift coverage.
    /// </summary>
    [HttpGet("coverage")]
    public async Task<ActionResult<IEnumerable<ShiftCoverageDto>>> GetCoverage(
        [FromQuery] string? shelter,
        CancellationToken cancellationToken)
    {
        var coverage = await _staffService.GetCoverageAsync(shelter, cancellationToken);
        return Ok(coverage);
    }

    /// <summary>
    /// Updates weekly shift coverage.
    /// </summary>
    [HttpPut("coverage")]
    public async Task<ActionResult<IEnumerable<ShiftCoverageDto>>> UpdateCoverage(
        [FromBody] IEnumerable<ShiftCoverageDto> coverage,
        CancellationToken cancellationToken)
    {
        var updated = await _staffService.UpdateCoverageAsync(coverage, cancellationToken);
        return Ok(updated);
    }

    /// <summary>
    /// Gets a single staff member by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<StaffMemberDto>> GetById(
        string id,
        CancellationToken cancellationToken)
    {
        var member = await _staffService.GetStaffByIdAsync(id, cancellationToken);
        if (member == null)
        {
            return NotFound(new { message = $"Staff member with ID '{id}' was not found." });
        }
        return Ok(member);
    }

    /// <summary>
    /// Registers a new staff member.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<StaffMemberDto>> Create(
        [FromBody] CreateStaffMemberDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var created = await _staffService.CreateStaffAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering staff member");
            return StatusCode(500, new { message = "An error occurred while registering the staff member." });
        }
    }

    /// <summary>
    /// Updates details of an existing staff member (availability, shift, contact, etc.).
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<StaffMemberDto>> Update(
        string id,
        [FromBody] UpdateStaffMemberDto dto,
        CancellationToken cancellationToken)
    {
        var updated = await _staffService.UpdateStaffAsync(id, dto, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { message = $"Staff member with ID '{id}' was not found." });
        }
        return Ok(updated);
    }

    /// <summary>
    /// Partially updates an existing staff member.
    /// </summary>
    [HttpPatch("{id}")]
    public async Task<ActionResult<StaffMemberDto>> Patch(
        string id,
        [FromBody] UpdateStaffMemberDto dto,
        CancellationToken cancellationToken)
    {
        var updated = await _staffService.UpdateStaffAsync(id, dto, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { message = $"Staff member with ID '{id}' was not found." });
        }
        return Ok(updated);
    }

    /// <summary>
    /// Deletes a staff member.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(
        string id,
        CancellationToken cancellationToken)
    {
        var deleted = await _staffService.DeleteStaffAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound(new { message = $"Staff member with ID '{id}' was not found." });
        }
        return NoContent();
    }
}
