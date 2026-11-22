using CareLink.Application.Interfaces;
using CareLink.Core.Entities;
using CareLink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CareLink.Infrastructure.Repositories;

public class AiRecommendationRepository : IAiRecommendationRepository
{
    private readonly ApplicationDbContext _context;

    public AiRecommendationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AiRecommendation?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        return await _context.AiRecommendations
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id.Trim(), cancellationToken);
    }

    public async Task<AiRecommendation?> GetByRequestIdAsync(string requestId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(requestId)) return null;
        return await _context.AiRecommendations
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.RequestId == requestId.Trim(), cancellationToken);
    }

    public async Task<IReadOnlyList<AiRecommendation>> GetAllAsync(
        string? status = null,
        string? priority = null,
        string? category = null,
        string? crisisId = null,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AiRecommendations.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && !status.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(r => r.ReviewState == status);
        }

        if (!string.IsNullOrWhiteSpace(priority) && !priority.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(r => r.RecommendedPriority == priority || r.DecidedPriority == priority);
        }

        if (!string.IsNullOrWhiteSpace(category) && !category.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(r => r.Category == category);
        }

        if (!string.IsNullOrWhiteSpace(crisisId) && !crisisId.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(r => r.CrisisId == crisisId);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(r => r.Id.Contains(s)
                                     || r.Title.Contains(s)
                                     || (r.RequestId != null && r.RequestId.Contains(s))
                                     || r.Location.Contains(s)
                                     || r.Explanation.Contains(s));
        }

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<AiRecommendation> AddAsync(AiRecommendation recommendation, CancellationToken cancellationToken = default)
    {
        recommendation.CreatedAt = DateTime.UtcNow;
        await _context.AiRecommendations.AddAsync(recommendation, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return recommendation;
    }

    public async Task<AiRecommendation> UpdateAsync(AiRecommendation recommendation, CancellationToken cancellationToken = default)
    {
        recommendation.UpdatedAt = DateTime.UtcNow;
        _context.AiRecommendations.Update(recommendation);
        await _context.SaveChangesAsync(cancellationToken);
        return recommendation;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.AiRecommendations.FindAsync(new object[] { id }, cancellationToken);
        if (entity == null) return false;

        _context.AiRecommendations.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
