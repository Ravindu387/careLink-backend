using System.Security.Claims;
using CareLink.Application.Common;
using CareLink.Application.DTOs.Users;
using CareLink.Application.DTOs.Verification;
using CareLink.Application.Interfaces;
using CareLink.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Route("api/verification")]
[Authorize(Roles = $"{UserRoles.Admin},{UserRoles.SystemAdmin},{UserRoles.EmergencyCoordinator},{UserRoles.IncidentManager}")]
public class VerificationController : ControllerBase
{
    private readonly IVerificationService _verificationService;

    public VerificationController(IVerificationService verificationService)
    {
        _verificationService = verificationService;
    }

    /// <summary>
    /// Retrieve paginated verification queue requests with optional search, type, priority, and status filters.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedListDto<VerificationRequestDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRequests([FromQuery] VerificationQueryDto query)
    {
        var result = await _verificationService.GetRequestsAsync(query);
        return Ok(result);
    }

    /// <summary>
    /// Retrieve verification queue overview statistics and SLA compliance metrics.
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(ApiResponse<VerificationStatsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats()
    {
        var result = await _verificationService.GetStatsAsync();
        return Ok(result);
    }

    /// <summary>
    /// Retrieve distinct verification request types for filtering.
    /// </summary>
    [HttpGet("types")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<string>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTypes()
    {
        var result = await _verificationService.GetRequestTypesAsync();
        return Ok(result);
    }

    /// <summary>
    /// Retrieve details of a specific verification request by identifier.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<VerificationRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<VerificationRequestDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _verificationService.GetRequestByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Submit a new identity or organization credential request into the verification queue.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<VerificationRequestDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<VerificationRequestDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateRequest([FromBody] CreateVerificationRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<VerificationRequestDto>.FailureResult("Validation failed.", errors));
        }

        var result = await _verificationService.CreateRequestAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Record an authorized administrative verification decision (Approve, Reject, or Request Info) with audit logging.
    /// </summary>
    [HttpPost("{id}/decision")]
    [ProducesResponseType(typeof(ApiResponse<VerificationRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<VerificationRequestDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<VerificationRequestDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordDecision(string id, [FromBody] VerificationDecisionRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<VerificationRequestDto>.FailureResult("Validation failed.", errors));
        }

        var actor = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "System Administrator";
        var result = await _verificationService.RecordDecisionAsync(id, request, actor);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Export filtered verification queue records as a CSV file.
    /// </summary>
    [HttpGet("export-csv")]
    [Produces("text/csv")]
    public async Task<IActionResult> ExportCsv([FromQuery] VerificationQueryDto query)
    {
        var bytes = await _verificationService.ExportCsvAsync(query);
        var fileName = $"verification-queue-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
        return File(bytes, "text/csv", fileName);
    }
}
