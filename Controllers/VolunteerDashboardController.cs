using Microsoft.AspNetCore.Authorization;
using CareLink.Backend.Models;
using CareLink.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.Backend.Controllers;

[ApiController]
[Authorize]
[Route("api/volunteer/dashboard")]
[Route("api/volunteer-dashboard")]
public class VolunteerDashboardController : ControllerBase
{
    private readonly IVolunteerDashboardService _service;
    private readonly ILogger<VolunteerDashboardController> _logger;

    public VolunteerDashboardController(
        IVolunteerDashboardService service,
        ILogger<VolunteerDashboardController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    [HttpGet("summary")]
    public async Task<ActionResult<ApiResponse<VolunteerDashboardOverviewDto>>> GetDashboardOverview()
    {
        try
        {
            var overview = await _service.GetOverviewAsync();
            return Ok(ApiResponse<VolunteerDashboardOverviewDto>.Ok(overview, "Volunteer coordination dashboard summary retrieved."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get volunteer dashboard overview");
            return StatusCode(500, ApiResponse<VolunteerDashboardOverviewDto>.Fail("Error retrieving dashboard summary."));
        }
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<List<DashboardStatItemDto>>>> GetStats()
    {
        try
        {
            var stats = await _service.GetStatsAsync();
            return Ok(ApiResponse<List<DashboardStatItemDto>>.Ok(stats));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get dashboard stats");
            return StatusCode(500, ApiResponse<List<DashboardStatItemDto>>.Fail("Error retrieving dashboard stats."));
        }
    }

    [HttpGet("urgent-tasks")]
    public async Task<ActionResult<ApiResponse<List<DashboardUrgentTaskDto>>>> GetUrgentTasks()
    {
        try
        {
            var tasks = await _service.GetUrgentTasksAsync();
            return Ok(ApiResponse<List<DashboardUrgentTaskDto>>.Ok(tasks));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get dashboard urgent tasks");
            return StatusCode(500, ApiResponse<List<DashboardUrgentTaskDto>>.Fail("Error retrieving urgent tasks."));
        }
    }

    [HttpGet("activities")]
    public async Task<ActionResult<ApiResponse<List<DashboardActivityItemDto>>>> GetActivities()
    {
        try
        {
            var activities = await _service.GetActivitiesAsync();
            return Ok(ApiResponse<List<DashboardActivityItemDto>>.Ok(activities));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get dashboard activities");
            return StatusCode(500, ApiResponse<List<DashboardActivityItemDto>>.Fail("Error retrieving activities."));
        }
    }

    [HttpPost("activities")]
    public async Task<ActionResult<ApiResponse<DashboardActivityItemDto>>> AddActivity(
        [FromBody] CreateDashboardActivityRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return BadRequest(ApiResponse<DashboardActivityItemDto>.Fail("Activity title is required."));
            }

            var created = await _service.AddActivityAsync(request);
            return Ok(ApiResponse<DashboardActivityItemDto>.Ok(created, "Activity logged successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log dashboard activity");
            return StatusCode(500, ApiResponse<DashboardActivityItemDto>.Fail("Error logging activity."));
        }
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportSummary()
    {
        try
        {
            var bytes = await _service.ExportDashboardSummaryAsync();
            var fileName = $"volunteer-dashboard-summary-{DateTime.UtcNow:yyyyMMddHHmmss}.txt";
            return File(bytes, "text/plain; charset=utf-8", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export dashboard summary");
            return StatusCode(500, ApiResponse<string>.Fail("Error exporting dashboard summary."));
        }
    }
}
