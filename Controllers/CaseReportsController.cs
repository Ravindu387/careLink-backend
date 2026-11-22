using CareLink.Backend.Models;
using CareLink.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.Backend.Controllers;

[ApiController]
[Route("api/casereports")]
public class CaseReportsController : ControllerBase
{
    private readonly ICaseReportsService _reportsService;
    private readonly ILogger<CaseReportsController> _logger;

    public CaseReportsController(ICaseReportsService reportsService, ILogger<CaseReportsController> logger)
    {
        _reportsService = reportsService;
        _logger = logger;
    }

    [HttpGet("analytics")]
    public async Task<ActionResult<ApiResponse<ReportsAnalyticsResponse>>> GetAnalytics(
        [FromQuery] string? dateRange,
        [FromQuery] string? crisisEvent,
        [FromQuery] string? operationalArea)
    {
        try
        {
            var filters = new ReportsFilterDto
            {
                DateRange = string.IsNullOrWhiteSpace(dateRange) ? "18–25 June 2025" : dateRange,
                CrisisEvent = string.IsNullOrWhiteSpace(crisisEvent) ? "Monsoon Floods 2025" : crisisEvent,
                OperationalArea = string.IsNullOrWhiteSpace(operationalArea) ? "All areas" : operationalArea
            };

            var data = await _reportsService.GetReportsAnalyticsAsync(filters);
            return Ok(ApiResponse<ReportsAnalyticsResponse>.Ok(data, "Reports analytics loaded successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load reports analytics");
            return StatusCode(500, ApiResponse<ReportsAnalyticsResponse>.Fail("Error loading reports analytics."));
        }
    }

    [HttpGet("library")]
    public async Task<ActionResult<ApiResponse<List<ReportItem>>>> GetReportLibrary()
    {
        try
        {
            var library = await _reportsService.GetReportLibraryAsync();
            return Ok(ApiResponse<List<ReportItem>>.Ok(library));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve report library");
            return StatusCode(500, ApiResponse<List<ReportItem>>.Fail("Error retrieving report library."));
        }
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportReport(
        [FromQuery] string? dateRange,
        [FromQuery] string? crisisEvent,
        [FromQuery] string? operationalArea)
    {
        try
        {
            var filters = new ReportsFilterDto
            {
                DateRange = string.IsNullOrWhiteSpace(dateRange) ? "18–25 June 2025" : dateRange,
                CrisisEvent = string.IsNullOrWhiteSpace(crisisEvent) ? "Monsoon Floods 2025" : crisisEvent,
                OperationalArea = string.IsNullOrWhiteSpace(operationalArea) ? "All areas" : operationalArea
            };

            var bytes = await _reportsService.ExportReportsCsvAsync(filters);
            var safeArea = filters.OperationalArea.Replace(" ", "-").ToLowerInvariant();
            var fileName = $"carelink-report-{safeArea}-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
            return File(bytes, "text/csv", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export report CSV");
            return StatusCode(500, ApiResponse<string>.Fail("Error exporting report CSV."));
        }
    }
}
