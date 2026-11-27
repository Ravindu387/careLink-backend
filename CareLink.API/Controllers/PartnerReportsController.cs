using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Authorize]
[Route("api/partner-reports")]
[Route("api/partner/reports")]
public class PartnerReportsController : ControllerBase
{
    private readonly IPartnerReportService _service;
    private readonly ILogger<PartnerReportsController> _logger;

    public PartnerReportsController(
        IPartnerReportService service,
        ILogger<PartnerReportsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all partner reports matching search, tab, type, or status.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PartnerReportDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PartnerReportDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? tab,
        [FromQuery] string? type,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var query = new PartnerReportQueryParameters
        {
            Search = search,
            Tab = tab,
            Type = type,
            Status = status
        };

        var list = await _service.GetAllAsync(query, cancellationToken);
        return Ok(list);
    }

    /// <summary>
    /// Retrieves partner reports summary metrics, weekly reach analytics, operational targets, schedules, and tab counts.
    /// </summary>
    [HttpGet("analytics")]
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(PartnerReportsAnalyticsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PartnerReportsAnalyticsDto>> GetAnalytics(CancellationToken cancellationToken)
    {
        var analytics = await _service.GetAnalyticsAsync(cancellationToken);
        return Ok(analytics);
    }

    /// <summary>
    /// Retrieves a single partner report by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PartnerReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PartnerReportDto>> GetById(string id, CancellationToken cancellationToken)
    {
        var report = await _service.GetByIdAsync(id, cancellationToken);
        if (report == null)
        {
            return NotFound(new { error = $"Report '{id}' not found." });
        }
        return Ok(report);
    }

    /// <summary>
    /// Creates a new partner activity report.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PartnerReportDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PartnerReportDto>> Create(
        [FromBody] CreatePartnerReportDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var created = await _service.CreateAsync(dto, cancellationToken);
        _logger.LogInformation("Partner report '{Title}' created with ID {Id}", created.Title, created.Id);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates details of an existing partner report.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(PartnerReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PartnerReportDto>> Update(
        string id,
        [FromBody] UpdatePartnerReportDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updated = await _service.UpdateAsync(id, dto, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { error = $"Report '{id}' not found." });
        }

        return Ok(updated);
    }

    /// <summary>
    /// Updates status of an existing partner report (e.g. Submitted, Approved, Draft, Archived).
    /// </summary>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(PartnerReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PartnerReportDto>> UpdateStatus(
        string id,
        [FromBody] UpdatePartnerReportStatusDto dto,
        CancellationToken cancellationToken)
    {
        var updated = await _service.UpdateStatusAsync(id, dto.Status, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { error = $"Report '{id}' not found." });
        }

        _logger.LogInformation("Report '{Id}' status updated to {Status}", id, dto.Status);
        return Ok(updated);
    }

    /// <summary>
    /// Deletes a partner report.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound(new { error = $"Report '{id}' not found." });
        }

        return Ok(new { id, message = $"Report '{id}' deleted successfully." });
    }

    /// <summary>
    /// Resets all partner reports back to initial seed data.
    /// </summary>
    [HttpPost("reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Reset(CancellationToken cancellationToken)
    {
        await _service.ResetToDefaultAsync(cancellationToken);
        _logger.LogInformation("Partner reports reset to baseline scenario.");
        return Ok(new { message = "Partner reports reset to default scenario." });
    }
}
