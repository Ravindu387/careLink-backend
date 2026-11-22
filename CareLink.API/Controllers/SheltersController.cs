using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Route("api/shelters")]
[Route("api/shelter-overview")]
public class SheltersController : ControllerBase
{
    private readonly IShelterService _service;
    private readonly ILogger<SheltersController> _logger;

    public SheltersController(
        IShelterService service,
        ILogger<SheltersController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all emergency shelters with optional filtering by search text, operational area, status, or availability.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ShelterDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ShelterDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? area,
        [FromQuery] string? status,
        [FromQuery] string? availability,
        [FromQuery] string? service,
        CancellationToken cancellationToken)
    {
        var query = new ShelterQueryParameters
        {
            Search = search,
            Area = area,
            Status = status,
            Availability = availability,
            Service = service
        };

        var shelters = await _service.GetAllAsync(query, cancellationToken);
        return Ok(shelters);
    }

    /// <summary>
    /// Retrieves summary capacity, occupancy, and risk metrics for all shelters.
    /// </summary>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(ShelterMetricsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ShelterMetricsDto>> GetMetrics(CancellationToken cancellationToken)
    {
        var metrics = await _service.GetMetricsAsync(cancellationToken);
        return Ok(metrics);
    }

    /// <summary>
    /// Retrieves a single shelter by its unique identifier.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ShelterDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShelterDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var shelter = await _service.GetByIdAsync(id, cancellationToken);
        if (shelter == null)
        {
            return NotFound(new { message = $"Shelter '{id}' not found." });
        }
        return Ok(shelter);
    }

    /// <summary>
    /// Coordinates and registers a new emergency shelter.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ShelterDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ShelterDto>> Create(
        [FromBody] CreateShelterDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var created = await _service.CreateAsync(dto, cancellationToken);
        _logger.LogInformation("Shelter '{Name}' coordinated with ID {Id} in {Location}", created.Name, created.Id, created.Location);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates details, capacity, and services for an existing shelter.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ShelterDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ShelterDto>> Update(
        int id,
        [FromBody] UpdateShelterDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updated = await _service.UpdateAsync(id, dto, cancellationToken);
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

        var updated = await _service.UpdateOccupancyAsync(id, dto.Occupied, cancellationToken);
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
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        bool deleted = await _service.DeleteAsync(id, cancellationToken);
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
        await _service.ResetToDefaultAsync(cancellationToken);
        _logger.LogInformation("Shelter overview data reset to defaults.");
        return Ok(new { message = "Shelter data reset to defaults." });
    }
}
