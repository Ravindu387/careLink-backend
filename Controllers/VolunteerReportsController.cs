using CareLink.Backend.Models;
using CareLink.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.Backend.Controllers;

[ApiController]
[Route("api/volunteer/reports")]
[Route("api/volunteer-reports")]
public class VolunteerReportsController : ControllerBase
{
    private readonly IVolunteerReportsService _service;
    private readonly ILogger<VolunteerReportsController> _logger;

    public VolunteerReportsController(
        IVolunteerReportsService service,
        ILogger<VolunteerReportsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("overview")]
    public async Task<ActionResult<ApiResponse<VolunteerReportsOverviewDto>>> GetOverview(
        [FromQuery] VolunteerReportsQueryDto query)
    {
        try
        {
            var overview = await _service.GetOverviewAsync(query);
            return Ok(ApiResponse<VolunteerReportsOverviewDto>.Ok(overview, "Volunteer reports overview retrieved."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get volunteer reports overview");
            return StatusCode(500, ApiResponse<VolunteerReportsOverviewDto>.Fail("Error retrieving reports overview."));
        }
    }

    [HttpGet("analytics")]
    public async Task<ActionResult<ApiResponse<VolunteerAnalyticsSummaryDto>>> GetAnalytics(
        [FromQuery] VolunteerReportsQueryDto query)
    {
        try
        {
            var analytics = await _service.GetAnalyticsAsync(query);
            return Ok(ApiResponse<VolunteerAnalyticsSummaryDto>.Ok(analytics));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get volunteer analytics summary");
            return StatusCode(500, ApiResponse<VolunteerAnalyticsSummaryDto>.Fail("Error retrieving analytics summary."));
        }
    }

    [HttpGet("trend")]
    public async Task<ActionResult<ApiResponse<List<TrendDataPoint>>>> GetTrend(
        [FromQuery] VolunteerReportsQueryDto query)
    {
        try
        {
            var trend = await _service.GetTrendDataAsync(query);
            return Ok(ApiResponse<List<TrendDataPoint>>.Ok(trend));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get trend data");
            return StatusCode(500, ApiResponse<List<TrendDataPoint>>.Fail("Error retrieving trend data."));
        }
    }

    [HttpGet("skills")]
    public async Task<ActionResult<ApiResponse<List<SkillHoursBreakdown>>>> GetSkills(
        [FromQuery] VolunteerReportsQueryDto query)
    {
        try
        {
            var skills = await _service.GetSkillBreakdownAsync(query);
            return Ok(ApiResponse<List<SkillHoursBreakdown>>.Ok(skills));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get skills breakdown");
            return StatusCode(500, ApiResponse<List<SkillHoursBreakdown>>.Fail("Error retrieving skills breakdown."));
        }
    }

    [HttpGet("library")]
    public async Task<ActionResult<ApiResponse<List<VolunteerReportItem>>>> GetReportLibrary()
    {
        try
        {
            var library = await _service.GetReportLibraryAsync();
            return Ok(ApiResponse<List<VolunteerReportItem>>.Ok(library));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get report library");
            return StatusCode(500, ApiResponse<List<VolunteerReportItem>>.Fail("Error retrieving report library."));
        }
    }

    [HttpGet("library/{id:int}")]
    public async Task<ActionResult<ApiResponse<VolunteerReportItem>>> GetReportById(int id)
    {
        try
        {
            var report = await _service.GetReportByIdAsync(id);
            if (report == null)
            {
                return NotFound(ApiResponse<VolunteerReportItem>.Fail($"Report {id} not found."));
            }
            return Ok(ApiResponse<VolunteerReportItem>.Ok(report));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get report {Id}", id);
            return StatusCode(500, ApiResponse<VolunteerReportItem>.Fail("Error retrieving report."));
        }
    }

    [HttpGet("library/{id:int}/export")]
    public async Task<IActionResult> ExportReportItem(
        int id,
        [FromQuery] VolunteerReportsQueryDto query)
    {
        try
        {
            var bytes = await _service.ExportReportItemCsvAsync(id, query);
            var fileName = $"carelink-report-{id}-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
            return File(bytes, "text/csv", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export report {Id}", id);
            return StatusCode(500, ApiResponse<string>.Fail("Error exporting report CSV."));
        }
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportOverview(
        [FromQuery] VolunteerReportsQueryDto query)
    {
        try
        {
            var bytes = await _service.ExportOverviewCsvAsync(query);
            var fileName = $"carelink-reports-analytics-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
            return File(bytes, "text/csv", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export reports analytics CSV");
            return StatusCode(500, ApiResponse<string>.Fail("Error exporting reports CSV."));
        }
    }
}
