using CareLink.Application.Common;
using CareLink.Application.DTOs.AiRecommendations;

namespace CareLink.Application.Interfaces;

public interface IAiRecommendationService
{
    /// <summary>
    /// Retrieve all AI recommendations with optional status, priority, and search filters.
    /// </summary>
    Task<ApiResponse<IReadOnlyList<AiRecommendationSummaryDto>>> GetRecommendationsAsync(
        string? status = null,
        string? priority = null,
        string? search = null);

    /// <summary>
    /// Retrieve full recommendation review details by its unique identifier (e.g. AI-9041).
    /// </summary>
    Task<ApiResponse<AiRecommendationDetailDto>> GetRecommendationByIdAsync(string id);

    /// <summary>
    /// Retrieve full recommendation review details by the associated assistance request identifier (e.g. REQ-2841).
    /// </summary>
    Task<ApiResponse<AiRecommendationDetailDto>> GetRecommendationByRequestIdAsync(string requestId);

    /// <summary>
    /// Retrieve aggregated metrics including awaiting review count, approvals, modifications, and average confidence.
    /// </summary>
    Task<ApiResponse<AiRecommendationMetricsDto>> GetMetricsAsync();

    /// <summary>
    /// Record an authorized human decision (Approve or Modify with explicit rationale and priority).
    /// </summary>
    Task<ApiResponse<AiRecommendationDetailDto>> RecordDecisionAsync(
        string id,
        RecordDecisionRequestDto request,
        string reviewerName);

    /// <summary>
    /// Defer review of an AI recommendation for subsequent evaluation.
    /// </summary>
    Task<ApiResponse<AiRecommendationDetailDto>> DeferRecommendationAsync(
        string id,
        DeferRecommendationRequestDto? request,
        string reviewerName);

    /// <summary>
    /// Reset an AI recommendation decision back to awaiting review status.
    /// </summary>
    Task<ApiResponse<AiRecommendationDetailDto>> ResetRecommendationAsync(string id);
}
