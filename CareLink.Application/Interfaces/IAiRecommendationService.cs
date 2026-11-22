using CareLink.Application.Common;
using CareLink.Application.DTOs.AiRecommendations;

namespace CareLink.Application.Interfaces;

public interface IAiRecommendationService
{
    /// <summary>
    /// Retrieve all AI recommendations with optional status, priority, category, crisis, and search filters.
    /// </summary>
    Task<ApiResponse<IReadOnlyList<AiRecommendationSummaryDto>>> GetRecommendationsAsync(
        string? status = null,
        string? priority = null,
        string? category = null,
        string? crisisId = null,
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
    /// Retrieve aggregated metrics including awaiting review count, approvals, modifications, rejections, and average confidence.
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
    /// Approve an AI recommendation with optional comments and confirmed priority.
    /// </summary>
    Task<ApiResponse<AiRecommendationDetailDto>> ApproveRecommendationAsync(
        string id,
        ApproveRecommendationRequestDto? request,
        string reviewerName);

    /// <summary>
    /// Reject an AI recommendation with mandatory rationale.
    /// </summary>
    Task<ApiResponse<AiRecommendationDetailDto>> RejectRecommendationAsync(
        string id,
        RejectRecommendationRequestDto request,
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

    /// <summary>
    /// Generate an AI crisis situational analysis using Google Gemini and database operational context.
    /// </summary>
    Task<ApiResponse<AiRecommendationDetailDto>> GenerateCrisisAnalysisAsync(
        string crisisId,
        GenerateCrisisAnalysisRequestDto request,
        string requestedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate AI resource allocation recommendations respecting real inventory and identifying shortages.
    /// </summary>
    Task<ApiResponse<AiRecommendationDetailDto>> GenerateResourceRecommendationsAsync(
        string crisisId,
        GenerateResourceRecommendationRequestDto request,
        string requestedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate AI shelter matching recommendations based on actual shelter capacity and special requirements.
    /// </summary>
    Task<ApiResponse<AiRecommendationDetailDto>> GenerateShelterRecommendationsAsync(
        string crisisId,
        GenerateShelterRecommendationRequestDto request,
        string requestedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate AI volunteer matching recommendations considering skills, radius, and availability.
    /// </summary>
    Task<ApiResponse<AiRecommendationDetailDto>> GenerateVolunteerRecommendationsAsync(
        string crisisId,
        GenerateVolunteerRecommendationRequestDto request,
        string requestedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate a comprehensive multi-entity response plan combining crisis, resource, shelter, and volunteer allocations.
    /// </summary>
    Task<ApiResponse<AiRecommendationDetailDto>> GenerateResponsePlanAsync(
        string crisisId,
        GenerateResponsePlanRequestDto request,
        string requestedBy,
        CancellationToken cancellationToken = default);
}
