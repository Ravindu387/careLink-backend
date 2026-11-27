using CareLink.Application.Common;
using CareLink.Application.DTOs.Dashboard;
using CareLink.Application.Interfaces;
using CareLink.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Roles = $"{UserRoles.Admin},{UserRoles.SystemAdmin},{UserRoles.EmergencyCoordinator},{UserRoles.CaseManager},{UserRoles.IncidentManager},{UserRoles.PartnerOfficer},{UserRoles.VolunteerCoordinator}")]
public class AdminDashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public AdminDashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Retrieve aggregated dashboard dataset (KPI summaries, weekly trends, urgent requests, and live activity).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<DashboardDataDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboardData()
    {
        var result = await _dashboardService.GetDashboardDataAsync();
        return Ok(result);
    }

    /// <summary>
    /// Retrieve overview KPI summary metrics for quick dashboard header refresh.
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(ApiResponse<DashboardSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary()
    {
        var result = await _dashboardService.GetSummaryAsync();
        return Ok(result);
    }

    /// <summary>
    /// Retrieve 7-day request receipt vs fulfillment chart points.
    /// </summary>
    [HttpGet("trend")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<DashboardChartPointDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWeeklyTrend()
    {
        var result = await _dashboardService.GetWeeklyTrendAsync();
        return Ok(result);
    }

    /// <summary>
    /// Retrieve live system event and audit stream for dashboard activity ticker.
    /// </summary>
    [HttpGet("activity")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<DashboardActivityItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecentActivities()
    {
        var result = await _dashboardService.GetRecentActivitiesAsync();
        return Ok(result);
    }

    /// <summary>
    /// Retrieve active operational and security alerts.
    /// </summary>
    [HttpGet("alerts")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<DashboardAlertItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveAlerts()
    {
        var result = await _dashboardService.GetActiveAlertsAsync();
        return Ok(result);
    }
}
