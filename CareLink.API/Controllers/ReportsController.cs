using System.Security.Claims;
using CareLink.Application.Common;
using CareLink.Application.DTOs.Reports;
using CareLink.Application.Interfaces;
using CareLink.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize(Roles = $"{UserRoles.Admin},{UserRoles.SystemAdmin},{UserRoles.EmergencyCoordinator},{UserRoles.IncidentManager}")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

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
}
