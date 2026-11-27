using Microsoft.AspNetCore.Authorization;
using CareLink.Backend.Models;
using CareLink.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.Backend.Controllers;

[ApiController]
[Authorize]
[Route("api/unassigned-cases")]
public class UnassignedCasesController : ControllerBase
{
    private readonly IUnassignedCasesService _unassignedCasesService;
    private readonly ILogger<UnassignedCasesController> _logger;

    public UnassignedCasesController(IUnassignedCasesService unassignedCasesService, ILogger<UnassignedCasesController> logger)
    {
        _unassignedCasesService = unassignedCasesService;
        _logger = logger;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<UnassignedCasesStatsDto>>> GetStats()
    {
        try
        {
            var stats = await _unassignedCasesService.GetStatsAsync();
            return Ok(ApiResponse<UnassignedCasesStatsDto>.Ok(stats, "Unassigned cases statistics retrieved successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get unassigned cases stats");
            return StatusCode(500, ApiResponse<UnassignedCasesStatsDto>.Fail("Error retrieving unassigned cases stats."));
        }
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResult<UnassignedCase>>>> GetCases([FromQuery] UnassignedCasesQueryDto query)
    {
        try
        {
            var paged = await _unassignedCasesService.GetCasesAsync(query);
            return Ok(ApiResponse<PaginatedResult<UnassignedCase>>.Ok(paged));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get unassigned cases");
            return StatusCode(500, ApiResponse<PaginatedResult<UnassignedCase>>.Fail("Error retrieving unassigned cases."));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<UnassignedCase>>> GetById(string id)
    {
        try
        {
            var item = await _unassignedCasesService.GetByIdAsync(id);
            if (item == null)
            {
                return NotFound(ApiResponse<UnassignedCase>.Fail($"Unassigned case {id} not found."));
            }
            return Ok(ApiResponse<UnassignedCase>.Ok(item));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get unassigned case {Id}", id);
            return StatusCode(500, ApiResponse<UnassignedCase>.Fail("Error retrieving unassigned case."));
        }
    }

    [HttpPost("claim")]
    public async Task<ActionResult<ApiResponse<int>>> ClaimCases([FromBody] ClaimUnassignedCasesRequest request)
    {
        try
        {
            if (request.CaseIds == null || request.CaseIds.Count == 0)
            {
                return BadRequest(ApiResponse<int>.Fail("At least one case ID is required."));
            }

            var claimedCount = await _unassignedCasesService.ClaimCasesAsync(request);
            return Ok(ApiResponse<int>.Ok(claimedCount, $"{claimedCount} case(s) claimed successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to claim unassigned cases");
            return StatusCode(500, ApiResponse<int>.Fail("Error claiming unassigned cases."));
        }
    }

    [HttpPost("assign")]
    public async Task<ActionResult<ApiResponse<UnassignedCase>>> AssignCase([FromBody] AssignUnassignedCaseRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.CaseId))
            {
                return BadRequest(ApiResponse<UnassignedCase>.Fail("Case ID is required."));
            }

            var assigned = await _unassignedCasesService.AssignCaseAsync(request);
            if (assigned == null)
            {
                return NotFound(ApiResponse<UnassignedCase>.Fail($"Case {request.CaseId} not found."));
            }

            return Ok(ApiResponse<UnassignedCase>.Ok(assigned, $"Case {request.CaseId} assigned to {request.AssignedTo} successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to assign case {CaseId}", request.CaseId);
            return StatusCode(500, ApiResponse<UnassignedCase>.Fail("Error assigning case."));
        }
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<ApiResponse<UnassignedCase>>> UpdateCase(string id, [FromBody] UpdateUnassignedCaseRequest request)
    {
        try
        {
            var updated = await _unassignedCasesService.UpdateCaseAsync(id, request);
            if (updated == null)
            {
                return NotFound(ApiResponse<UnassignedCase>.Fail($"Unassigned case {id} not found."));
            }
            return Ok(ApiResponse<UnassignedCase>.Ok(updated, $"Case {id} updated successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update unassigned case {Id}", id);
            return StatusCode(500, ApiResponse<UnassignedCase>.Fail("Error updating unassigned case."));
        }
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] UnassignedCasesQueryDto query)
    {
        try
        {
            var bytes = await _unassignedCasesService.ExportCsvAsync(query);
            var fileName = $"carelink-unassigned-cases-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
            return File(bytes, "text/csv", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export unassigned cases CSV");
            return StatusCode(500, ApiResponse<string>.Fail("Error exporting unassigned cases."));
        }
    }
}
