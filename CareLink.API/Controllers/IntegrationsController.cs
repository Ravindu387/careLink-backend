using CareLink.Application.Common;
using CareLink.Application.DTOs.Integrations;
using CareLink.Application.Interfaces;
using CareLink.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{UserRoles.Admin},{UserRoles.SystemAdmin},{UserRoles.EmergencyCoordinator},{UserRoles.CaseManager},{UserRoles.IncidentManager},{UserRoles.VolunteerCoordinator}")]
public class IntegrationsController : ControllerBase
{
    private readonly IIntegrationHealthService _integrationService;

    public IntegrationsController(IIntegrationHealthService integrationService)
    {
        _integrationService = integrationService;
    }

    /// <summary>
    /// Retrieve all registered system integrations with health status and telemetry.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<IntegrationHealthDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetIntegrations(
        [FromQuery] string? type,
        [FromQuery] string? health,
        [FromQuery] string? search)
    {
        var result = await _integrationService.GetIntegrationsAsync(type, health, search);
        return Ok(result);
    }

    /// <summary>
    /// Retrieve overall platform health metrics and availability summary.
    /// </summary>
    [HttpGet("overview")]
    [ProducesResponseType(typeof(ApiResponse<PlatformHealthOverviewDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOverview()
    {
        var result = await _integrationService.GetOverviewAsync();
        return Ok(result);
    }

    /// <summary>
    /// Retrieve details of a specific integration by its identifier.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<IntegrationHealthDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IntegrationHealthDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _integrationService.GetIntegrationByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Retrieve the audit and event logs for a specific integration.
    /// </summary>
    [HttpGet("{id}/logs")]
    [ProducesResponseType(typeof(ApiResponse<List<IntegrationLogDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLogs(string id)
    {
        var result = await _integrationService.GetIntegrationLogsAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Trigger an automated live health check across all integration endpoints.
    /// </summary>
    [HttpPost("health-check")]
    [ProducesResponseType(typeof(ApiResponse<List<IntegrationHealthDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RunHealthChecks()
    {
        var result = await _integrationService.RunHealthChecksAsync();
        return Ok(result);
    }

    /// <summary>
    /// Initiate a manual recovery and retry action on an integration.
    /// </summary>
    [HttpPost("{id}/retry")]
    [ProducesResponseType(typeof(ApiResponse<IntegrationHealthDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IntegrationHealthDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<IntegrationHealthDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RetryIntegration(string id, [FromBody] IntegrationActionRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return BadRequest(ApiResponse<IntegrationHealthDto>.FailureResult("Validation failed.", errors));
        }

        var result = await _integrationService.RetryIntegrationAsync(id, request);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Record an authorized administrative action on integrations.
    /// </summary>
    [HttpPost("record-action")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RecordAction(
        [FromQuery] string? integrationId,
        [FromBody] IntegrationActionRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return BadRequest(ApiResponse<bool>.FailureResult("Validation failed.", errors));
        }

        var result = await _integrationService.RecordActionAsync(integrationId, request);
        return Ok(result);
    }
}
