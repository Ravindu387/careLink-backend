using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Route("api/field-activities")]
[Route("api/fieldactivities")]
[Route("api/partner/field-activities")]
public class FieldActivitiesController : ControllerBase
{
    private readonly IFieldActivityService _service;
    private readonly ILogger<FieldActivitiesController> _logger;

    public FieldActivitiesController(
        IFieldActivityService service,
        ILogger<FieldActivitiesController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves field activities with optional filtering by search text, tab, sector, location, team, and status.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<FieldActivityDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? tab,
        [FromQuery] string? sector,
        [FromQuery] string? location,
        [FromQuery] string? team,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var query = new FieldActivityQueryParameters
        {
            Search = search,
            Tab = tab,
            Sector = sector,
            Location = location,
            Team = team,
            Status = status
        };

        var results = await _service.GetAllAsync(query, cancellationToken);
        return Ok(results);
    }

    /// <summary>
    /// Retrieves summary metrics for field activities including active operations today, people reached, and teams deployed.
    /// </summary>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(FieldActivityMetricsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMetrics(CancellationToken cancellationToken)
    {
        var metrics = await _service.GetMetricsAsync(cancellationToken);
        return Ok(metrics);
    }

    /// <summary>
    /// Retrieves a single field activity by its unique identifier.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(FieldActivityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        var item = await _service.GetByIdAsync(id, cancellationToken);
        if (item == null)
        {
            return NotFound(new { error = $"Field activity '{id}' was not found." });
        }

        return Ok(item);
    }

    /// <summary>
    /// Creates and schedules a new field activity operation.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(FieldActivityDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateFieldActivityDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _service.CreateAsync(dto, cancellationToken);
        _logger.LogInformation("Field activity {Id} created by {Team}: {Name}", result.Id, result.Team, result.Name);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates details of an existing field activity.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(FieldActivityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        string id,
        [FromBody] UpdateFieldActivityDto dto,
        CancellationToken cancellationToken)
    {
        var updated = await _service.UpdateAsync(id, dto, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { error = $"Field activity '{id}' was not found." });
        }

        return Ok(updated);
    }

    /// <summary>
    /// Updates the status of a field activity (e.g. In Progress, Active, Completed, Scheduled, Archived).
    /// </summary>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(FieldActivityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateStatus(
        string id,
        [FromBody] UpdateFieldActivityStatusDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updated = await _service.UpdateStatusAsync(id, dto, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { error = $"Field activity '{id}' was not found." });
        }

        _logger.LogInformation("Field activity {Id} status updated to {Status}", id, dto.Status);
        return Ok(updated);
    }

    /// <summary>
    /// Deletes a field activity record.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        var success = await _service.DeleteAsync(id, cancellationToken);
        if (!success)
        {
            return NotFound(new { error = $"Field activity '{id}' was not found." });
        }

        return NoContent();
    }

    /// <summary>
    /// Resets field activities to default demonstration dataset.
    /// </summary>
    [HttpPost("reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Reset(CancellationToken cancellationToken)
    {
        await _service.ResetToDefaultAsync(cancellationToken);
        var metrics = await _service.GetMetricsAsync(cancellationToken);
        return Ok(new { message = "Field activities reset to default state.", metrics });
    }
}
