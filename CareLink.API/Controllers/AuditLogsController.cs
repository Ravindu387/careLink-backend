using System.Security.Claims;
using CareLink.Application.Common;
using CareLink.Application.DTOs.AuditLogs;
using CareLink.Application.DTOs.Users;
using CareLink.Application.Interfaces;
using CareLink.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Route("api/audit-logs")]
[Authorize(Roles = $"{UserRoles.Admin},{UserRoles.SystemAdmin},{UserRoles.EmergencyCoordinator},{UserRoles.IncidentManager}")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogsController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    /// <summary>
    /// Retrieve paginated immutable audit logs with optional search, action, actor, and record type filters.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedListDto<AuditLogDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditLogs([FromQuery] AuditLogQueryDto query)
    {
        var result = await _auditLogService.GetAuditLogsAsync(query);
        return Ok(result);
    }

    /// <summary>
    /// Retrieve audit log summary statistics, retention status, and distinct filter taxonomies.
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(ApiResponse<AuditLogStatsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats()
    {
        var result = await _auditLogService.GetStatsAsync();
        return Ok(result);
    }

    /// <summary>
    /// Retrieve distinct action types recorded in the audit trail.
    /// </summary>
    [HttpGet("actions")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<string>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActionTypes()
    {
        var result = await _auditLogService.GetActionTypesAsync();
        return Ok(result);
    }

    /// <summary>
    /// Retrieve distinct record types recorded in the audit trail.
    /// </summary>
    [HttpGet("record-types")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<string>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecordTypes()
    {
        var result = await _auditLogService.GetRecordTypesAsync();
        return Ok(result);
    }

    /// <summary>
    /// Retrieve distinct actors who have performed actions recorded in the audit trail.
    /// </summary>
    [HttpGet("actors")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<string>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActors()
    {
        var result = await _auditLogService.GetActorsAsync();
        return Ok(result);
    }

    /// <summary>
    /// Retrieve full details of an immutable audit record by identifier.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<AuditLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuditLogDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _auditLogService.GetAuditLogByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Record a verified administrative, system, or operational audit entry.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<AuditLogDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<AuditLogDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAuditLog([FromBody] CreateAuditLogRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<AuditLogDto>.FailureResult("Validation failed.", errors));
        }

        var result = await _auditLogService.CreateAuditLogAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Export audit logs as CSV while registering an authorized audit event with mandatory administrative rationale.
    /// </summary>
    [HttpPost("export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportAuditLog([FromBody] ExportAuditLogRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Rationale))
        {
            return BadRequest(ApiResponse<object>.FailureResult("An administrative rationale is required for audit log exports."));
        }

        var actor = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "System Administrator";
        var (bytes, logEntry) = await _auditLogService.ExportAuditLogAsync(request, actor);

        Response.Headers.Append("X-Audit-Event-Id", logEntry.Id);
        var fileName = $"carelink-audit-log-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
        return File(bytes, "text/csv", fileName);
    }
}
