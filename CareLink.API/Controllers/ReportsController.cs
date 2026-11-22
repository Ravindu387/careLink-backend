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

    #region Operational and Shelter Reports (Prabhani)

    /// <summary>
    /// Gets aggregated reports analytics, trends, and metrics according to applied filters.
    /// </summary>
    [HttpGet("analytics")]
    public async Task<ActionResult<ReportsAnalyticsDto>> GetAnalytics(
        [FromQuery] ReportsAnalyticsQueryParameters parameters,
        CancellationToken cancellationToken)
    {
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
    /// Exports report data in CSV format based on the selected report title and filter parameters.
    /// </summary>
    [HttpGet("export")]
    public async Task<IActionResult> ExportCsv(
        [FromQuery] string reportTitle,
        [FromQuery] ReportsAnalyticsQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var title = string.IsNullOrWhiteSpace(reportTitle) ? "Operational Report" : reportTitle.Trim();
        var csv = await _reportsService.ExportReportCsvAsync(title, parameters, cancellationToken);

        var safeFileName = title.ToLower().Replace(" ", "-") + ".csv";
        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv)).ToArray();

        return File(bytes, "text/csv; charset=utf-8", safeFileName);
    }

    #endregion
}
