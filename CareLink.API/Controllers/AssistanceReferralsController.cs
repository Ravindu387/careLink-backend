using Microsoft.AspNetCore.Authorization;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Authorize]
[Route("api/assistance-referrals")]
[Route("api/partner/assistance-referrals")]
[Route("api/referrals")]
public class AssistanceReferralsController : ControllerBase
{
    private readonly IAssistanceReferralService _service;
    private readonly ILogger<AssistanceReferralsController> _logger;

    public AssistanceReferralsController(
        IAssistanceReferralService service,
        ILogger<AssistanceReferralsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all assistance referrals matching filters.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AssistanceReferralDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AssistanceReferralDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? tab,
        [FromQuery] string? crisis,
        [FromQuery] string? priority,
        [FromQuery] string? sector,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var query = new AssistanceReferralQueryParameters
        {
            Search = search,
            Tab = tab,
            Crisis = crisis,
            Priority = priority,
            Sector = sector,
            Status = status
        };

        var list = await _service.GetAllAsync(query, cancellationToken);
        return Ok(list);
    }

    /// <summary>
    /// Retrieves aggregated metrics for assistance referrals.
    /// </summary>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(AssistanceReferralMetricsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AssistanceReferralMetricsDto>> GetMetrics(CancellationToken cancellationToken)
    {
        var metrics = await _service.GetMetricsAsync(cancellationToken);
        return Ok(metrics);
    }

    /// <summary>
    /// Retrieves a single assistance referral by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AssistanceReferralDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssistanceReferralDto>> GetById(
        string id,
        CancellationToken cancellationToken)
    {
        var item = await _service.GetByIdAsync(id, cancellationToken);
        if (item == null)
        {
            return NotFound(new { error = $"Assistance referral '{id}' not found." });
        }
        return Ok(item);
    }

    /// <summary>
    /// Creates a new assistance referral.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AssistanceReferralDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AssistanceReferralDto>> Create(
        [FromBody] CreateAssistanceReferralDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var created = await _service.CreateAsync(dto, cancellationToken);
        _logger.LogInformation("Created assistance referral {Id} for sector {Sector}", created.Id, created.Sector);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates an assistance referral.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(AssistanceReferralDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssistanceReferralDto>> Update(
        string id,
        [FromBody] UpdateAssistanceReferralDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updated = await _service.UpdateAsync(id, dto, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { error = $"Assistance referral '{id}' not found." });
        }
        return Ok(updated);
    }

    /// <summary>
    /// Updates the status of an assistance referral.
    /// </summary>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(AssistanceReferralDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssistanceReferralDto>> UpdateStatus(
        string id,
        [FromBody] UpdateReferralStatusDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updated = await _service.UpdateStatusAsync(id, dto, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { error = $"Assistance referral '{id}' not found." });
        }
        _logger.LogInformation("Updated referral {Id} status to {Status}", id, dto.Status);
        return Ok(updated);
    }

    /// <summary>
    /// Assigns a response team to an assistance referral.
    /// </summary>
    [HttpPatch("{id}/assign")]
    [ProducesResponseType(typeof(AssistanceReferralDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssistanceReferralDto>> AssignTeam(
        string id,
        [FromBody] AssignReferralTeamDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updated = await _service.AssignTeamAsync(id, dto, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { error = $"Assistance referral '{id}' not found." });
        }
        _logger.LogInformation("Assigned referral {Id} to team {Assigned}", id, dto.Assigned);
        return Ok(updated);
    }

    /// <summary>
    /// Deletes an assistance referral.
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
            return NotFound(new { error = $"Assistance referral '{id}' not found." });
        }
        _logger.LogInformation("Deleted assistance referral {Id}", id);
        return NoContent();
    }

    /// <summary>
    /// Resets the referrals store to default seeds.
    /// </summary>
    [HttpPost("reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Reset(CancellationToken cancellationToken)
    {
        await _service.ResetToDefaultAsync(cancellationToken);
        _logger.LogInformation("Reset assistance referrals to default seeds");
        return Ok(new { message = "Assistance referrals reset to defaults." });
    }
}
