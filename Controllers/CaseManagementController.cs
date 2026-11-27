using CareLink.Application.Interfaces;
using CareLink.Backend.Models;
using CareLink.Backend.Services;
using CareLink.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.Backend.Controllers;

[ApiController]
[Authorize]
[Route("api/casemanagement")]
public class CaseManagementController : ControllerBase
{
    private readonly ICaseManagementService _caseService;
    private readonly IMyCasesService? _myCasesService;
    private readonly ICitizenRequestService? _citizenRequestService;
    private readonly IAssistanceRequestRepository? _assistanceRequestRepository;
    private readonly ILogger<CaseManagementController> _logger;

    public CaseManagementController(
        ICaseManagementService caseService,
        ILogger<CaseManagementController> logger,
        IMyCasesService? myCasesService = null,
        ICitizenRequestService? citizenRequestService = null,
        IAssistanceRequestRepository? assistanceRequestRepository = null)
    {
        _caseService = caseService;
        _logger = logger;
        _myCasesService = myCasesService;
        _citizenRequestService = citizenRequestService;
        _assistanceRequestRepository = assistanceRequestRepository;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<ApiResponse<CaseDashboardResponse>>> GetDashboard()
    {
        try
        {
            var data = await _caseService.GetDashboardDataAsync();
            return Ok(ApiResponse<CaseDashboardResponse>.Ok(data, "Dashboard data loaded successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load dashboard data");
            return StatusCode(500, ApiResponse<CaseDashboardResponse>.Fail("An error occurred while loading dashboard metrics."));
        }
    }

    [HttpGet("cases")]
    public async Task<ActionResult<ApiResponse<List<CaseItem>>>> GetCases(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] string? priority,
        [FromQuery] string? assigned)
    {
        try
        {
            var list = await _caseService.GetCasesAsync(search, status, priority, assigned);
            return Ok(ApiResponse<List<CaseItem>>.Ok(list));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve cases");
            return StatusCode(500, ApiResponse<List<CaseItem>>.Fail("Error retrieving cases."));
        }
    }

    [HttpGet("cases/{id}")]
    public async Task<ActionResult<ApiResponse<CaseItem>>> GetCaseById(string id)
    {
        try
        {
            var item = await _caseService.GetCaseByIdAsync(id);
            if (item == null)
            {
                return NotFound(ApiResponse<CaseItem>.Fail($"Case {id} not found."));
            }
            return Ok(ApiResponse<CaseItem>.Ok(item));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get case {Id}", id);
            return StatusCode(500, ApiResponse<CaseItem>.Fail("Error retrieving case."));
        }
    }

    [HttpPost("cases")]
    public async Task<ActionResult<ApiResponse<CaseItem>>> CreateCase([FromBody] CreateCaseRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Category) || string.IsNullOrWhiteSpace(request.ApplicantName))
            {
                return BadRequest(ApiResponse<CaseItem>.Fail("Category and applicant name are required."));
            }

            var created = await _caseService.CreateCaseAsync(request);
            return CreatedAtAction(nameof(GetCaseById), new { id = created.Id }, ApiResponse<CaseItem>.Ok(created, "Case created successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create case");
            return StatusCode(500, ApiResponse<CaseItem>.Fail("Error creating case."));
        }
    }

    [HttpPost("cases/{id}/claim")]
    public async Task<ActionResult<ApiResponse<CaseItem>>> ClaimCase(string id, [FromBody] ClaimCaseRequest? request)
    {
        try
        {
            var updated = await _caseService.ClaimCaseAsync(id, request?.AssignedTo);
            if (updated == null)
            {
                return NotFound(ApiResponse<CaseItem>.Fail($"Case {id} not found."));
            }
            return Ok(ApiResponse<CaseItem>.Ok(updated, $"Case {id} claimed successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to claim case {Id}", id);
            return StatusCode(500, ApiResponse<CaseItem>.Fail("Error claiming case."));
        }
    }

    [HttpPatch("cases/{id}/status")]
    public async Task<ActionResult<ApiResponse<CaseItem>>> UpdateStatus(string id, [FromBody] UpdateCaseStatusRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Status))
            {
                return BadRequest(ApiResponse<CaseItem>.Fail("Status cannot be empty."));
            }

            var updated = await _caseService.UpdateStatusAsync(id, request.Status, request.Notes);
            if (updated == null)
            {
                return NotFound(ApiResponse<CaseItem>.Fail($"Case {id} not found."));
            }

            if (_myCasesService != null)
            {
                await _myCasesService.UpdateStatusAsync(id, new UpdateMyCaseStatusRequest
                {
                    Status = request.Status,
                    Notes = request.Notes
                });
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

            return Ok(ApiResponse<CaseItem>.Ok(updated, $"Case {id} status updated to {request.Status}."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update case {Id} status", id);
            return StatusCode(500, ApiResponse<CaseItem>.Fail("Error updating status."));
        }
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportSummary()
    {
        try
        {
            var bytes = await _caseService.ExportSummaryCsvAsync();
            var fileName = $"carelink-cases-summary-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
            return File(bytes, "text/csv", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export summary");
            return StatusCode(500, ApiResponse<string>.Fail("Error exporting summary report."));
        }
    }
}
