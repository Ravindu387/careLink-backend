using System.Text;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly IReportsService _reportsService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(
        IReportsService reportsService,
        ILogger<ReportsController> logger)
    {
        _reportsService = reportsService;
        _logger = logger;
    }

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
}
