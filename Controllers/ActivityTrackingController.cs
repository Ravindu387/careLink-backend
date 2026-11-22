using CareLink.Backend.Models;
using CareLink.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.Backend.Controllers;

[ApiController]
[Route("api/activity-tracking")]
public class ActivityTrackingController : ControllerBase
{
    private readonly IActivityTrackingService _activityTrackingService;
    private readonly ILogger<ActivityTrackingController> _logger;

    public ActivityTrackingController(IActivityTrackingService activityTrackingService, ILogger<ActivityTrackingController> logger)
    {
        _activityTrackingService = activityTrackingService;
        _logger = logger;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<ActivityTrackingStatsDto>>> GetStats()
    {
        try
        {
            var stats = await _activityTrackingService.GetStatsAsync();
            return Ok(ApiResponse<ActivityTrackingStatsDto>.Ok(stats, "Activity tracking statistics retrieved successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get activity tracking stats");
            return StatusCode(500, ApiResponse<ActivityTrackingStatsDto>.Fail("Error retrieving activity tracking stats."));
        }
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResult<ActivityRecord>>>> GetActivities([FromQuery] ActivityTrackingQueryDto query)
    {
        try
        {
            var paged = await _activityTrackingService.GetActivitiesAsync(query);
            return Ok(ApiResponse<PaginatedResult<ActivityRecord>>.Ok(paged));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get activity records");
            return StatusCode(500, ApiResponse<PaginatedResult<ActivityRecord>>.Fail("Error retrieving activity records."));
        }
    }

    [HttpGet("locations")]
    public async Task<ActionResult<ApiResponse<List<string>>>> GetLocations()
    {
        try
        {
            var locations = await _activityTrackingService.GetLocationsAsync();
            return Ok(ApiResponse<List<string>>.Ok(locations));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get activity locations");
            return StatusCode(500, ApiResponse<List<string>>.Fail("Error retrieving locations."));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ActivityRecord>>> GetById(string id)
    {
        try
        {
            var item = await _activityTrackingService.GetByIdAsync(id);
            if (item == null)
            {
                return NotFound(ApiResponse<ActivityRecord>.Fail($"Activity record {id} not found."));
            }
            return Ok(ApiResponse<ActivityRecord>.Ok(item));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get activity record {Id}", id);
            return StatusCode(500, ApiResponse<ActivityRecord>.Fail("Error retrieving activity record."));
        }
    }

    [HttpPost("check-in")]
    public async Task<ActionResult<ApiResponse<ActivityRecord>>> ManualCheckIn([FromBody] ManualCheckInRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Volunteer) || string.IsNullOrWhiteSpace(request.Task))
            {
                return BadRequest(ApiResponse<ActivityRecord>.Fail("Volunteer name and Task ID are required."));
            }

            var record = await _activityTrackingService.ManualCheckInAsync(request);
            return Ok(ApiResponse<ActivityRecord>.Ok(record, "Volunteer checked in successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record manual check-in");
            return StatusCode(500, ApiResponse<ActivityRecord>.Fail("Error recording check-in."));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ActivityRecord>>> RecordActivity([FromBody] RecordActivityRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Volunteer) || string.IsNullOrWhiteSpace(request.Task))
            {
                return BadRequest(ApiResponse<ActivityRecord>.Fail("Volunteer name and Task ID are required."));
            }

            var record = await _activityTrackingService.RecordActivityAsync(request);
            return Ok(ApiResponse<ActivityRecord>.Ok(record, "Activity recorded successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record activity");
            return StatusCode(500, ApiResponse<ActivityRecord>.Fail("Error recording activity."));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<ActivityRecord>>> Update(string id, [FromBody] UpdateActivityRequest request)
    {
        try
        {
            var updated = await _activityTrackingService.UpdateActivityAsync(id, request);
            if (updated == null)
            {
                return NotFound(ApiResponse<ActivityRecord>.Fail($"Activity record {id} not found."));
            }
            return Ok(ApiResponse<ActivityRecord>.Ok(updated, "Activity record updated successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update activity record {Id}", id);
            return StatusCode(500, ApiResponse<ActivityRecord>.Fail("Error updating activity record."));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(string id)
    {
        try
        {
            var deleted = await _activityTrackingService.DeleteActivityAsync(id);
            if (!deleted)
            {
                return NotFound(ApiResponse<bool>.Fail($"Activity record {id} not found."));
            }
            return Ok(ApiResponse<bool>.Ok(true, "Activity record deleted successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete activity record {Id}", id);
            return StatusCode(500, ApiResponse<bool>.Fail("Error deleting activity record."));
        }
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] ActivityTrackingQueryDto query)
    {
        try
        {
            var bytes = await _activityTrackingService.ExportCsvAsync(query);
            var fileName = $"carelink-activity-tracking-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
            return File(bytes, "text/csv", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export activity tracking CSV");
            return StatusCode(500, ApiResponse<string>.Fail("Error exporting activity tracking data."));
        }
    }
}
