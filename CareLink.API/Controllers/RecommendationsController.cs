using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecommendationsController : ControllerBase
{
    private readonly IRecommendationService _recommendationService;
    private readonly ILogger<RecommendationsController> _logger;

    public RecommendationsController(
        IRecommendationService recommendationService,
        ILogger<RecommendationsController> logger)
    {
        _recommendationService = recommendationService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves AI recommendations with optional filtering by search term, category, priority, review state, and confidence range.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RecommendationResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? category,
        [FromQuery] string? priority,
        [FromQuery] string? reviewState,
        [FromQuery] string? confidence,
        [FromQuery] int? minConfidence,
        [FromQuery] int? maxConfidence,
        CancellationToken cancellationToken)
    {
        // Parse friendly confidence strings if provided
        if (!string.IsNullOrWhiteSpace(confidence) && !confidence.Equals("Confidence range", StringComparison.OrdinalIgnoreCase))
        {
            if (confidence.Contains("90", StringComparison.OrdinalIgnoreCase))
            {
                minConfidence = 90;
            }
            else if (confidence.Contains("80", StringComparison.OrdinalIgnoreCase) && confidence.Contains("89", StringComparison.OrdinalIgnoreCase))
            {
                minConfidence = 80;
                maxConfidence = 89;
            }
            else if (confidence.Contains("below", StringComparison.OrdinalIgnoreCase))
            {
                maxConfidence = 79;
            }
        }

        var query = new RecommendationQueryParameters
        {
            Search = search,
            Category = category,
            Priority = priority,
            ReviewState = reviewState,
            MinConfidence = minConfidence,
            MaxConfidence = maxConfidence
        };

        var results = await _recommendationService.GetRecommendationsAsync(query, cancellationToken);
        return Ok(results);
    }

    /// <summary>
    /// Retrieves aggregated metrics for AI recommendations.
    /// </summary>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(RecommendationMetricsResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMetrics(CancellationToken cancellationToken)
    {
        var metrics = await _recommendationService.GetMetricsAsync(cancellationToken);
        return Ok(metrics);
    }

    /// <summary>
    /// Retrieves a single AI recommendation by identifier.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(RecommendationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        var recommendation = await _recommendationService.GetRecommendationByIdAsync(id, cancellationToken);
        if (recommendation == null)
        {
            return NotFound(new { error = $"Recommendation '{id}' was not found." });
        }

        return Ok(recommendation);
    }

    /// <summary>
    /// Records an authorized human review decision for an AI recommendation (Approved, Modified, Rejected, Deferred).
    /// </summary>
    [HttpPost("{id}/decision")]
    [ProducesResponseType(typeof(RecommendationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordDecision(
        string id,
        [FromBody] RecordDecisionRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var validDecisions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Approved", "Modified", "Rejected", "Deferred"
        };

        if (!validDecisions.Contains(request.Decision))
        {
            return BadRequest(new { error = $"Decision must be one of: {string.Join(", ", validDecisions)}." });
        }

        var updated = await _recommendationService.RecordDecisionAsync(id, request, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { error = $"Recommendation '{id}' was not found." });
        }

        _logger.LogInformation("Human decision recorded for recommendation {Id}: {Decision} by {Reviewer}",
            id, request.Decision, request.ReviewedBy ?? "Emergency Coordinator");

        return Ok(updated);
    }

    /// <summary>
    /// Generates a new AI recommendation based on disaster request attributes.
    /// </summary>
    [HttpPost("generate")]
    [ProducesResponseType(typeof(RecommendationResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Generate(
        [FromBody] GenerateRecommendationRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _recommendationService.GenerateRecommendationAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Resets recommendations and metrics back to initial seed state.
    /// </summary>
    [HttpPost("reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Reset(CancellationToken cancellationToken)
    {
        await _recommendationService.ResetDataAsync(cancellationToken);
        var metrics = await _recommendationService.GetMetricsAsync(cancellationToken);
        return Ok(new { message = "Recommendations reset to default state.", metrics });
    }
}
