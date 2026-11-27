using Microsoft.AspNetCore.Authorization;
using CareLink.Backend.Models;
using CareLink.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.Backend.Controllers;

[ApiController]
[Authorize]
[Route("api/duplicates")]
public class DuplicatesController : ControllerBase
{
    private readonly IDuplicatesService _duplicatesService;
    private readonly ILogger<DuplicatesController> _logger;

    public DuplicatesController(IDuplicatesService duplicatesService, ILogger<DuplicatesController> logger)
    {
        _duplicatesService = duplicatesService;
        _logger = logger;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<DuplicatesStatsDto>>> GetStats()
    {
        try
        {
            var stats = await _duplicatesService.GetStatsAsync();
            return Ok(ApiResponse<DuplicatesStatsDto>.Ok(stats, "Duplicates statistics retrieved successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get duplicates stats");
            return StatusCode(500, ApiResponse<DuplicatesStatsDto>.Fail("Error retrieving duplicates stats."));
        }
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResult<DuplicateCaseItem>>>> GetDuplicates([FromQuery] DuplicateQueryDto query)
    {
        try
        {
            var paged = await _duplicatesService.GetDuplicatesAsync(query);
            return Ok(ApiResponse<PaginatedResult<DuplicateCaseItem>>.Ok(paged));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get duplicates");
            return StatusCode(500, ApiResponse<PaginatedResult<DuplicateCaseItem>>.Fail("Error retrieving duplicates."));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<DuplicateCaseItem>>> GetById(string id)
    {
        try
        {
            var item = await _duplicatesService.GetByIdAsync(id);
            if (item == null)
            {
                return NotFound(ApiResponse<DuplicateCaseItem>.Fail($"Duplicate match {id} not found."));
            }
            return Ok(ApiResponse<DuplicateCaseItem>.Ok(item));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get duplicate match {Id}", id);
            return StatusCode(500, ApiResponse<DuplicateCaseItem>.Fail("Error retrieving duplicate match."));
        }
    }

    [HttpPatch("{id}/status")]
    public async Task<ActionResult<ApiResponse<DuplicateCaseItem>>> UpdateStatus(string id, [FromBody] UpdateDuplicateStatusRequest request)
    {
        try
        {
            var updated = await _duplicatesService.UpdateStatusAsync(id, request);
            if (updated == null)
            {
                return NotFound(ApiResponse<DuplicateCaseItem>.Fail($"Duplicate match {id} not found."));
            }
            return Ok(ApiResponse<DuplicateCaseItem>.Ok(updated, $"Match {id} status updated to {request.Status}."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update match {Id} status", id);
            return StatusCode(500, ApiResponse<DuplicateCaseItem>.Fail("Error updating status."));
        }
    }

    [HttpPost("merge")]
    public async Task<ActionResult<ApiResponse<bool>>> Merge([FromBody] MergeCasesRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.MatchId))
            {
                return BadRequest(ApiResponse<bool>.Fail("Match ID is required."));
            }

            var merged = await _duplicatesService.MergeCasesAsync(request);
            if (!merged)
            {
                return NotFound(ApiResponse<bool>.Fail($"Match {request.MatchId} not found."));
            }
            return Ok(ApiResponse<bool>.Ok(true, $"Successfully merged {request.DuplicateCaseId} into {request.PrimaryCaseId}."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to merge cases");
            return StatusCode(500, ApiResponse<bool>.Fail("Error merging cases."));
        }
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] DuplicateQueryDto query)
    {
        try
        {
            var bytes = await _duplicatesService.ExportCsvAsync(query);
            var fileName = $"carelink-potential-duplicates-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
            return File(bytes, "text/csv", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export duplicates CSV");
            return StatusCode(500, ApiResponse<string>.Fail("Error exporting duplicates."));
        }
    }
}
