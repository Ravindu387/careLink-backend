using CareLink.Backend.Models;
using CareLink.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.Backend.Controllers;

[ApiController]
[Route("api/caseverification")]
public class CaseVerificationController : ControllerBase
{
    private readonly ICaseVerificationService _verificationService;
    private readonly ILogger<CaseVerificationController> _logger;

    public CaseVerificationController(ICaseVerificationService verificationService, ILogger<CaseVerificationController> logger)
    {
        _verificationService = verificationService;
        _logger = logger;
    }

    [HttpGet("analytics")]
    public async Task<ActionResult<ApiResponse<VerificationAnalyticsResponse>>> GetAnalytics()
    {
        try
        {
            var data = await _verificationService.GetAnalyticsAsync();
            return Ok(ApiResponse<VerificationAnalyticsResponse>.Ok(data, "Verification analytics loaded successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load verification analytics");
            return StatusCode(500, ApiResponse<VerificationAnalyticsResponse>.Fail("Error loading verification analytics."));
        }
    }

    [HttpGet("cases")]
    public async Task<ActionResult<ApiResponse<PaginatedResult<VerificationCaseItem>>>> GetCases([FromQuery] VerificationQueryDto query)
    {
        try
        {
            var paged = await _verificationService.GetCasesAsync(query);
            return Ok(ApiResponse<PaginatedResult<VerificationCaseItem>>.Ok(paged));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve verification cases");
            return StatusCode(500, ApiResponse<PaginatedResult<VerificationCaseItem>>.Fail("Error retrieving verification cases."));
        }
    }

    [HttpGet("cases/{id}")]
    public async Task<ActionResult<ApiResponse<VerificationCaseItem>>> GetCaseById(string id)
    {
        try
        {
            var item = await _verificationService.GetCaseByIdAsync(id);
            if (item == null)
            {
                return NotFound(ApiResponse<VerificationCaseItem>.Fail($"Case {id} not found."));
            }
            return Ok(ApiResponse<VerificationCaseItem>.Ok(item));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get verification case {Id}", id);
            return StatusCode(500, ApiResponse<VerificationCaseItem>.Fail("Error retrieving case."));
        }
    }

    [HttpPost("cases/{id}/decision")]
    public async Task<ActionResult<ApiResponse<VerificationCaseItem>>> RecordDecision(string id, [FromBody] VerificationDecisionRequest request)
    {
        try
        {
            var updated = await _verificationService.RecordDecisionAsync(id, request);
            if (updated == null)
            {
                return NotFound(ApiResponse<VerificationCaseItem>.Fail($"Case {id} not found."));
            }

            var msg = request.Decision.ToLowerInvariant() switch
            {
                "verify" or "approve" => $"Case {id} successfully verified and approved.",
                "requestinfo" or "needs info" => $"Information request sent for case {id}.",
                "reject" => $"Case {id} rejected with recorded rationale.",
                _ => $"Decision recorded for case {id}."
            };

            return Ok(ApiResponse<VerificationCaseItem>.Ok(updated, msg));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record decision for case {Id}", id);
            return StatusCode(500, ApiResponse<VerificationCaseItem>.Fail("Error recording verification decision."));
        }
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportCases([FromQuery] VerificationQueryDto query)
    {
        try
        {
            var bytes = await _verificationService.ExportVerificationCsvAsync(query);
            var fileName = $"carelink-verification-cases-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
            return File(bytes, "text/csv", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export verification cases CSV");
            return StatusCode(500, ApiResponse<string>.Fail("Error exporting verification cases."));
        }
    }
}
