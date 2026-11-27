using System.Security.Claims;
using System.Text;
using CareLink.Application.Common;
using CareLink.Application.DTOs;
using CareLink.Application.DTOs.AuditLogs;
using CareLink.Application.DTOs.Users;
using CareLink.Application.Interfaces;
using CareLink.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Authorize]
[Route("api/audit-logs")]
[Route("api/auditlogs")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogsController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    /// <summary>
    /// Retrieve audit logs with optional pagination and filtering.
    /// Supports both paginated administrative queries and flat filter queries.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] string? search,
        [FromQuery] string? action,
        [FromQuery] string? actionType,
        [FromQuery] string? actor,
        [FromQuery] string? recordType,
        [FromQuery] string? date,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] int? page,
        [FromQuery] int? pageSize)
    {
        if (page.HasValue && page.Value > 0)
        {
            var query = new AuditLogQueryDto
            {
                Search = search,
                Action = action ?? actionType,
                Actor = actor,
                RecordType = recordType,
                StartDate = startDate,
                EndDate = endDate,
                Page = page.Value,
                PageSize = pageSize ?? 10
            };
            var result = await _auditLogService.GetAuditLogsAsync(query);
            return Ok(result);
        }
        else
        {
            var filters = new AuditLogFilterParams
            {
                Search = search,
                ActionType = actionType ?? action,
                RecordType = recordType,
                Date = date
            };
            var result = await _auditLogService.GetAuditLogsAsync(filters);
            return Ok(result);
        }
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
    /// Retrieve summary metrics for audit logs (Oshadi).
    /// </summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var result = await _auditLogService.GetSummaryAsync();
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
    public async Task<IActionResult> CreateAuditLog([FromBody] CreateAuditLogRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<AuditLogDto>.FailureResult("Validation failed.", errors));
        }

        var result = await _auditLogService.CreateAuditLogAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Export audit logs as CSV with mandatory administrative rationale (HEAD).
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

    /// <summary>
    /// Export audit logs as CSV using filter parameters (Oshadi).
    /// </summary>
    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] AuditLogFilterParams filters)
    {
        var result = await _auditLogService.GetAuditLogsAsync(filters);
        var logs = result.Data ?? [];

        var sb = new StringBuilder();
        sb.AppendLine("Event ID,Action,Actor,Timestamp,Record,Change,Rationale,Action Type,Record Type,Date");

        foreach (var item in logs)
        {
            sb.AppendLine($"\"{EscapeCsv(item.Id)}\",\"{EscapeCsv(item.Action)}\",\"{EscapeCsv(item.Actor)}\",\"{EscapeCsv(item.Timestamp)}\",\"{EscapeCsv(item.Record)}\",\"{EscapeCsv(item.Change)}\",\"{EscapeCsv(item.Rationale)}\",\"{EscapeCsv(item.ActionType)}\",\"{EscapeCsv(item.RecordType)}\",\"{EscapeCsv(item.Date)}\"");
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", $"carelink-audit-log-{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
    }

    private static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value.Replace("\"", "\"\"");
    }
}
