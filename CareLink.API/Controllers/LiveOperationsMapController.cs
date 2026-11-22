using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Route("api/live-operations-map")]
[Route("api/operations-map")]
public class LiveOperationsMapController : ControllerBase
{
    private readonly ILiveOperationsMapService _service;
    private readonly ILogger<LiveOperationsMapController> _logger;

    public LiveOperationsMapController(
        ILiveOperationsMapService service,
        ILogger<LiveOperationsMapController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all operational map points (requests, shelters, warehouses, teams),
    /// with optional filters by search term, district, operational layer, priority, or status.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<OperationPointDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<OperationPointDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? district,
        [FromQuery] string? layer,
        [FromQuery] string? priority,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var query = new OperationPointQueryParameters
        {
            Search = search,
            District = district,
            Layer = layer,
            Priority = priority,
            Status = status
        };

        var points = await _service.GetAllAsync(query, cancellationToken);
        return Ok(points);
    }

    /// <summary>
    /// Retrieves aggregated metrics for the Live Operations Map (counts by layer, priority, total people affected, districts).
    /// </summary>
    [HttpGet("metrics")]
    [HttpGet("summary")]
    [ProducesResponseType(typeof(LiveOperationsMapMetricsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<LiveOperationsMapMetricsDto>> GetMetrics(CancellationToken cancellationToken)
    {
        var metrics = await _service.GetMetricsAsync(cancellationToken);
        return Ok(metrics);
    }

    /// <summary>
    /// Retrieves a single operation point by its unique id or requestId (e.g., REQ-2841, SHEL-038).
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OperationPointDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OperationPointDto>> GetById(
        string id,
        CancellationToken cancellationToken)
    {
        var point = await _service.GetByIdAsync(id, cancellationToken);
        if (point == null)
        {
            return NotFound(new { message = $"Operation point '{id}' not found." });
        }

        return Ok(point);
    }

    /// <summary>
    /// Creates a new operational point on the live map (new request, emergency shelter, supply warehouse, or response team).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OperationPointDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OperationPointDto>> Create(
        [FromBody] CreateOperationPointDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var created = await _service.CreateAsync(dto, cancellationToken);
        _logger.LogInformation("Operational point {RequestId} ({Title}) created in {District}",
            created.RequestId, created.Title, created.District);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates details of an existing operation point.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(OperationPointDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OperationPointDto>> Update(
        string id,
        [FromBody] UpdateOperationPointDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updated = await _service.UpdateAsync(id, dto, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { message = $"Operation point '{id}' not found." });
        }

        return Ok(updated);
    }

    /// <summary>
    /// Assigns or reassigns an emergency team to an operation point.
    /// </summary>
    [HttpPatch("{id}/assign")]
    [ProducesResponseType(typeof(OperationPointDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OperationPointDto>> AssignTeam(
        string id,
        [FromBody] AssignOperationPointDto dto,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto?.AssignedTeam))
        {
            return BadRequest(new { message = "AssignedTeam is required." });
        }

        var updated = await _service.AssignTeamAsync(id, dto, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { message = $"Operation point '{id}' not found." });
        }

        _logger.LogInformation("Operation point {Id} assigned to team {Team}", id, dto.AssignedTeam);
        return Ok(updated);
    }

    /// <summary>
    /// Updates the geographic canvas coordinates (X, Y percentage) of an operational point.
    /// </summary>
    [HttpPatch("{id}/coordinates")]
    [ProducesResponseType(typeof(OperationPointDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OperationPointDto>> UpdateCoordinates(
        string id,
        [FromBody] UpdateCoordinatesDto dto,
        CancellationToken cancellationToken)
    {
        if (dto.X < 0 || dto.X > 100 || dto.Y < 0 || dto.Y > 100)
        {
            return BadRequest(new { message = "Coordinates X and Y must be between 0 and 100 percent." });
        }

        var updated = await _service.UpdateCoordinatesAsync(id, dto, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { message = $"Operation point '{id}' not found." });
        }

        return Ok(updated);
    }

    /// <summary>
    /// Removes an operation point from the live operations map.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        string id,
        CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound(new { message = $"Operation point '{id}' not found." });
        }

        _logger.LogInformation("Operation point {Id} deleted", id);
        return Ok(new { id, message = $"Operation point '{id}' deleted successfully." });
    }

    /// <summary>
    /// Resets all live operation map data back to the default initial scenario.
    /// </summary>
    [HttpPost("reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ResetDefaultData(CancellationToken cancellationToken)
    {
        await _service.ResetDefaultDataAsync(cancellationToken);
        _logger.LogInformation("Live Operations Map reset to default seed data.");
        return Ok(new { message = "Live Operations Map reset to default seed data." });
    }
}
