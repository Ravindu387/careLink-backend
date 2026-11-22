using CareLink.Backend.Models;
using CareLink.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.Backend.Controllers;

[ApiController]
[Route("api/shelter/reports")]
[Route("api/shelter-reports")]
public class ShelterReportsController : ControllerBase
{
    private readonly IShelterReportsService _service;
    private readonly ILogger<ShelterReportsController> _logger;

    public ShelterReportsController(
        IShelterReportsService service,
        ILogger<ShelterReportsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("overview")]
    public async Task<ActionResult<ApiResponse<ShelterReportsOverviewDto>>> GetOverview(
        [FromQuery] ShelterReportsFilterDto filter)
    {
        try
        {
            var data = await _service.GetOverviewAsync(filter);
            return Ok(ApiResponse<ShelterReportsOverviewDto>.Ok(data, "Shelter reports overview loaded successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve shelter reports overview");
            return StatusCode(500, ApiResponse<ShelterReportsOverviewDto>.Fail("Error retrieving reports overview."));
        }
    }

    [HttpGet("metrics")]
    public async Task<ActionResult<ApiResponse<ShelterMetricsDto>>> GetMetrics(
        [FromQuery] ShelterReportsFilterDto filter)
    {
        try
        {
            var data = await _service.GetMetricsAsync(filter);
            return Ok(ApiResponse<ShelterMetricsDto>.Ok(data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve shelter metrics");
            return StatusCode(500, ApiResponse<ShelterMetricsDto>.Fail("Error retrieving metrics."));
        }
    }

    [HttpGet("trend")]
    public async Task<ActionResult<ApiResponse<List<ShelterTrendDataPoint>>>> GetTrend(
        [FromQuery] ShelterReportsFilterDto filter)
    {
        try
        {
            var data = await _service.GetTrendDataAsync(filter);
            return Ok(ApiResponse<List<ShelterTrendDataPoint>>.Ok(data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve shelter trend data");
            return StatusCode(500, ApiResponse<List<ShelterTrendDataPoint>>.Fail("Error retrieving trend data."));
        }
    }

    [HttpGet("library")]
    public async Task<ActionResult<ApiResponse<List<ShelterReportItem>>>> GetLibrary()
    {
        try
        {
            var data = await _service.GetReportLibraryAsync();
            return Ok(ApiResponse<List<ShelterReportItem>>.Ok(data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve shelter report library");
            return StatusCode(500, ApiResponse<List<ShelterReportItem>>.Fail("Error retrieving report library."));
        }
    }

    [HttpGet("library/{id:int}")]
    public async Task<ActionResult<ApiResponse<ShelterReportItem>>> GetReportById(int id)
    {
        try
        {
            var report = await _service.GetReportByIdAsync(id);
            if (report == null)
            {
                return NotFound(ApiResponse<ShelterReportItem>.Fail($"Report {id} not found."));
            }
            return Ok(ApiResponse<ShelterReportItem>.Ok(report));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve report {Id}", id);
            return StatusCode(500, ApiResponse<ShelterReportItem>.Fail("Error retrieving report."));
        }
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportOverview(
        [FromQuery] ShelterReportsFilterDto filter)
    {
        try
        {
            var bytes = await _service.ExportOverviewCsvAsync(filter);
            var safeShelter = (filter.Shelter ?? "all-shelters").Replace(" ", "-").ToLowerInvariant();
            var fileName = $"carelink-shelter-report-{safeShelter}-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
            return File(bytes, "text/csv; charset=utf-8", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export shelter reports overview CSV");
            return StatusCode(500, ApiResponse<string>.Fail("Error exporting shelter reports CSV."));
        }
    }

    [HttpGet("export/item")]
    public async Task<IActionResult> ExportReportItem(
        [FromQuery] string reportTitle,
        [FromQuery] ShelterReportsFilterDto filter)
    {
        try
        {
            var bytes = await _service.ExportReportItemCsvAsync(reportTitle, filter);
            var safeTitle = (reportTitle ?? "report").Replace(" ", "-").ToLowerInvariant();
            var fileName = $"carelink-{safeTitle}-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
            return File(bytes, "text/csv; charset=utf-8", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export report item CSV for {Title}", reportTitle);
            return StatusCode(500, ApiResponse<string>.Fail("Error exporting report CSV."));
        }
    }
}
