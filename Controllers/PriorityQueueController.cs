using CareLink.Backend.Models;
using CareLink.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.Backend.Controllers;

[ApiController]
[Route("api/priority-queue")]
public class PriorityQueueController : ControllerBase
{
    private readonly IPriorityQueueService _priorityQueueService;
    private readonly ILogger<PriorityQueueController> _logger;

    public PriorityQueueController(IPriorityQueueService priorityQueueService, ILogger<PriorityQueueController> logger)
    {
        _priorityQueueService = priorityQueueService;
        _logger = logger;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<PriorityQueueStatsDto>>> GetStats()
    {
        try
        {
            var stats = await _priorityQueueService.GetStatsAsync();
            return Ok(ApiResponse<PriorityQueueStatsDto>.Ok(stats, "Priority queue statistics retrieved successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get priority queue stats");
            return StatusCode(500, ApiResponse<PriorityQueueStatsDto>.Fail("Error retrieving priority queue stats."));
        }
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResult<PriorityQueueItem>>>> GetCases([FromQuery] PriorityQueueQueryDto query)
    {
        try
        {
            var paged = await _priorityQueueService.GetCasesAsync(query);
            return Ok(ApiResponse<PaginatedResult<PriorityQueueItem>>.Ok(paged));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get priority queue cases");
            return StatusCode(500, ApiResponse<PaginatedResult<PriorityQueueItem>>.Fail("Error retrieving priority queue cases."));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<PriorityQueueItem>>> GetById(string id)
    {
        try
        {
            var item = await _priorityQueueService.GetByIdAsync(id);
            if (item == null)
            {
                return NotFound(ApiResponse<PriorityQueueItem>.Fail($"Priority case {id} not found."));
            }
            return Ok(ApiResponse<PriorityQueueItem>.Ok(item));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get priority case {Id}", id);
            return StatusCode(500, ApiResponse<PriorityQueueItem>.Fail("Error retrieving priority case."));
        }
    }

    [HttpPost("claim")]
    public async Task<ActionResult<ApiResponse<int>>> ClaimCases([FromBody] ClaimPriorityCasesRequest request)
    {
        try
        {
            if (request.CaseIds == null || request.CaseIds.Count == 0)
            {
                return BadRequest(ApiResponse<int>.Fail("At least one case ID is required."));
            }

            var claimedCount = await _priorityQueueService.ClaimCasesAsync(request);
            return Ok(ApiResponse<int>.Ok(claimedCount, $"{claimedCount} case(s) claimed successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to claim priority cases");
            return StatusCode(500, ApiResponse<int>.Fail("Error claiming priority cases."));
        }
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<ApiResponse<PriorityQueueItem>>> UpdateCase(string id, [FromBody] UpdatePriorityCaseRequest request)
    {
        try
        {
            var updated = await _priorityQueueService.UpdateCaseAsync(id, request);
            if (updated == null)
            {
                return NotFound(ApiResponse<PriorityQueueItem>.Fail($"Priority case {id} not found."));
            }
            return Ok(ApiResponse<PriorityQueueItem>.Ok(updated, $"Case {id} updated successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update priority case {Id}", id);
            return StatusCode(500, ApiResponse<PriorityQueueItem>.Fail("Error updating priority case."));
        }
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] PriorityQueueQueryDto query)
    {
        try
        {
            var bytes = await _priorityQueueService.ExportCsvAsync(query);
            var fileName = $"carelink-priority-queue-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
            return File(bytes, "text/csv", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export priority queue CSV");
            return StatusCode(500, ApiResponse<string>.Fail("Error exporting priority queue."));
        }
    }
}
