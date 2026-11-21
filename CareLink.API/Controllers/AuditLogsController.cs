namespace CareLink.API.Controllers;

using System.Text;
using Microsoft.AspNetCore.Mvc;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogsController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAuditLogs([FromQuery] AuditLogFilterParams filters)
    {
        var result = await _auditLogService.GetAuditLogsAsync(filters);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _auditLogService.GetAuditLogByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var result = await _auditLogService.GetSummaryAsync();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAuditLogRequest request)
    {
        var result = await _auditLogService.CreateAuditLogAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

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
