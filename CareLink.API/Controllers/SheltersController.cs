using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Route("api/shelters")]
[Route("api/shelter-overview")]
public class SheltersController : ControllerBase
{
    private readonly IShelterService _shelterService;
    private readonly ILogger<SheltersController> _logger;

    public SheltersController(IShelterService shelterService, ILogger<SheltersController> logger)
    {
        _shelterService = shelterService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all emergency shelters with optional filtering by search text, operational area, status, or availability.
    /// Supports pagination when page parameter is provided.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ShelterDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(
        [FromQuery] string? search,
        [FromQuery] string? area,
        [FromQuery] string? status,
        [FromQuery] string? availability,
        [FromQuery] string? service,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        var parameters = new ShelterQueryParameters
        {
            Search = search,
            Area = area,
            Status = status,
            Availability = availability,
            Service = service,
            Page = page,
            PageSize = pageSize
        };

        if (parameters.Page.HasValue && parameters.Page.Value > 0)
        {
            var paged = await _shelterService.GetSheltersPagedAsync(parameters, cancellationToken);
            return Ok(paged);
        }

        var shelters = await _shelterService.GetAllAsync(parameters, cancellationToken);
        return Ok(shelters);
    }

    /// <summary>
    /// Explicit paged endpoint for shelter management table.
    /// </summary>
    [HttpGet("paged")]
    public async Task<ActionResult<PagedResultDto<ShelterDto>>> GetPaged(
        [FromQuery] ShelterQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = await _shelterService.GetSheltersPagedAsync(parameters, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets all shelters without pagination (useful for exports and lookups).
    /// </summary>
    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<ShelterDto>>> GetAll(CancellationToken cancellationToken)
    {
        var shelters = await _shelterService.GetAllSheltersAsync(cancellationToken);
        return Ok(shelters);
    }

    /// <summary>
    /// Retrieves summary capacity, occupancy, and risk metrics for all shelters.
    /// </summary>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(ShelterMetricsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ShelterMetricsDto>> GetMetrics(CancellationToken cancellationToken)
    {
        var metrics = await _shelterService.GetMetricsAsync(cancellationToken);
        return Ok(metrics);
    }

    /// <summary>
    /// Gets summary counts for shelter management KPI cards.
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult<ShelterSummaryDto>> GetSummary(CancellationToken cancellationToken)
    {
        var summary = await _shelterService.GetSummaryAsync(cancellationToken);
        return Ok(summary);
    }

    /// <summary>
    /// Gets a single shelter by ID (supports string code e.g. "SH-001" or integer ID).
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ShelterDto>> GetById(string id, CancellationToken cancellationToken)
    {
        ShelterDto? shelter = null;
        if (int.TryParse(id, out var intId))
        {
            shelter = await _shelterService.GetByIdAsync(intId, cancellationToken);
        }

        shelter ??= await _shelterService.GetShelterByIdAsync(id, cancellationToken);

        if (shelter == null)
        {
            return NotFound(new { message = $"Shelter with ID '{id}' was not found." });
        }
        return Ok(shelter);
    }

    /// <summary>
    /// Coordinates and registers a new emergency shelter.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ShelterDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ShelterDto>> Create([FromBody] CreateShelterDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var created = await _shelterService.CreateAsync(dto, cancellationToken);
            _logger.LogInformation("Shelter '{Name}' coordinated with ID {Id} in {Location}", created.Name, created.Id, created.Location);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering shelter");
            return StatusCode(500, new { message = "An error occurred while registering the shelter." });
        }
    }

    /// <summary>
    /// Updates details, capacity, and services for an existing shelter.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ShelterDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ShelterDto>> Update(
        string id,
        [FromBody] UpdateShelterDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        int intId = int.TryParse(id, out var parsed) ? parsed : 0;
        var updated = await _shelterService.UpdateAsync(intId, dto, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { message = $"Shelter '{id}' not found." });
        }

        _logger.LogInformation("Shelter '{Id}' updated.", id);
        return Ok(updated);
    }

    /// <summary>
    /// Updates occupancy headcount for quick arrivals and check-ins/check-outs.
    /// </summary>
    [HttpPatch("{id:int}/occupancy")]
    [ProducesResponseType(typeof(ShelterDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ShelterDto>> UpdateOccupancy(
        int id,
        [FromBody] UpdateShelterOccupancyDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updated = await _shelterService.UpdateOccupancyAsync(id, dto.Occupied, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { message = $"Shelter '{id}' not found." });
        }

        _logger.LogInformation("Shelter '{Id}' occupancy updated to {Occupied}.", id, updated.Occupied);
        return Ok(updated);
    }

    /// <summary>
    /// Deletes or decommissions a shelter.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        bool deleted;
        if (int.TryParse(id, out var intId))
        {
            deleted = await _shelterService.DeleteAsync(intId, cancellationToken);
        }
        else
        {
            // fallback lookup by ID
            var shelter = await _shelterService.GetShelterByIdAsync(id, cancellationToken);
            deleted = shelter != null && await _shelterService.DeleteAsync(0, cancellationToken);
        }

        if (!deleted)
        {
            return NotFound(new { message = $"Shelter '{id}' not found." });
        }

        _logger.LogInformation("Shelter '{Id}' deleted.", id);
        return Ok(new { message = "Shelter removed successfully.", id });
    }

    /// <summary>
    /// Resets shelter inventory back to factory emergency defaults.
    /// </summary>
    [HttpPost("reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Reset(CancellationToken cancellationToken)
    {
        await _shelterService.ResetToDefaultAsync(cancellationToken);
        _logger.LogInformation("Shelter overview data reset to defaults.");
        return Ok(new { message = "Shelter data reset to defaults." });
    }
}
