using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Route("api/team-contacts")]
[Route("api/partner/team-contacts")]
[Route("api/partner/team")]
public class TeamContactsController : ControllerBase
{
    private readonly ITeamMemberService _service;
    private readonly ILogger<TeamContactsController> _logger;

    public TeamContactsController(
        ITeamMemberService service,
        ILogger<TeamContactsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all team members matching filters (search, tab, role, sector, location, status).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TeamMemberDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TeamMemberDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? tab,
        [FromQuery] string? role,
        [FromQuery] string? sector,
        [FromQuery] string? location,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var query = new TeamMemberQueryParameters
        {
            Search = search,
            Tab = tab,
            Role = role,
            Sector = sector,
            Location = location,
            Status = status
        };

        var list = await _service.GetAllAsync(query, cancellationToken);
        return Ok(list);
    }

    /// <summary>
    /// Retrieves team contact summary metrics and tab counts.
    /// </summary>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(TeamContactsMetricsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TeamContactsMetricsDto>> GetMetrics(CancellationToken cancellationToken)
    {
        var metrics = await _service.GetMetricsAsync(cancellationToken);
        return Ok(metrics);
    }

    /// <summary>
    /// Retrieves a single team member by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TeamMemberDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TeamMemberDto>> GetById(string id, CancellationToken cancellationToken)
    {
        var item = await _service.GetByIdAsync(id, cancellationToken);
        if (item == null)
        {
            return NotFound(new { error = $"Team member '{id}' not found." });
        }
        return Ok(item);
    }

    /// <summary>
    /// Creates or invites a new team member.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TeamMemberDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TeamMemberDto>> Create(
        [FromBody] CreateTeamMemberDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var created = await _service.CreateAsync(dto, cancellationToken);
        _logger.LogInformation("Invited new team member {Id} ({Name})", created.Id, created.Name);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates an existing team member.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(TeamMemberDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TeamMemberDto>> Update(
        string id,
        [FromBody] UpdateTeamMemberDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updated = await _service.UpdateAsync(id, dto, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { error = $"Team member '{id}' not found." });
        }
        return Ok(updated);
    }

    /// <summary>
    /// Updates the status of a team member (e.g. Active, Needs Review, Completed, Archived).
    /// </summary>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(TeamMemberDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TeamMemberDto>> UpdateStatus(
        string id,
        [FromBody] UpdateTeamMemberStatusDto dto,
        CancellationToken cancellationToken)
    {
        var updated = await _service.UpdateStatusAsync(id, dto, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { error = $"Team member '{id}' not found." });
        }
        return Ok(updated);
    }

    /// <summary>
    /// Deletes or removes a team member record.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        var removed = await _service.DeleteAsync(id, cancellationToken);
        if (!removed)
        {
            return NotFound(new { error = $"Team member '{id}' not found." });
        }
        _logger.LogInformation("Removed team member {Id}", id);
        return NoContent();
    }

    /// <summary>
    /// Resets the team members store back to seed defaults.
    /// </summary>
    [HttpPost("reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Reset(CancellationToken cancellationToken)
    {
        await _service.ResetToDefaultsAsync(cancellationToken);
        _logger.LogInformation("Reset team members to default seeds");
        return Ok(new { message = "Team members successfully reset to defaults." });
    }
}
