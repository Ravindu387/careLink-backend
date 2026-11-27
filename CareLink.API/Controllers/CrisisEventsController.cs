using Microsoft.AspNetCore.Authorization;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
[Route("api/crisis-events")]
public class CrisisEventsController : ControllerBase
{
    private readonly ICrisisEventService _service;
    private readonly ILogger<CrisisEventsController> _logger;

    public CrisisEventsController(
        ICrisisEventService service,
        ILogger<CrisisEventsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves crisis events with optional filtering by search text, type, severity, status, date range, or archive state.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CrisisEventResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? type,
        [FromQuery] string? severity,
        [FromQuery] string? status,
        [FromQuery] string? dateRange,
        [FromQuery] bool? showArchived,
        CancellationToken cancellationToken)
    {
        var query = new CrisisEventQueryParameters
        {
            Search = search,
            Type = type,
            Severity = severity,
            Status = status,
            DateRange = dateRange,
            ShowArchived = showArchived
        };

        var results = await _service.GetAllAsync(query, cancellationToken);
        return Ok(results);
    }

    /// <summary>
    /// Retrieves aggregated dashboard metrics across all crisis events.
    /// </summary>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(CrisisEventMetricsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMetrics(CancellationToken cancellationToken)
    {
        var metrics = await _service.GetMetricsAsync(cancellationToken);
        return Ok(metrics);
    }

    /// <summary>
    /// Retrieves a single crisis event by its unique identifier (e.g. CR-2025-018).
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CrisisEventResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        var item = await _service.GetByIdAsync(id, cancellationToken);
        if (item == null)
        {
            return NotFound(new { error = $"Crisis event '{id}' was not found." });
        }

        return Ok(item);
    }

    /// <summary>
    /// Registers a new humanitarian crisis event into active operations.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CrisisEventResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCrisisEventDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _service.CreateAsync(dto, cancellationToken);
        _logger.LogInformation("Crisis event {Id} ({Name}) created in area {Area}",
            result.Id, result.Name, result.Area);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates an existing crisis event.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CrisisEventResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        string id,
        [FromBody] UpdateCrisisEventDto dto,
        CancellationToken cancellationToken)
    {
        var updated = await _service.UpdateAsync(id, dto, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { error = $"Crisis event '{id}' was not found." });
        }

        return Ok(updated);
    }

    /// <summary>
    /// Updates the operational status of a crisis event (Active, Monitoring, Resolved).
    /// </summary>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(CrisisEventResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(
        string id,
        [FromBody] UpdateCrisisStatusDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var valid = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Active", "Monitoring", "Resolved", "Archived" };
        if (!valid.Contains(dto.Status))
        {
            return BadRequest(new { error = $"Status must be one of: {string.Join(", ", valid)}." });
        }

        var updated = await _service.UpdateStatusAsync(id, dto, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { error = $"Crisis event '{id}' was not found." });
        }

        _logger.LogInformation("Crisis event {Id} status changed to {Status}", id, dto.Status);
        return Ok(updated);
    }

    /// <summary>
    /// Deletes or archives a crisis event.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound(new { error = $"Crisis event '{id}' was not found." });
        }

        return NoContent();
    }

    /// <summary>
    /// Resets crisis events back to default baseline state.
    /// </summary>
    [HttpPost("reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Reset(CancellationToken cancellationToken)
    {
        await _service.ResetDataAsync(cancellationToken);
        var metrics = await _service.GetMetricsAsync(cancellationToken);
        return Ok(new { message = "Crisis events reset to default state.", metrics });
    }

    /// <summary>
    /// Retrieves shared crisis events for partner officer workspace with query filtering.
    /// </summary>
    [HttpGet("shared")]
    [HttpGet("/api/shared-crisis-events")]
    [HttpGet("/api/partner/shared-crisis-events")]
    [ProducesResponseType(typeof(IReadOnlyList<SharedCrisisEventDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetShared(
        [FromQuery] string? search,
        [FromQuery] string? tab,
        [FromQuery] string? type,
        [FromQuery] string? severity,
        [FromQuery] string? area,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var query = new SharedCrisisQueryParameters
        {
            Search = search,
            Tab = tab,
            Type = type,
            Severity = severity,
            Area = area,
            Status = status
        };

        var results = await _service.GetSharedEventsAsync(query, cancellationToken);
        return Ok(results);
    }

    /// <summary>
    /// Retrieves shared crisis events statistics for partner officer workspace.
    /// </summary>
    [HttpGet("shared/metrics")]
    [HttpGet("/api/shared-crisis-events/metrics")]
    [HttpGet("/api/partner/shared-crisis-events/metrics")]
    [ProducesResponseType(typeof(SharedCrisisMetricsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSharedMetrics(CancellationToken cancellationToken)
    {
        var metrics = await _service.GetSharedMetricsAsync(cancellationToken);
        return Ok(metrics);
    }

    /// <summary>
    /// Submits a partner request for crisis event access.
    /// </summary>
    [HttpPost("access-requests")]
    [HttpPost("/api/shared-crisis-events/access-requests")]
    [ProducesResponseType(typeof(CrisisAccessRequestResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAccessRequest(
        [FromBody] CreateCrisisAccessRequestDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var created = await _service.CreateAccessRequestAsync(dto, cancellationToken);
        _logger.LogInformation("Crisis access request {Id} submitted for event {EventName}", created.Id, created.EventName);
        return CreatedAtAction(nameof(GetAccessRequests), new { id = created.Id }, created);
    }

    /// <summary>
    /// Retrieves crisis access requests.
    /// </summary>
    [HttpGet("access-requests")]
    [HttpGet("/api/shared-crisis-events/access-requests")]
    [ProducesResponseType(typeof(IReadOnlyList<CrisisAccessRequestResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAccessRequests(CancellationToken cancellationToken)
    {
        var requests = await _service.GetAccessRequestsAsync(cancellationToken);
        return Ok(requests);
    }
}
