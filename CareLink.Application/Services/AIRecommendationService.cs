using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;

namespace CareLink.Application.Services;

public class AIRecommendationService : IAIRecommendationService
{
    private readonly IRecommendationRepository _repository;
    private readonly IAIInferenceEngine _inferenceEngine;

    public AIRecommendationService(
        IRecommendationRepository repository,
        IAIInferenceEngine inferenceEngine)
    {
        _repository = repository;
        _inferenceEngine = inferenceEngine;
    }

    public async Task<IReadOnlyList<RecommendationResponseDto>> GetRecommendationsAsync(
        RecommendationQueryParameters query,
        CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetAllAsync(
            search: query.Search,
            category: query.Category,
            priority: query.Priority,
            reviewState: query.ReviewState,
            minConfidence: query.MinConfidence,
            maxConfidence: query.MaxConfidence,
            cancellationToken: cancellationToken);

        return entities.Select(MapToDto).ToList();
    }

    public async Task<RecommendationResponseDto?> GetRecommendationByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<RecommendationResponseDto?> RecordDecisionAsync(
        string id,
        RecordDecisionRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
        {
            return null;
        }

        entity.ReviewState = request.Decision;
        entity.Decision = request.Decision;
        entity.Rationale = request.Rationale;
        entity.ReviewedBy = string.IsNullOrWhiteSpace(request.ReviewedBy)
            ? "Emergency Coordinator"
            : request.ReviewedBy.Trim();
        entity.ReviewedAt = DateTimeOffset.UtcNow;

        var updated = await _repository.UpdateAsync(entity, cancellationToken);
        return updated == null ? null : MapToDto(updated);
    }

    public async Task<RecommendationResponseDto> GenerateRecommendationAsync(
        GenerateRecommendationRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var recommendation = _inferenceEngine.InferRecommendation(request);
        var created = await _repository.AddAsync(recommendation, cancellationToken);
        return MapToDto(created);
    }

    public async Task<RecommendationMetricsResponseDto> GetMetricsAsync(
        CancellationToken cancellationToken = default)
    {
        var metrics = await _repository.GetMetricsAsync(cancellationToken);
        return new RecommendationMetricsResponseDto
        {
            AwaitingReview = metrics.AwaitingReview,
            HighUrgency = metrics.HighUrgency,
            AcceptanceRate = metrics.AcceptanceRate,
            OverrideRate = metrics.OverrideRate,
            TotalReviewed = metrics.TotalReviewed,
            TotalRecommendations = metrics.TotalRecommendations
        };
    }

    public async Task ResetDataAsync(CancellationToken cancellationToken = default)
    {
        await _repository.ResetToDefaultAsync(cancellationToken);
    }

    private static RecommendationResponseDto MapToDto(Recommendation entity)
    {
        return new RecommendationResponseDto
        {
            Id = entity.Id,
            Request = entity.RequestId,
            Category = entity.Category,
            Priority = entity.Priority,
            Confidence = entity.Confidence,
            Action = entity.Action,
            Timestamp = entity.CreatedAt.ToLocalTime().ToString("HH:mm"),
            ReviewState = entity.ReviewState,
            Explanation = entity.Explanation,
            Alternative = entity.Alternative,
            Decision = entity.Decision,
            Rationale = entity.Rationale,
            ReviewedBy = entity.ReviewedBy,
            ReviewedAt = entity.ReviewedAt,
            CreatedAt = entity.CreatedAt
        };
    }
}
