using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Route("api/partner-dashboard")]
[Route("api/partner/dashboard")]
public class PartnerDashboardController : ControllerBase
{
    private readonly IPartnerDashboardService _service;
    private readonly ILogger<PartnerDashboardController> _logger;

    public PartnerDashboardController(
        IPartnerDashboardService service,
        ILogger<PartnerDashboardController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves the aggregate partner officer dashboard including summary statistics, recent activities, and organisation overview.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PartnerDashboardResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard(
        [FromQuery] string? partnerId,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetDashboardAsync(partnerId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Logs a new real-time activity event into the partner officer activity feed.
    /// </summary>
    [HttpPost("activities")]
    [ProducesResponseType(typeof(PartnerActivityDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LogActivity(
        [FromBody] CreatePartnerActivityDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var created = await _service.LogActivityAsync(dto, cancellationToken);
        _logger.LogInformation("Logged new partner dashboard activity: {Title}", created.Title);
        return CreatedAtAction(nameof(GetDashboard), new { id = created.Id }, created);
    }

    /// <summary>
    /// Resets the partner dashboard to default demonstration data.
    /// </summary>
    [HttpPost("reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Reset(CancellationToken cancellationToken)
    {
        await _service.ResetToDefaultAsync(cancellationToken);
        var data = await _service.GetDashboardAsync(null, cancellationToken);
        return Ok(new { message = "Partner dashboard data reset to defaults.", data });
    }
}
