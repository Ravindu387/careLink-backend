using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Authorize]
[Route("api/priority-queue")]
[Route("api/priorityqueue")]
[Route("api/coordinator/priority-queue")]
public class PriorityQueueController : ControllerBase
{
    private readonly IPriorityQueueService _service;
    private readonly ILogger<PriorityQueueController> _logger;

    public PriorityQueueController(
        IPriorityQueueService service,
        ILogger<PriorityQueueController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all priority queue requests matching optional search and filter criteria.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PriorityQueueItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PriorityQueueItemDto>>> GetAll(
        [FromQuery] PriorityQueueQueryParameters query,
        CancellationToken cancellationToken)
    {
        var items = await _service.GetAllAsync(query, cancellationToken);
        return Ok(items);
    }

    /// <summary>
    /// Retrieves paginated priority queue requests with paging metadata.
    /// </summary>
    [HttpGet("paged")]
    [ProducesResponseType(typeof(PagedPriorityQueueDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedPriorityQueueDto>> GetPaged(
        [FromQuery] PriorityQueueQueryParameters query,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetPagedAsync(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves summary metrics for the Priority Queue (Total, Unassigned, Overdue, Potential Duplicates, Critical Attention).
    /// </summary>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(PriorityQueueMetricsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PriorityQueueMetricsDto>> GetMetrics(CancellationToken cancellationToken)
    {
        var metrics = await _service.GetMetricsAsync(cancellationToken);
        return Ok(metrics);
    }

    /// <summary>
    /// Retrieves operational SLA response policies.
    /// </summary>
    [HttpGet("sla-policy")]
    [ProducesResponseType(typeof(IReadOnlyList<SlaPolicyDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SlaPolicyDto>>> GetSlaPolicies(CancellationToken cancellationToken)
    {
        var policies = await _service.GetSlaPoliciesAsync(cancellationToken);
        return Ok(policies);
    }

    /// <summary>
    /// Retrieves details for a specific priority request by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PriorityQueueItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PriorityQueueItemDto>> GetById(
        string id,
        CancellationToken cancellationToken)
    {
        var item = await _service.GetByIdAsync(id, cancellationToken);
        if (item == null)
        {
            return NotFound(new { error = $"Priority request '{id}' not found." });
        }
        return Ok(item);
    }

    /// <summary>
    /// Creates a new priority queue request.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PriorityQueueItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PriorityQueueItemDto>> Create(
        [FromBody] CreatePriorityRequestDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var created = await _service.CreateAsync(dto, cancellationToken);
        _logger.LogInformation("Priority request created with ID {Id} for {Category} at {Location}", created.Id, created.Category, created.Location);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Assigns a response team or unit to a priority request.
    /// </summary>
    [HttpPatch("{id}/assign")]
    [ProducesResponseType(typeof(PriorityQueueItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PriorityQueueItemDto>> AssignTeam(
        string id,
        [FromBody] AssignTeamDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updated = await _service.AssignTeamAsync(id, dto, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { error = $"Priority request '{id}' not found." });
        }

        _logger.LogInformation("Priority request {Id} assigned to {Team}", id, dto.AssignedTeam);
        return Ok(updated);
    }

    /// <summary>
    /// Updates the triage status (e.g. Under Review, Assigned, In Progress, Escalated, Completed) of a priority request.
    /// </summary>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(PriorityQueueItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PriorityQueueItemDto>> UpdateStatus(
        string id,
        [FromBody] UpdatePriorityStatusDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updated = await _service.UpdateStatusAsync(id, dto, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { error = $"Priority request '{id}' not found." });
        }

        _logger.LogInformation("Priority request {Id} status updated to {Status}", id, dto.Status);
        return Ok(updated);
    }

    /// <summary>
    /// Overrides the priority level and confidence of a request.
    /// </summary>
    [HttpPatch("{id}/priority")]
    [ProducesResponseType(typeof(PriorityQueueItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PriorityQueueItemDto>> UpdatePriority(
        string id,
        [FromBody] UpdatePriorityLevelDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updated = await _service.UpdatePriorityAsync(id, dto, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { error = $"Priority request '{id}' not found." });
        }

        _logger.LogInformation("Priority request {Id} priority updated to {Priority}", id, updated.Priority);
        return Ok(updated);
    }

    /// <summary>
    /// Deletes a priority request by ID.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        string id,
        CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound(new { error = $"Priority request '{id}' not found." });
        }

        _logger.LogInformation("Priority request {Id} deleted", id);
        return NoContent();
    }

    /// <summary>
    /// Resets priority queue requests to default baseline data.
    /// </summary>
    [HttpPost("reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ResetDefaultData(CancellationToken cancellationToken)
    {
        await _service.ResetDefaultDataAsync(cancellationToken);
        _logger.LogInformation("Priority queue reset to default baseline data.");
        return Ok(new { message = "Priority queue reset to default baseline data." });
    }
}
