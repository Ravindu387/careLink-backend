using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Route("api/audit-events")]
[Route("api/audit-log")]
[Route("api/auditlog")]
public class AuditTimelineController : ControllerBase
{
    private readonly IAuditTimelineService _service;
    private readonly ILogger<AuditTimelineController> _logger;

    public AuditTimelineController(
        IAuditTimelineService service,
        ILogger<AuditTimelineController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves immutable audit events with optional filtering by search text, action, actor, type, and date range.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AuditEventResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? action,
        [FromQuery] string? actionType,
        [FromQuery] string? actor,
        [FromQuery] string? user,
        [FromQuery] string? type,
        [FromQuery] string? recordType,
        [FromQuery] string? dateRange,
        [FromQuery] string? dateFrom,
        [FromQuery] string? dateTo,
        CancellationToken cancellationToken)
    {
        var query = new AuditQueryParameters
        {
            Search = search,
            Action = action ?? actionType,
            ActionType = actionType,
            Actor = actor ?? user,
            User = user,
            Type = type ?? recordType,
            RecordType = recordType,
            DateRange = dateRange,
            DateFrom = dateFrom,
            DateTo = dateTo
        };

        var results = await _service.GetAllAsync(query, cancellationToken);
        return Ok(results);
    }

    /// <summary>
    /// Retrieves summary metrics for the audit log, including total events, today's count, and integrity verification.
    /// </summary>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(AuditTimelineMetricsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMetrics(CancellationToken cancellationToken)
    {
        var metrics = await _service.GetMetricsAsync(cancellationToken);
        return Ok(metrics);
    }

    /// <summary>
    /// Retrieves a single audit event by its identifier.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AuditEventResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        var item = await _service.GetByIdAsync(id, cancellationToken);
        if (item == null)
        {
            return NotFound(new { error = $"Audit event '{id}' was not found." });
        }

        return Ok(item);
    }

    /// <summary>
    /// Logs a new immutable audit record with cryptographic SHA-256 checksum verification.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AuditEventResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LogEvent(
        [FromBody] CreateAuditEventDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _service.LogEventAsync(dto, cancellationToken);
        _logger.LogInformation("Audit event {Id} logged by {Actor} for {Record}: {Action}",
            result.Id, result.Actor, result.Record, result.Action);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Performs cryptographic verification on all audit records to ensure non-repudiation and tamper evidence.
    /// </summary>
    [HttpPost("verify")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> VerifyIntegrity(CancellationToken cancellationToken)
    {
        var isValid = await _service.VerifyLogIntegrityAsync(cancellationToken);
        var metrics = await _service.GetMetricsAsync(cancellationToken);

        return Ok(new
        {
            verified = isValid,
            status = isValid ? "Tamper-proof verified" : "Integrity discrepancy detected",
            recordsChecked = metrics.TotalEvents,
            algorithm = "SHA-256",
            timestamp = DateTimeOffset.UtcNow
        });
    }

    /// <summary>
    /// Resets audit log to baseline demonstration dataset.
    /// </summary>
    [HttpPost("reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Reset(CancellationToken cancellationToken)
    {
        await _service.ResetDataAsync(cancellationToken);
        var metrics = await _service.GetMetricsAsync(cancellationToken);
        return Ok(new { message = "Audit timeline reset to default state.", metrics });
    }
}
