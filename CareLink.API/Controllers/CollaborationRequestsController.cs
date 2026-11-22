using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Route("api/collaboration-requests")]
[Route("api/collaborations")]
public class CollaborationRequestsController : ControllerBase
{
    private readonly ICollaborationRequestService _service;
    private readonly ILogger<CollaborationRequestsController> _logger;

    public CollaborationRequestsController(
        ICollaborationRequestService service,
        ILogger<CollaborationRequestsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves collaboration requests with optional filtering by search text, tab, sector, partner, operational area, and status.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CollaborationRequestDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? tab,
        [FromQuery] string? sector,
        [FromQuery] string? partner,
        [FromQuery] string? area,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var query = new CollaborationQueryParameters
        {
            Search = search,
            Tab = tab,
            Sector = sector,
            Partner = partner,
            Area = area,
            Status = status
        };

        var results = await _service.GetAllAsync(query, cancellationToken);
        return Ok(results);
    }

    /// <summary>
    /// Retrieves summary metrics for collaboration requests including open collaborations, active partnerships, and tab counts.
    /// </summary>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(CollaborationRequestMetricsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMetrics(CancellationToken cancellationToken)
    {
        var metrics = await _service.GetMetricsAsync(cancellationToken);
        return Ok(metrics);
    }

    /// <summary>
    /// Retrieves a single collaboration request by its unique identifier.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CollaborationRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        var item = await _service.GetByIdAsync(id, cancellationToken);
        if (item == null)
        {
            return NotFound(new { error = $"Collaboration request '{id}' was not found." });
        }

        return Ok(item);
    }

    /// <summary>
    /// Creates a new partner collaboration request.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CollaborationRequestDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCollaborationRequestDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _service.CreateAsync(dto, cancellationToken);
        _logger.LogInformation("Created collaboration request {Id} for partner {Partner}: {Title}",
            result.Id, result.Partner, result.Title);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates details of an existing collaboration request.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CollaborationRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        string id,
        [FromBody] UpdateCollaborationRequestDto dto,
        CancellationToken cancellationToken)
    {
        var updated = await _service.UpdateAsync(id, dto, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { error = $"Collaboration request '{id}' was not found." });
        }

        return Ok(updated);
    }

    /// <summary>
    /// Updates the status of a collaboration request (e.g. Accepted, In Progress, Completed, Archived).
    /// </summary>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(CollaborationRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateStatus(
        string id,
        [FromBody] UpdateCollaborationStatusDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updated = await _service.UpdateStatusAsync(id, dto, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { error = $"Collaboration request '{id}' was not found." });
        }

        _logger.LogInformation("Updated collaboration request {Id} status to {Status}", id, dto.Status);
        return Ok(updated);
    }

    /// <summary>
    /// Deletes a collaboration request.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        var success = await _service.DeleteAsync(id, cancellationToken);
        if (!success)
        {
            return NotFound(new { error = $"Collaboration request '{id}' was not found." });
        }

        return NoContent();
    }

    /// <summary>
    /// Resets collaboration requests to the default demonstration dataset.
    /// </summary>
    [HttpPost("reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Reset(CancellationToken cancellationToken)
    {
        await _service.ResetToDefaultAsync(cancellationToken);
        var metrics = await _service.GetMetricsAsync(cancellationToken);
        return Ok(new { message = "Collaboration requests reset to default demonstration data.", metrics });
    }
}
