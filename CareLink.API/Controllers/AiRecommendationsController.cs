using System.Security.Claims;
using CareLink.Application.Common;
using CareLink.Application.DTOs.AiRecommendations;
using CareLink.Application.Interfaces;
using CareLink.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Route("api/ai-recommendations")]
[Route("api/ai")]
[Authorize(Roles = $"{UserRoles.Admin},{UserRoles.SystemAdmin},{UserRoles.EmergencyCoordinator},{UserRoles.IncidentManager},{UserRoles.VolunteerCoordinator}")]
public class AiRecommendationsController : ControllerBase
{
    private readonly IAiRecommendationService _service;

    public AiRecommendationsController(IAiRecommendationService service)
    {
        _service = service;
    }

    /// <summary>
    /// Retrieve AI recommendations with optional status, priority, category, crisis, and search filters.
    /// </summary>
    [HttpGet]
    [HttpGet("recommendations")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AiRecommendationSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecommendations(
        [FromQuery] string? status,
        [FromQuery] string? priority,
        [FromQuery] string? category,
        [FromQuery] string? crisisId,
        [FromQuery] string? search)
    {
        var result = await _service.GetRecommendationsAsync(status, priority, category, crisisId, search);
        return Ok(result);
    }

    /// <summary>
    /// Retrieve overall AI recommendation statistics (awaiting, approved, modified, deferred, avg confidence).
    /// </summary>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(ApiResponse<AiRecommendationMetricsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMetrics()
    {
        var result = await _service.GetMetricsAsync();
        return Ok(result);
    }

    /// <summary>
    /// Retrieve full review details of an AI recommendation by its ID.
    /// </summary>
    [HttpGet("{id}")]
    [HttpGet("recommendations/{id}")]
    [ProducesResponseType(typeof(ApiResponse<AiRecommendationDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AiRecommendationDetailDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _service.GetRecommendationByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Retrieve full review details of an AI recommendation by the associated request ID (e.g. REQ-2841).
    /// </summary>
    [HttpGet("by-request/{requestId}")]
    [ProducesResponseType(typeof(ApiResponse<AiRecommendationDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AiRecommendationDetailDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByRequestId(string requestId)
    {
        var result = await _service.GetRecommendationByRequestIdAsync(requestId);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Record an authorized human decision (Approve or Modify priority with required rationale).
    /// </summary>
    [HttpPost("{id}/decision")]
    [ProducesResponseType(typeof(ApiResponse<AiRecommendationDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AiRecommendationDetailDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<AiRecommendationDetailDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordDecision(string id, [FromBody] RecordDecisionRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return BadRequest(ApiResponse<AiRecommendationDetailDto>.FailureResult("Validation failed.", errors));
        }

        var reviewerName = GetCurrentUserName();
        var result = await _service.RecordDecisionAsync(id, request, reviewerName);
        if (!result.Success)
        {
            if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(result);
            }
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Explicitly approve an AI recommendation with optional notes.
    /// </summary>
    [HttpPost("{id}/approve")]
    [HttpPost("recommendations/{id}/approve")]
    [ProducesResponseType(typeof(ApiResponse<AiRecommendationDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AiRecommendationDetailDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApproveRecommendation(string id, [FromBody] ApproveRecommendationRequestDto? request)
    {
        var reviewerName = GetCurrentUserName();
        var result = await _service.ApproveRecommendationAsync(id, request, reviewerName);
        if (!result.Success)
        {
            if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(result);
            }
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Reject an AI recommendation with mandatory rationale.
    /// </summary>
    [HttpPost("{id}/reject")]
    [HttpPost("recommendations/{id}/reject")]
    [ProducesResponseType(typeof(ApiResponse<AiRecommendationDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AiRecommendationDetailDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<AiRecommendationDetailDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RejectRecommendation(string id, [FromBody] RejectRecommendationRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request?.RejectionReason))
        {
            return BadRequest(ApiResponse<AiRecommendationDetailDto>.FailureResult("A rejection reason is required."));
        }

        var reviewerName = GetCurrentUserName();
        var result = await _service.RejectRecommendationAsync(id, request, reviewerName);
        if (!result.Success)
        {
            if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(result);
            }
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Defer an AI recommendation for subsequent evaluation.
    /// </summary>
    [HttpPost("{id}/defer")]
    [HttpPost("recommendations/{id}/defer")]
    [ProducesResponseType(typeof(ApiResponse<AiRecommendationDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AiRecommendationDetailDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeferRecommendation(string id, [FromBody] DeferRecommendationRequestDto? request)
    {
        var reviewerName = GetCurrentUserName();
        var result = await _service.DeferRecommendationAsync(id, request, reviewerName);
        if (!result.Success)
        {
            if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(result);
            }
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Reset an AI recommendation decision back to awaiting review status.
    /// </summary>
    [HttpPost("{id}/reset")]
    [HttpPost("recommendations/{id}/reset")]
    [ProducesResponseType(typeof(ApiResponse<AiRecommendationDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AiRecommendationDetailDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetRecommendation(string id)
    {
        var result = await _service.ResetRecommendationAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Generate an AI situational crisis analysis using Google Gemini.
    /// </summary>
    [HttpPost("crises/{crisisId}/analysis")]
    [ProducesResponseType(typeof(ApiResponse<AiRecommendationDetailDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GenerateCrisisAnalysis(
        string crisisId,
        [FromBody] GenerateCrisisAnalysisRequestDto? request,
        CancellationToken cancellationToken)
    {
        var reviewerName = GetCurrentUserName();
        var result = await _service.GenerateCrisisAnalysisAsync(
            crisisId,
            request ?? new GenerateCrisisAnalysisRequestDto { CrisisId = crisisId },
            reviewerName,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Generate AI resource allocation recommendations respecting real inventory and detecting shortages.
    /// </summary>
    [HttpPost("crises/{crisisId}/resources")]
    [HttpPost("crises/{crisisId}/recommendations")]
    [ProducesResponseType(typeof(ApiResponse<AiRecommendationDetailDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GenerateResourceRecommendations(
        string crisisId,
        [FromBody] GenerateResourceRecommendationRequestDto? request,
        CancellationToken cancellationToken)
    {
        var reviewerName = GetCurrentUserName();
        var result = await _service.GenerateResourceRecommendationsAsync(
            crisisId,
            request ?? new GenerateResourceRecommendationRequestDto { CrisisId = crisisId },
            reviewerName,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Generate AI shelter matching recommendations based on actual capacity and facilities.
    /// </summary>
    [HttpPost("crises/{crisisId}/shelters")]
    [ProducesResponseType(typeof(ApiResponse<AiRecommendationDetailDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GenerateShelterRecommendations(
        string crisisId,
        [FromBody] GenerateShelterRecommendationRequestDto? request,
        CancellationToken cancellationToken)
    {
        var reviewerName = GetCurrentUserName();
        var result = await _service.GenerateShelterRecommendationsAsync(
            crisisId,
            request ?? new GenerateShelterRecommendationRequestDto { CrisisId = crisisId },
            reviewerName,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Generate AI volunteer matching recommendations considering skills and current availability.
    /// </summary>
    [HttpPost("crises/{crisisId}/volunteers")]
    [ProducesResponseType(typeof(ApiResponse<AiRecommendationDetailDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GenerateVolunteerRecommendations(
        string crisisId,
        [FromBody] GenerateVolunteerRecommendationRequestDto? request,
        CancellationToken cancellationToken)
    {
        var reviewerName = GetCurrentUserName();
        var result = await _service.GenerateVolunteerRecommendationsAsync(
            crisisId,
            request ?? new GenerateVolunteerRecommendationRequestDto { CrisisId = crisisId },
            reviewerName,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Generate a comprehensive multi-entity crisis response plan.
    /// </summary>
    [HttpPost("crises/{crisisId}/response-plan")]
    [ProducesResponseType(typeof(ApiResponse<AiRecommendationDetailDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GenerateResponsePlan(
        string crisisId,
        [FromBody] GenerateResponsePlanRequestDto? request,
        CancellationToken cancellationToken)
    {
        var reviewerName = GetCurrentUserName();
        var result = await _service.GenerateResponsePlanAsync(
            crisisId,
            request ?? new GenerateResponsePlanRequestDto { CrisisId = crisisId },
            reviewerName,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    private string GetCurrentUserName()
    {
        return User.FindFirst(ClaimTypes.Name)?.Value
            ?? User.FindFirst("unique_name")?.Value
            ?? User.Identity?.Name
            ?? "Authorised Reviewer";
    }
}
