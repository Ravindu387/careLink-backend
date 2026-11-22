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
[Authorize(Roles = $"{UserRoles.Admin},{UserRoles.SystemAdmin},{UserRoles.EmergencyCoordinator},{UserRoles.IncidentManager},{UserRoles.VolunteerCoordinator}")]
public class AiRecommendationsController : ControllerBase
{
    private readonly IAiRecommendationService _service;

    public AiRecommendationsController(IAiRecommendationService service)
    {
        _service = service;
    }

    /// <summary>
    /// Retrieve AI recommendations with optional status, priority, and search filters.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AiRecommendationSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecommendations(
        [FromQuery] string? status,
        [FromQuery] string? priority,
        [FromQuery] string? search)
    {
        var result = await _service.GetRecommendationsAsync(status, priority, search);
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

        var reviewerName = User.FindFirst(ClaimTypes.Name)?.Value
            ?? User.FindFirst("unique_name")?.Value
            ?? User.Identity?.Name
            ?? "Authorised Reviewer";

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
    /// Defer an AI recommendation for subsequent evaluation.
    /// </summary>
    [HttpPost("{id}/defer")]
    [ProducesResponseType(typeof(ApiResponse<AiRecommendationDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AiRecommendationDetailDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeferRecommendation(string id, [FromBody] DeferRecommendationRequestDto? request)
    {
        var reviewerName = User.FindFirst(ClaimTypes.Name)?.Value
            ?? User.FindFirst("unique_name")?.Value
            ?? User.Identity?.Name
            ?? "Authorised Reviewer";

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
}
