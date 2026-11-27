using Microsoft.AspNetCore.Authorization;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Authorize]
[Route("api/residents")]
public class ResidentsController : ControllerBase
{
    private readonly IResidentService _residentService;
    private readonly ILogger<ResidentsController> _logger;

    public ResidentsController(
        IResidentService residentService,
        ILogger<ResidentsController> logger)
    {
        _residentService = residentService;
        _logger = logger;
    }

    /// <summary>
    /// Gets paged list of residents with search and filter parameters.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResultDto<ResidentDto>>> GetPaged(
        [FromQuery] ResidentQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = await _residentService.GetPagedResidentsAsync(parameters, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets all residents without pagination (useful for exports).
    /// </summary>
    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<ResidentDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _residentService.GetAllResidentsAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets summary counts and KPI metrics for resident management.
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult<ResidentSummaryDto>> GetSummary(CancellationToken cancellationToken)
    {
        var summary = await _residentService.GetSummaryAsync(cancellationToken);
        return Ok(summary);
    }

    /// <summary>
    /// Gets a single resident by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ResidentDto>> GetById(
        string id,
        CancellationToken cancellationToken)
    {
        var resident = await _residentService.GetResidentByIdAsync(id, cancellationToken);
        if (resident == null)
        {
            return NotFound(new { message = $"Resident with ID '{id}' was not found." });
        }
        return Ok(resident);
    }

    /// <summary>
    /// Registers a new resident.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ResidentDto>> Create(
        [FromBody] CreateResidentDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var created = await _residentService.CreateResidentAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating resident");
            return StatusCode(500, new { message = "An error occurred while registering the resident." });
        }
    }

    /// <summary>
    /// Updates details of an existing resident.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ResidentDto>> Update(
        string id,
        [FromBody] UpdateResidentDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var updated = await _residentService.UpdateResidentAsync(id, dto, cancellationToken);
            if (updated == null)
            {
                return NotFound(new { message = $"Resident with ID '{id}' was not found." });
            }
            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating resident {Id}", id);
            return StatusCode(500, new { message = "An error occurred while updating the resident." });
        }
    }

    /// <summary>
    /// Updates the status of a resident.
    /// </summary>
    [HttpPatch("{id}/status")]
    public async Task<ActionResult<ResidentDto>> UpdateStatus(
        string id,
        [FromBody] UpdateResidentStatusDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updated = await _residentService.UpdateStatusAsync(id, dto, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { message = $"Resident with ID '{id}' was not found." });
        }
        return Ok(updated);
    }

    /// <summary>
    /// Checks out a resident.
    /// </summary>
    [HttpPost("{id}/checkout")]
    public async Task<ActionResult<ResidentDto>> Checkout(
        string id,
        CancellationToken cancellationToken)
    {
        var updated = await _residentService.CheckoutResidentAsync(id, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { message = $"Resident with ID '{id}' was not found." });
        }
        return Ok(updated);
    }

    /// <summary>
    /// Deletes a resident record.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(
        string id,
        CancellationToken cancellationToken)
    {
        var deleted = await _residentService.DeleteResidentAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound(new { message = $"Resident with ID '{id}' was not found." });
        }
        return NoContent();
    }
}
