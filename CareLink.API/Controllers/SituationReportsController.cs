using Microsoft.AspNetCore.Authorization;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Authorize]
[Route("api/situation-reports")]
[Route("api/situationreports")]
public class SituationReportsController : ControllerBase
{
    private readonly ISituationReportService _service;
    private readonly ILogger<SituationReportsController> _logger;

    public SituationReportsController(
        ISituationReportService service,
        ILogger<SituationReportsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all situation reports with optional filtering by search text, crisis event, report status, or date range.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SituationReportDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SituationReportDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? @event,
        [FromQuery] string? status,
        [FromQuery] string? dateRange,
        CancellationToken cancellationToken)
    {
        var query = new SituationReportQueryParameters
        {
            Search = search,
            Event = @event,
            Status = status,
            DateRange = dateRange
        };

        var reports = await _service.GetAllAsync(query, cancellationToken);
        return Ok(reports);
    }

    /// <summary>
    /// Retrieves summary metrics for situation reports (published count, draft count, total recipients, next report due, latest approved).
    /// </summary>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(SituationReportMetricsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SituationReportMetricsDto>> GetMetrics(CancellationToken cancellationToken)
    {
        var metrics = await _service.GetMetricsAsync(cancellationToken);
        return Ok(metrics);
    }

    /// <summary>
    /// Retrieves a single situation report by its unique ID (e.g., "SITREP 024").
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SituationReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SituationReportDto>> GetById(
        string id,
        CancellationToken cancellationToken)
    {
        var report = await _service.GetByIdAsync(id, cancellationToken);
        if (report == null)
        {
            return NotFound(new { message = $"Situation report '{id}' not found." });
        }
        return Ok(report);
    }

    /// <summary>
    /// Creates a new operational situation report draft.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SituationReportDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SituationReportDto>> Create(
        [FromBody] CreateSituationReportDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var created = await _service.CreateReportAsync(dto, cancellationToken);
        _logger.LogInformation("Situation report '{Id}' created for event '{Event}'", created.Id, created.Event);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates the approval/publishing status of a situation report (Draft, Approved, Published).
    /// </summary>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(SituationReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SituationReportDto>> UpdateStatus(
        string id,
        [FromBody] UpdateReportStatusDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updated = await _service.UpdateStatusAsync(id, dto, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { message = $"Situation report '{id}' not found." });
        }

        _logger.LogInformation("Situation report '{Id}' status changed to '{Status}' by '{ApprovedBy}'", id, updated.Status, updated.ApprovedBy);
        return Ok(updated);
    }

    /// <summary>
    /// Deletes a situation report.
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
            return NotFound(new { message = $"Situation report '{id}' not found." });
        }

        _logger.LogInformation("Situation report '{Id}' deleted", id);
        return Ok(new { id, message = $"Situation report '{id}' deleted successfully." });
    }

    /// <summary>
    /// Resets all situation reports back to baseline demonstration seed fixtures.
    /// </summary>
    [HttpPost("reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ResetDefaultData(CancellationToken cancellationToken)
    {
        await _service.ResetDefaultDataAsync(cancellationToken);
        _logger.LogInformation("Situation reports reset to baseline data.");
        return Ok(new { message = "Situation reports reset to baseline data." });
    }
}
