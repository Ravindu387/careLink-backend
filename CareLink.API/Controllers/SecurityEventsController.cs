using System.Security.Claims;
using CareLink.Application.Common;
using CareLink.Application.DTOs.SecurityEvents;
using CareLink.Application.DTOs.Users;
using CareLink.Application.Interfaces;
using CareLink.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Route("api/security-events")]
[Authorize(Roles = $"{UserRoles.Admin},{UserRoles.SystemAdmin},{UserRoles.EmergencyCoordinator},{UserRoles.CaseManager},{UserRoles.IncidentManager}")]
public class SecurityEventsController : ControllerBase
{
    private readonly ISecurityEventService _securityEventService;

    public SecurityEventsController(ISecurityEventService securityEventService)
    {
        _securityEventService = securityEventService;
    }

    /// <summary>
    /// Retrieve paginated security events with optional search, severity, type, status, and date range filters.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedListDto<SecurityEventDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEvents([FromQuery] SecurityEventQueryDto query)
    {
        var result = await _securityEventService.GetEventsAsync(query);
        return Ok(result);
    }

    /// <summary>
    /// Retrieve security event summary statistics and live threat level metrics.
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(ApiResponse<SecurityEventStatsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats()
    {
        var result = await _securityEventService.GetStatsAsync();
        return Ok(result);
    }

    /// <summary>
    /// Retrieve distinct event types for filtering and classification.
    /// </summary>
    [HttpGet("types")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<string>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEventTypes()
    {
        var result = await _securityEventService.GetEventTypesAsync();
        return Ok(result);
    }

    /// <summary>
    /// Retrieve details of a specific security event by its identifier.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<SecurityEventDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SecurityEventDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _securityEventService.GetEventByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Manually record a security event.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SecurityEventDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<SecurityEventDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateEvent([FromBody] CreateSecurityEventRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<SecurityEventDto>.FailureResult("Validation failed.", errors));
        }

        var actor = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "System Admin";
        var result = await _securityEventService.CreateEventAsync(request, actor);

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Update the investigation/resolution status of a security event.
    /// </summary>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(ApiResponse<SecurityEventDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SecurityEventDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateSecurityEventStatusRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<SecurityEventDto>.FailureResult("Validation failed.", errors));
        }

        var actor = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "System Admin";
        var result = await _securityEventService.UpdateEventStatusAsync(id, request, actor);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Record an authorized security policy enforcement action with administrative rationale.
    /// </summary>
    [HttpPost("policy-actions")]
    [ProducesResponseType(typeof(ApiResponse<SecurityPolicyActionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SecurityPolicyActionDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RecordPolicyAction([FromBody] RecordSecurityPolicyActionRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<SecurityPolicyActionDto>.FailureResult("Validation failed.", errors));
        }

        var actor = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "System Administrator";
        var result = await _securityEventService.RecordPolicyActionAsync(request, actor);

        return Ok(result);
    }

    /// <summary>
    /// Export filtered security events as a CSV file.
    /// </summary>
    [HttpGet("export-csv")]
    [Produces("text/csv")]
    public async Task<IActionResult> ExportCsv([FromQuery] SecurityEventQueryDto query)
    {
        var bytes = await _securityEventService.ExportEventsCsvAsync(query);
        var fileName = $"security-events-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
        return File(bytes, "text/csv", fileName);
    }
}
