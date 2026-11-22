using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Route("api/health-safety")]
public class HealthSafetyController : ControllerBase
{
    private readonly IHealthSafetyService _service;
    private readonly ILogger<HealthSafetyController> _logger;

    public HealthSafetyController(
        IHealthSafetyService service,
        ILogger<HealthSafetyController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Gets health and safety incidents with optional filtering.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<HealthSafetyIncidentDto>>> GetAll(
        [FromQuery] HealthSafetyIncidentQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var results = await _service.GetIncidentsAsync(parameters, cancellationToken);
        return Ok(results);
    }

    /// <summary>
    /// Gets summary counts for health and safety incidents.
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult<HealthSafetySummaryDto>> GetSummary(CancellationToken cancellationToken)
    {
        var summary = await _service.GetSummaryAsync(cancellationToken);
        return Ok(summary);
    }

    /// <summary>
    /// Gets metadata (incident types, staff members, severities, statuses).
    /// </summary>
    [HttpGet("meta")]
    public async Task<ActionResult<HealthSafetyMetaDto>> GetMeta(CancellationToken cancellationToken)
    {
        var meta = await _service.GetMetaAsync(cancellationToken);
        return Ok(meta);
    }

    /// <summary>
    /// Gets safety checks.
    /// </summary>
    [HttpGet("safety-checks")]
    public async Task<ActionResult<IEnumerable<SafetyCheckDto>>> GetSafetyChecks(CancellationToken cancellationToken)
    {
        var checks = await _service.GetSafetyChecksAsync(cancellationToken);
        return Ok(checks);
    }

    /// <summary>
    /// Gets an incident by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<HealthSafetyIncidentDto>> GetById(
        string id,
        CancellationToken cancellationToken)
    {
        var item = await _service.GetIncidentByIdAsync(id, cancellationToken);
        if (item == null)
        {
            return NotFound(new { message = $"Incident with ID '{id}' was not found." });
        }
        return Ok(item);
    }

    /// <summary>
    /// Creates a new health and safety incident.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<HealthSafetyIncidentDto>> Create(
        [FromBody] CreateHealthSafetyIncidentDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var created = await _service.CreateIncidentAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating health safety incident");
            return StatusCode(500, new { message = "An error occurred while creating the incident." });
        }
    }

    /// <summary>
    /// Updates an existing health and safety incident.
    /// </summary>
    [HttpPatch("{id}")]
    [HttpPut("{id}")]
    public async Task<ActionResult<HealthSafetyIncidentDto>> Update(
        string id,
        [FromBody] UpdateHealthSafetyIncidentDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var updated = await _service.UpdateIncidentAsync(id, dto, cancellationToken);
            if (updated == null)
            {
                return NotFound(new { message = $"Incident with ID '{id}' was not found." });
            }

            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating incident {Id}", id);
            return StatusCode(500, new { message = "An error occurred while updating the incident." });
        }
    }

    /// <summary>
    /// Deletes an incident by ID.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        var success = await _service.DeleteIncidentAsync(id, cancellationToken);
        if (!success)
        {
            return NotFound(new { message = $"Incident with ID '{id}' was not found." });
        }

        return NoContent();
    }
}
