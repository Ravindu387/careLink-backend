using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Route("api/resource-sharing")]
[Route("api/partner/resource-sharing")]
[Route("api/resource-commitments")]
public class ResourceSharingController : ControllerBase
{
    private readonly IResourceSharingService _service;
    private readonly ILogger<ResourceSharingController> _logger;

    public ResourceSharingController(
        IResourceSharingService service,
        ILogger<ResourceSharingController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all resource sharing commitments matching filters (search, tab, type, location, partner, status).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ResourceSharingCommitmentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ResourceSharingCommitmentDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? tab,
        [FromQuery] string? type,
        [FromQuery] string? location,
        [FromQuery] string? partner,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var query = new ResourceSharingQueryParameters
        {
            Search = search,
            Tab = tab,
            Type = type,
            Location = location,
            Partner = partner,
            Status = status
        };

        var list = await _service.GetAllAsync(query, cancellationToken);
        return Ok(list);
    }

    /// <summary>
    /// Retrieves resource sharing metrics and summary counts.
    /// </summary>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(ResourceSharingMetricsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResourceSharingMetricsDto>> GetMetrics(CancellationToken cancellationToken)
    {
        var metrics = await _service.GetMetricsAsync(cancellationToken);
        return Ok(metrics);
    }

    /// <summary>
    /// Retrieves a single resource sharing commitment by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ResourceSharingCommitmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResourceSharingCommitmentDto>> GetById(string id, CancellationToken cancellationToken)
    {
        var item = await _service.GetByIdAsync(id, cancellationToken);
        if (item == null)
        {
            return NotFound(new { error = $"Resource commitment '{id}' not found." });
        }
        return Ok(item);
    }

    /// <summary>
    /// Creates a new resource sharing commitment offer.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ResourceSharingCommitmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ResourceSharingCommitmentDto>> Create(
        [FromBody] CreateResourceSharingCommitmentDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var created = await _service.CreateAsync(dto, cancellationToken);
        _logger.LogInformation("Resource commitment '{Id}' created for {Resource} ({Quantity} units)", created.Id, created.Resource, created.Quantity);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates details of an existing resource sharing commitment.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ResourceSharingCommitmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ResourceSharingCommitmentDto>> Update(
        string id,
        [FromBody] UpdateResourceSharingCommitmentDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updated = await _service.UpdateAsync(id, dto, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { error = $"Resource commitment '{id}' not found." });
        }

        return Ok(updated);
    }

    /// <summary>
    /// Updates the status of an existing resource commitment (e.g. Approved, In Transit, Delivered, Completed, Archived).
    /// </summary>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(ResourceSharingCommitmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResourceSharingCommitmentDto>> UpdateStatus(
        string id,
        [FromBody] UpdateResourceSharingStatusDto dto,
        CancellationToken cancellationToken)
    {
        var updated = await _service.UpdateStatusAsync(id, dto.Status, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { error = $"Resource commitment '{id}' not found." });
        }

        _logger.LogInformation("Resource commitment '{Id}' status changed to {Status}", id, dto.Status);
        return Ok(updated);
    }

    /// <summary>
    /// Deletes a resource sharing commitment.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        var success = await _service.DeleteAsync(id, cancellationToken);
        if (!success)
        {
            return NotFound(new { error = $"Resource commitment '{id}' not found." });
        }

        _logger.LogInformation("Resource commitment '{Id}' deleted", id);
        return Ok(new { id, message = $"Resource commitment '{id}' deleted successfully." });
    }

    /// <summary>
    /// Resets resource sharing commitments to default demo state.
    /// </summary>
    [HttpPost("reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Reset(CancellationToken cancellationToken)
    {
        await _service.ResetAsync(cancellationToken);
        _logger.LogInformation("Resource sharing commitments reset to initial state.");
        return Ok(new { message = "Resource sharing commitments reset to initial state." });
    }
}
