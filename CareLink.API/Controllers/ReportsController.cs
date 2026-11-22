using System.Security.Claims;
using System.Text;
using CareLink.Application.Common;
using CareLink.Application.DTOs;
using CareLink.Application.DTOs.Reports;
using CareLink.Application.Interfaces;
using CareLink.Core.Constants;
using CareLink.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Route("api/reports")]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly IReportsService _reportsService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(
        IReportService reportService,
        IReportsService reportsService,
        ILogger<ReportsController> logger)
    {
        _reportService = reportService;
        _reportsService = reportsService;
        _logger = logger;
    }

    #region Resource Manager & Governance Reports (Oshadi)

    /// <summary>
    /// Retrieves all reports with optional filtering (Oshadi).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetReports([FromQuery] ReportFilterParams filters)
    {
        var result = await _reportService.GetReportsAsync(filters);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a single report by integer ID (Oshadi).
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _reportService.GetReportByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Generates a new resource manager report.
    /// </summary>
    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] GenerateReportRequest request)
    {
        var result = await _reportService.GenerateReportAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Deletes a report by integer ID.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _reportService.DeleteReportAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Retrieves report schedules.
    /// </summary>
    [HttpGet("schedules")]
    public async Task<IActionResult> GetSchedules()
    {
        var result = await _reportService.GetSchedulesAsync();
        return Ok(result);
    }

    /// <summary>
    /// Creates a report schedule.
    /// </summary>
    [HttpPost("schedule")]
    public async Task<IActionResult> CreateSchedule([FromBody] CreateReportScheduleRequest request)
    {
        var result = await _reportService.CreateScheduleAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Toggles active status of a report schedule.
    /// </summary>
    [HttpPatch("schedules/{id:int}/toggle")]
    public async Task<IActionResult> ToggleSchedule(int id)
    {
        var result = await _reportService.ToggleScheduleStatusAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Deletes a report schedule.
    /// </summary>
    [HttpDelete("schedules/{id:int}")]
    public async Task<IActionResult> DeleteSchedule(int id)
    {
        var result = await _reportService.DeleteScheduleAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Exports a single report file by ID.
    /// </summary>
    [HttpGet("{id:int}/export")]
    public async Task<IActionResult> ExportSingle(int id, [FromQuery] string format = "csv")
    {
        var file = await _reportService.ExportReportFileAsync(id, format);
        return File(file.Bytes, file.ContentType, file.FileName);
    }

    #endregion

    #region Administrative and Audit Reports (HEAD)

    /// <summary>
    /// Retrieve system report overview metrics, monthly adoption trends, role breakdown, and report catalogue.
    /// </summary>
    [HttpGet("overview")]
    [ProducesResponseType(typeof(ApiResponse<ReportOverviewDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReportOverview([FromQuery] ReportFilterDto filter)
    {
        var result = await _reportService.GetReportOverviewAsync(filter);
        return Ok(result);
    }

    /// <summary>
    /// Retrieve filter options including available date ranges, organisations, and administrative regions.
    /// </summary>
    [HttpGet("filter-options")]
    [ProducesResponseType(typeof(ApiResponse<ReportFilterOptionsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFilterOptions()
    {
        var result = await _reportService.GetFilterOptionsAsync();
        return Ok(result);
    }

    /// <summary>
    /// Retrieve the catalogue of administrative and governance reports available for generation.
    /// </summary>
    [HttpGet("catalog")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ReportCatalogItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCatalog()
    {
        var result = await _reportService.GetCatalogAsync();
        return Ok(result);
    }

    /// <summary>
    /// Generate a detailed report with column schemas, structured preview records, and key performance metrics.
    /// </summary>
    [HttpGet("{reportId}")]
    [ProducesResponseType(typeof(ApiResponse<ReportDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ReportDetailDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GenerateReport(string reportId, [FromQuery] ReportFilterDto filter)
    {
        var actor = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "System Administrator";
        var result = await _reportService.GenerateReportAsync(reportId, filter, actor);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Export an authorized administrative report in CSV or JSON format with immutable audit recording.
    /// </summary>
    [HttpPost("{reportId}/export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportReport(string reportId, [FromBody] ExportReportRequestDto request)
    {
        var actor = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "System Administrator";
        var (bytes, contentType, fileName) = await _reportService.ExportReportAsync(reportId, request, actor);

        Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{fileName}\"");
        Response.Headers.Append("X-Report-Id", reportId);
        return File(bytes, contentType, fileName);
    }

    #endregion

    #region Operational Analytics and Exports

    /// <summary>
    /// Gets aggregated reports analytics and trends according to applied query parameters.
    /// </summary>
    [HttpGet("analytics")]
    public async Task<IActionResult> GetAnalytics(
        [FromQuery] ReportsAnalyticsQueryParameters parameters,
        [FromQuery] string? dateRange,
        [FromQuery] string? metricType,
        [FromQuery] string? comparisonPeriod,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(dateRange) || !string.IsNullOrWhiteSpace(metricType))
        {
            var query = new AnalyticsQueryParams
            {
                DateRange = dateRange,
                MetricType = metricType,
                ComparisonPeriod = comparisonPeriod
            };
            var res = await _reportService.GetAnalyticsDashboardAsync(query);
            return Ok(res);
        }

        var analytics = await _reportsService.GetAnalyticsAsync(parameters, cancellationToken);
        return Ok(analytics);
    }

    /// <summary>
    /// Gets available standard report definitions from the library.
    /// </summary>
    [HttpGet("library")]
    public async Task<ActionResult<IEnumerable<ReportDefinitionDto>>> GetLibrary(CancellationToken cancellationToken)
    {
        var reports = await _reportsService.GetReportDefinitionsAsync(cancellationToken);
        return Ok(reports);
    }

    /// <summary>
    /// Exports reports in CSV format with title or filter parameters.
    /// </summary>
    [HttpGet("export")]
    public async Task<IActionResult> ExportCsv(
        [FromQuery] string? reportTitle,
        [FromQuery] ReportsAnalyticsQueryParameters parameters,
        [FromQuery] ReportFilterParams? filters,
        [FromQuery] string format = "csv",
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(reportTitle))
        {
            var title = reportTitle.Trim();
            var csv = await _reportsService.ExportReportCsvAsync(title, parameters, cancellationToken);
            var safeFileName = title.ToLower().Replace(" ", "-") + ".csv";
            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv)).ToArray();
            return File(bytes, "text/csv; charset=utf-8", safeFileName);
        }

        var file = await _reportService.ExportReportFileAsync(null, format, filters);
        return File(file.Bytes, file.ContentType, file.FileName);
    }

    #endregion
}
