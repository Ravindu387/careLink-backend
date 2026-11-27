using Microsoft.AspNetCore.Authorization;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Authorize]
[Route("api/operation-dashboard")]
[Route("api/operations-dashboard")]
public class OperationDashboardController : ControllerBase
{
    private readonly IOperationDashboardService _service;
    private readonly ILogger<OperationDashboardController> _logger;

    public OperationDashboardController(
        IOperationDashboardService service,
        ILogger<OperationDashboardController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves the consolidated emergency operation dashboard payload
    /// (Active crisis, 8 top KPIs, weekly received vs fulfilled trends, recent operations, upcoming briefings, map points).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(OperationDashboardDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<OperationDashboardDto>> GetDashboard(CancellationToken cancellationToken)
    {
        var data = await _service.GetDashboardAsync(cancellationToken);
        return Ok(data);
    }

    /// <summary>
    /// Retrieves the 8 core operational KPI metric cards.
    /// </summary>
    [HttpGet("kpis")]
    [ProducesResponseType(typeof(IReadOnlyList<DashboardKpiDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<DashboardKpiDto>>> GetKpis(CancellationToken cancellationToken)
    {
        var kpis = await _service.GetKpisAsync(cancellationToken);
        return Ok(kpis);
    }

    /// <summary>
    /// Retrieves weekly requests received vs fulfilled trends for chart telemetry.
    /// </summary>
    [HttpGet("trends")]
    [ProducesResponseType(typeof(IReadOnlyList<WeeklyRequestTrendDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<WeeklyRequestTrendDto>>> GetWeeklyTrends(CancellationToken cancellationToken)
    {
        var trends = await _service.GetWeeklyTrendsAsync(cancellationToken);
        return Ok(trends);
    }

    /// <summary>
    /// Retrieves the latest emergency response activities feed.
    /// </summary>
    [HttpGet("activities")]
    [ProducesResponseType(typeof(IReadOnlyList<RecentActivityDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RecentActivityDto>>> GetActivities(
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var activities = await _service.GetRecentActivitiesAsync(limit, cancellationToken);
        return Ok(activities);
    }

    /// <summary>
    /// Adds a new activity event into the operations dashboard timeline.
    /// </summary>
    [HttpPost("activities")]
    [ProducesResponseType(typeof(RecentActivityDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RecentActivityDto>> AddActivity(
        [FromBody] CreateActivityRequestDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var created = await _service.AddActivityAsync(dto, cancellationToken);
        _logger.LogInformation("Activity '{Title}' logged at {Location}", created.Title, created.Location);
        return CreatedAtAction(nameof(GetActivities), new { id = created.Id }, created);
    }

    /// <summary>
    /// Quickly logs a new emergency incident from the dashboard HUD.
    /// </summary>
    [HttpPost("incident")]
    [ProducesResponseType(typeof(RecentActivityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RecentActivityDto>> LogQuickIncident(
        [FromBody] LogIncidentRequestDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var activity = await _service.LogQuickIncidentAsync(dto, cancellationToken);
        _logger.LogInformation("Quick incident logged: {Title} in {District}", dto.Title, dto.District);
        return Ok(activity);
    }

    /// <summary>
    /// Retrieves the current active crisis summary banner.
    /// </summary>
    [HttpGet("crisis")]
    [ProducesResponseType(typeof(ActiveCrisisBriefingDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ActiveCrisisBriefingDto>> GetActiveCrisis(CancellationToken cancellationToken)
    {
        var crisis = await _service.GetActiveCrisisAsync(cancellationToken);
        return Ok(crisis);
    }

    /// <summary>
    /// Retrieves the list of scheduled operational briefings and meetings.
    /// </summary>
    [HttpGet("briefings")]
    [ProducesResponseType(typeof(IReadOnlyList<UpcomingBriefingDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UpcomingBriefingDto>>> GetUpcomingBriefings(CancellationToken cancellationToken)
    {
        var briefings = await _service.GetUpcomingBriefingsAsync(cancellationToken);
        return Ok(briefings);
    }

    /// <summary>
    /// Resets the operations dashboard to default baseline telemetry.
    /// </summary>
    [HttpPost("reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ResetDefaultData(CancellationToken cancellationToken)
    {
        await _service.ResetDefaultDataAsync(cancellationToken);
        _logger.LogInformation("Operation Dashboard reset to baseline data.");
        return Ok(new { message = "Operation Dashboard reset to baseline data." });
    }
}
