using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Route("api/assistance-requests")]
public class AssistanceRequestsController : ControllerBase
{
    private readonly IAssistanceRequestService _service;
    private readonly ILogger<AssistanceRequestsController> _logger;

    public AssistanceRequestsController(
        IAssistanceRequestService service,
        ILogger<AssistanceRequestsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves assistance requests with optional filtering by search query, tab, crisis event, priority, category, status, and organisation.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AssistanceRequestResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? tab,
        [FromQuery] string? crisisEvent,
        [FromQuery] string? priority,
        [FromQuery] string? category,
        [FromQuery] string? status,
        [FromQuery] string? organisation,
        [FromQuery] bool? verifiedOnly,
        [FromQuery] bool? unassignedOnly,
        CancellationToken cancellationToken)
    {
        var query = new AssistanceRequestQueryParameters
        {
            Search = search,
            Tab = tab,
            CrisisEvent = crisisEvent,
            Priority = priority,
            Category = category,
            Status = status,
            Organisation = organisation,
            VerifiedOnly = verifiedOnly,
            UnassignedOnly = unassignedOnly
        };

        var results = await _service.GetAllAsync(query, cancellationToken);
        return Ok(results);
    }

    /// <summary>
    /// Retrieves aggregate metrics and counts for active assistance requests across tabs.
    /// </summary>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(AssistanceRequestMetricsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMetrics(CancellationToken cancellationToken)
    {
        var metrics = await _service.GetMetricsAsync(cancellationToken);
        return Ok(metrics);
    }

    /// <summary>
    /// Retrieves a single assistance request by its unique identifier (e.g. REQ-2841).
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AssistanceRequestResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        var item = await _service.GetByIdAsync(id, cancellationToken);
        if (item == null)
        {
            return NotFound(new { error = $"Assistance request '{id}' was not found." });
        }

        return Ok(item);
    }

    /// <summary>
    /// Creates and registers a new humanitarian assistance request, automatically running AI triage evaluation.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AssistanceRequestResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateAssistanceRequestDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _service.CreateAsync(dto, cancellationToken);
        _logger.LogInformation("Assistance request {Id} registered for {Category} at {Location}",
            result.Id, result.Category, result.Location);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates an existing assistance request.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(AssistanceRequestResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        string id,
        [FromBody] UpdateAssistanceRequestDto dto,
        CancellationToken cancellationToken)
    {
        var updated = await _service.UpdateAsync(id, dto, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { error = $"Assistance request '{id}' was not found." });
        }

        return Ok(updated);
    }

    /// <summary>
    /// Assigns or reassigns an assistance request to a response team and organisation.
    /// </summary>
    [HttpPatch("{id}/assign")]
    [ProducesResponseType(typeof(AssistanceRequestResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Assign(
        string id,
        [FromBody] AssignAssistanceRequestDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updated = await _service.AssignAsync(id, dto, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { error = $"Assistance request '{id}' was not found." });
        }

        _logger.LogInformation("Assistance request {Id} assigned to {Assigned} ({Organisation})",
            id, dto.Assigned, dto.Organisation ?? updated.Organisation);

        return Ok(updated);
    }

    /// <summary>
    /// Updates the operational status of an assistance request.
    /// </summary>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(AssistanceRequestResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(
        string id,
        [FromBody] UpdateAssistanceRequestStatusDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var validStatuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Submitted", "Under Review", "Assigned", "In Progress", "Escalated", "Completed", "Cancelled"
        };

        if (!validStatuses.Contains(dto.Status))
        {
            return BadRequest(new { error = $"Status must be one of: {string.Join(", ", validStatuses)}." });
        }

        var updated = await _service.UpdateStatusAsync(id, dto, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { error = $"Assistance request '{id}' was not found." });
        }

        _logger.LogInformation("Assistance request {Id} status updated to {Status}", id, dto.Status);
        return Ok(updated);
    }

    /// <summary>
    /// Deletes an assistance request.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound(new { error = $"Assistance request '{id}' was not found." });
        }

        return NoContent();
    }

    /// <summary>
    /// Resets assistance requests and metrics back to initial seed data.
    /// </summary>
    [HttpPost("reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Reset(CancellationToken cancellationToken)
    {
        await _service.ResetDataAsync(cancellationToken);
        var metrics = await _service.GetMetricsAsync(cancellationToken);
        return Ok(new { message = "Assistance requests reset to default baseline.", metrics });
    }
}
