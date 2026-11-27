using CareLink.Core.Entities;

namespace CareLink.Application.Interfaces;

public interface IAiRecommendationRepository
{
    Task<AiRecommendation?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<AiRecommendation?> GetByRequestIdAsync(string requestId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AiRecommendation>> GetAllAsync(
        string? status = null,
        string? priority = null,
        string? category = null,
        string? crisisId = null,
        string? search = null,
        CancellationToken cancellationToken = default);
    Task<AiRecommendation> AddAsync(AiRecommendation recommendation, CancellationToken cancellationToken = default);
    Task<AiRecommendation> UpdateAsync(AiRecommendation recommendation, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
