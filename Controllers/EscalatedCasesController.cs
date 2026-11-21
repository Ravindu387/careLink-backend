using CareLink.Backend.Models;
using CareLink.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.Backend.Controllers;

[ApiController]
[Route("api/escalatedcases")]
public class EscalatedCasesController : ControllerBase
{
    private readonly IEscalatedCasesService _escalatedService;
    private readonly ILogger<EscalatedCasesController> _logger;

    public EscalatedCasesController(IEscalatedCasesService escalatedService, ILogger<EscalatedCasesController> logger)
    {
        _escalatedService = escalatedService;
        _logger = logger;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<EscalationStatsDto>>> GetStats()
    {
        try
        {
            var stats = await _escalatedService.GetStatsAsync();
            return Ok(ApiResponse<EscalationStatsDto>.Ok(stats, "Escalation stats loaded successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load escalation stats");
            return StatusCode(500, ApiResponse<EscalationStatsDto>.Fail("Error loading escalation stats."));
        }
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResult<EscalatedCaseItem>>>> GetCases([FromQuery] EscalatedQueryDto query)
    {
        try
        {
            var paged = await _escalatedService.GetCasesAsync(query);
            return Ok(ApiResponse<PaginatedResult<EscalatedCaseItem>>.Ok(paged));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve escalated cases");
            return StatusCode(500, ApiResponse<PaginatedResult<EscalatedCaseItem>>.Fail("Error retrieving escalated cases."));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<EscalatedCaseItem>>> GetCaseById(string id)
    {
        try
        {
            var item = await _escalatedService.GetCaseByIdAsync(id);
            if (item == null)
            {
                return NotFound(ApiResponse<EscalatedCaseItem>.Fail($"Escalated case {id} not found."));
            }
            return Ok(ApiResponse<EscalatedCaseItem>.Ok(item));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get escalated case {Id}", id);
            return StatusCode(500, ApiResponse<EscalatedCaseItem>.Fail("Error retrieving case."));
        }
    }

    [HttpPost("bulk-action")]
    public async Task<ActionResult<ApiResponse<int>>> BulkAction([FromBody] BulkEscalationActionRequest request)
    {
        try
        {
            if (request.CaseIds == null || request.CaseIds.Count == 0)
            {
                return BadRequest(ApiResponse<int>.Fail("At least one case must be selected."));
            }

            var count = await _escalatedService.BulkActionAsync(request);
            return Ok(ApiResponse<int>.Ok(count, $"{count} case(s) updated with action '{request.Action}'."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to perform bulk action");
            return StatusCode(500, ApiResponse<int>.Fail("Error processing bulk action."));
        }
    }

    [HttpPost("{id}/decision")]
    public async Task<ActionResult<ApiResponse<EscalatedCaseItem>>> RecordDecision(string id, [FromBody] EscalationDecisionRequest request)
    {
        try
        {
            var updated = await _escalatedService.RecordDecisionAsync(id, request);
            if (updated == null)
            {
                return NotFound(ApiResponse<EscalatedCaseItem>.Fail($"Case {id} not found."));
            }
            return Ok(ApiResponse<EscalatedCaseItem>.Ok(updated, $"Case {id} decision '{request.Action}' recorded."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record coordinator decision for case {Id}", id);
            return StatusCode(500, ApiResponse<EscalatedCaseItem>.Fail("Error updating escalated case."));
        }
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportCases([FromQuery] EscalatedQueryDto query)
    {
        try
        {
            var bytes = await _escalatedService.ExportCsvAsync(query);
            var fileName = $"carelink-escalated-cases-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
            return File(bytes, "text/csv", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export escalated cases CSV");
            return StatusCode(500, ApiResponse<string>.Fail("Error exporting escalated cases."));
        }
    }
}
