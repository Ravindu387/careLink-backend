using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Route("api/partner-organisations")]
[Route("api/partners")]
public class PartnerOrganisationsController : ControllerBase
{
    private readonly IPartnerOrganisationService _service;
    private readonly ILogger<PartnerOrganisationsController> _logger;

    public PartnerOrganisationsController(
        IPartnerOrganisationService service,
        ILogger<PartnerOrganisationsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all partner organisations matching optional search and filter criteria.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PartnerOrganisationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PartnerOrganisationDto>>> GetAll(
        [FromQuery] PartnerOrganisationQueryParameters query,
        CancellationToken cancellationToken)
    {
        var list = await _service.GetAllAsync(query, cancellationToken);
        return Ok(list);
    }

    /// <summary>
    /// Retrieves summary metrics for partner organisations (active count, resources, collaboration requests, verification coverage).
    /// </summary>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(PartnerOrganisationsMetricsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PartnerOrganisationsMetricsDto>> GetMetrics(CancellationToken cancellationToken)
    {
        var metrics = await _service.GetMetricsAsync(cancellationToken);
        return Ok(metrics);
    }

    /// <summary>
    /// Retrieves details for a specific partner organisation by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PartnerOrganisationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PartnerOrganisationDto>> GetById(
        string id,
        CancellationToken cancellationToken)
    {
        var org = await _service.GetByIdAsync(id, cancellationToken);
        if (org == null)
        {
            return NotFound(new { error = $"Partner organisation '{id}' not found." });
        }
        return Ok(org);
    }

    /// <summary>
    /// Creates or invites a new partner organisation.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PartnerOrganisationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PartnerOrganisationDto>> Create(
        [FromBody] CreatePartnerOrganisationDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var created = await _service.CreateAsync(dto, cancellationToken);
        _logger.LogInformation("Partner organisation '{Name}' invited with ID {Id}", created.Name, created.Id);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates an existing partner organisation's profile.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(PartnerOrganisationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PartnerOrganisationDto>> Update(
        string id,
        [FromBody] UpdatePartnerOrganisationDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updated = await _service.UpdateAsync(id, dto, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { error = $"Partner organisation '{id}' not found." });
        }

        _logger.LogInformation("Partner organisation {Id} updated", id);
        return Ok(updated);
    }

    /// <summary>
    /// Updates the active/inactive status of a partner organisation.
    /// </summary>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(PartnerOrganisationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PartnerOrganisationDto>> UpdateStatus(
        string id,
        [FromBody] UpdatePartnerStatusDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updated = await _service.UpdateStatusAsync(id, dto, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { error = $"Partner organisation '{id}' not found." });
        }

        _logger.LogInformation("Partner organisation {Id} status updated to {Status}", id, dto.Status);
        return Ok(updated);
    }

    /// <summary>
    /// Updates the verification status (Verified, Pending, Rejected) of a partner organisation.
    /// </summary>
    [HttpPatch("{id}/verification")]
    [ProducesResponseType(typeof(PartnerOrganisationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PartnerOrganisationDto>> UpdateVerification(
        string id,
        [FromBody] UpdatePartnerVerificationDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updated = await _service.UpdateVerificationAsync(id, dto, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { error = $"Partner organisation '{id}' not found." });
        }

        _logger.LogInformation("Partner organisation {Id} verification updated to {Verification}", id, dto.Verification);
        return Ok(updated);
    }

    /// <summary>
    /// Deletes a partner organisation by ID.
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
            return NotFound(new { error = $"Partner organisation '{id}' not found." });
        }

        _logger.LogInformation("Partner organisation {Id} deleted", id);
        return NoContent();
    }

    /// <summary>
    /// Resets partner organisations to default baseline state.
    /// </summary>
    [HttpPost("reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ResetDefaultData(CancellationToken cancellationToken)
    {
        await _service.ResetDefaultDataAsync(cancellationToken);
        _logger.LogInformation("Partner organisations reset to default baseline data.");
        return Ok(new { message = "Partner organisations reset to default baseline data." });
    }
}
