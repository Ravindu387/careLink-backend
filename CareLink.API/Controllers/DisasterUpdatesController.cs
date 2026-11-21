using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Route("api/disaster-updates")]
public class DisasterUpdatesController : ControllerBase
{
    private readonly IDisasterUpdateService _service;
    private readonly ILogger<DisasterUpdatesController> _logger;

    public DisasterUpdatesController(
        IDisasterUpdateService service,
        ILogger<DisasterUpdatesController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Gets disaster updates with optional filtering by type, search, archived, and drafts.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DisasterUpdateDto>>> GetAll(
        [FromQuery] DisasterUpdateQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var results = await _service.GetUpdatesAsync(parameters, cancellationToken);
        return Ok(results);
    }

    /// <summary>
    /// Gets aggregated summary counts and the latest critical alert.
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult<DisasterUpdateSummaryDto>> GetSummary(CancellationToken cancellationToken)
    {
        var summary = await _service.GetSummaryAsync(cancellationToken);
        return Ok(summary);
    }

    /// <summary>
    /// Gets verified emergency contacts.
    /// </summary>
    [HttpGet("emergency-contacts")]
    public async Task<ActionResult<IEnumerable<EmergencyContactDto>>> GetEmergencyContacts(CancellationToken cancellationToken)
    {
        var contacts = await _service.GetEmergencyContactsAsync(cancellationToken);
        return Ok(contacts);
    }

    /// <summary>
    /// Gets a single disaster update by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<DisasterUpdateDto>> GetById(string id, CancellationToken cancellationToken)
    {
        var item = await _service.GetByIdAsync(id, cancellationToken);
        if (item == null)
        {
            return NotFound(new { message = $"Disaster update with ID '{id}' was not found." });
        }
        return Ok(item);
    }

    /// <summary>
    /// Creates a new disaster update or saves a draft.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<DisasterUpdateDto>> Create(
        [FromBody] CreateDisasterUpdateDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var created = await _service.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating disaster update.");
            return StatusCode(500, new { message = "An error occurred while creating the disaster update." });
        }
    }

    /// <summary>
    /// Toggles the archive status of a disaster update.
    /// </summary>
    [HttpPatch("{id}/archive")]
    public async Task<ActionResult<DisasterUpdateDto>> ToggleArchive(string id, CancellationToken cancellationToken)
    {
        var updated = await _service.ToggleArchiveAsync(id, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { message = $"Disaster update with ID '{id}' was not found." });
        }
        return Ok(updated);
    }

    /// <summary>
    /// Publishes a saved draft announcement.
    /// </summary>
    [HttpPost("{id}/publish")]
    public async Task<ActionResult<DisasterUpdateDto>> PublishDraft(string id, CancellationToken cancellationToken)
    {
        var published = await _service.PublishDraftAsync(id, cancellationToken);
        if (published == null)
        {
            return NotFound(new { message = $"Draft with ID '{id}' was not found or is already published." });
        }
        return Ok(published);
    }

    /// <summary>
    /// Deletes a disaster update or draft.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound(new { message = $"Disaster update with ID '{id}' was not found." });
        }
        return NoContent();
    }
}
