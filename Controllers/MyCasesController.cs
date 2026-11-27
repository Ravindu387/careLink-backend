using CareLink.Application.Interfaces;
using CareLink.Backend.Models;
using CareLink.Backend.Services;
using CareLink.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.Backend.Controllers;

[ApiController]
[Authorize]
[Route("api/mycases")]
public class MyCasesController : ControllerBase
{
    private readonly IMyCasesService _myCasesService;
    private readonly ICaseManagementService? _caseManagementService;
    private readonly ICitizenRequestService? _citizenRequestService;
    private readonly IAssistanceRequestRepository? _assistanceRequestRepository;
    private readonly ILogger<MyCasesController> _logger;

    public MyCasesController(
        IMyCasesService myCasesService,
        ILogger<MyCasesController> logger,
        ICaseManagementService? caseManagementService = null,
        ICitizenRequestService? citizenRequestService = null,
        IAssistanceRequestRepository? assistanceRequestRepository = null)
    {
        _myCasesService = myCasesService;
        _logger = logger;
        _caseManagementService = caseManagementService;
        _citizenRequestService = citizenRequestService;
        _assistanceRequestRepository = assistanceRequestRepository;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<MyCasesStatsDto>>> GetStats()
    {
        try
        {
            var stats = await _myCasesService.GetStatsAsync();
            return Ok(ApiResponse<MyCasesStatsDto>.Ok(stats, "My cases stats loaded successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load my cases stats");
            return StatusCode(500, ApiResponse<MyCasesStatsDto>.Fail("Error loading my cases stats."));
        }
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResult<MyCaseItem>>>> GetCases([FromQuery] MyCasesQueryDto query)
    {
        try
        {
            var paged = await _myCasesService.GetCasesAsync(query);
            return Ok(ApiResponse<PaginatedResult<MyCaseItem>>.Ok(paged));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve my cases");
            return StatusCode(500, ApiResponse<PaginatedResult<MyCaseItem>>.Fail("Error retrieving my cases."));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<MyCaseItem>>> GetCaseById(string id)
    {
        try
        {
            var item = await _myCasesService.GetCaseByIdAsync(id);
            if (item == null)
            {
                return NotFound(ApiResponse<MyCaseItem>.Fail($"Case {id} not found."));
            }
            return Ok(ApiResponse<MyCaseItem>.Ok(item));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get case {Id}", id);
            return StatusCode(500, ApiResponse<MyCaseItem>.Fail("Error retrieving case."));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<MyCaseItem>>> CreateCase([FromBody] CreateMyCaseRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.ApplicantName) || string.IsNullOrWhiteSpace(request.Location))
            {
                return BadRequest(ApiResponse<MyCaseItem>.Fail("Applicant name and location are required."));
            }

            var created = await _myCasesService.CreateCaseAsync(request);
            return CreatedAtAction(nameof(GetCaseById), new { id = created.Id }, ApiResponse<MyCaseItem>.Ok(created, $"Case {created.Id} created and assigned to you."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create case");
            return StatusCode(500, ApiResponse<MyCaseItem>.Fail("Error creating case."));
        }
    }

    [HttpPatch("{id}/status")]
    public async Task<ActionResult<ApiResponse<MyCaseItem>>> UpdateStatus(string id, [FromBody] UpdateMyCaseStatusRequest request)
    {
        try
        {
            var updated = await _myCasesService.UpdateStatusAsync(id, request);
            if (updated == null)
            {
                return NotFound(ApiResponse<MyCaseItem>.Fail($"Case {id} not found."));
            }

            if (_caseManagementService != null)
            {
                await _caseManagementService.UpdateStatusAsync(id, request.Status, request.Notes);
            }

            if (_citizenRequestService != null)
            {
                await _citizenRequestService.UpdateStatusAsync(id, request.Status);
            }

            if (_assistanceRequestRepository != null)
            {
                var existingReq = await _assistanceRequestRepository.GetByIdAsync(id);
                if (existingReq != null)
                {
                    existingReq.Status = request.Status;
                    await _assistanceRequestRepository.UpdateAsync(existingReq);
                }
            }

            return Ok(ApiResponse<MyCaseItem>.Ok(updated, $"Case {id} status updated to {request.Status}."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update case {Id} status", id);
            return StatusCode(500, ApiResponse<MyCaseItem>.Fail("Error updating status."));
        }
    }

    [HttpPost("bulk-action")]
    public async Task<ActionResult<ApiResponse<int>>> BulkAction([FromBody] BulkMyCasesActionRequest request)
    {
        try
        {
            if (request.CaseIds == null || request.CaseIds.Count == 0)
            {
                return BadRequest(ApiResponse<int>.Fail("At least one case must be selected."));
            }

            var count = await _myCasesService.BulkActionAsync(request);
            return Ok(ApiResponse<int>.Ok(count, $"{count} case(s) updated with action '{request.Action}'."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process bulk action");
            return StatusCode(500, ApiResponse<int>.Fail("Error processing bulk action."));
        }
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportCases([FromQuery] MyCasesQueryDto query)
    {
        try
        {
            var bytes = await _myCasesService.ExportCsvAsync(query);
            var fileName = $"carelink-mycases-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
            return File(bytes, "text/csv", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export my cases CSV");
            return StatusCode(500, ApiResponse<string>.Fail("Error exporting cases."));
        }
    }
}
