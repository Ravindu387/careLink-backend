using CareLink.Application.DTOs;

namespace CareLink.Application.Interfaces;

public interface IAIRecommendationService
{
    Task<IReadOnlyList<RecommendationResponseDto>> GetRecommendationsAsync(
        RecommendationQueryParameters query,
        CancellationToken cancellationToken = default);

    Task<RecommendationResponseDto?> GetRecommendationByIdAsync(
        string id,
        CancellationToken cancellationToken = default);

    Task<RecommendationResponseDto?> RecordDecisionAsync(
        string id,
        RecordDecisionRequestDto request,
        CancellationToken cancellationToken = default);

    Task<RecommendationResponseDto> GenerateRecommendationAsync(
        GenerateRecommendationRequestDto request,
        CancellationToken cancellationToken = default);

    Task<RecommendationMetricsResponseDto> GetMetricsAsync(
        CancellationToken cancellationToken = default);

    Task ResetDataAsync(CancellationToken cancellationToken = default);
}
