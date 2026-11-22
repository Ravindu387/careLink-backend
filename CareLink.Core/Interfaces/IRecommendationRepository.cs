using CareLink.Core.Entities;

namespace CareLink.Core.Interfaces;

public interface IRecommendationRepository
{
    Task<IReadOnlyList<Recommendation>> GetAllAsync(
        string? search = null,
        string? category = null,
        string? priority = null,
        string? reviewState = null,
        int? minConfidence = null,
        int? maxConfidence = null,
        CancellationToken cancellationToken = default);

    Task<Recommendation?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<Recommendation> AddAsync(Recommendation recommendation, CancellationToken cancellationToken = default);

    Task<Recommendation?> UpdateAsync(Recommendation recommendation, CancellationToken cancellationToken = default);

    Task<RecommendationMetrics> GetMetricsAsync(CancellationToken cancellationToken = default);

    Task ResetToDefaultAsync(CancellationToken cancellationToken = default);
}
