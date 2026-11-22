using System.Collections.Concurrent;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;

namespace CareLink.Infrastructure.Repositories;

public class InMemoryRecommendationRepository : IRecommendationRepository
{
    private readonly ConcurrentDictionary<string, Recommendation> _recommendations = new();
    private const int BaselineHistoricalReviewed = 1498;
    private const int BaselineHistoricalApproved = 1372; // ~91.6%
    private const int BaselineHistoricalOverrides = 126; // ~8.4%

    public InMemoryRecommendationRepository()
    {
        SeedDefaultData();
    }

    public Task<IReadOnlyList<Recommendation>> GetAllAsync(
        string? search = null,
        string? category = null,
        string? priority = null,
        string? reviewState = null,
        int? minConfidence = null,
        int? maxConfidence = null,
        CancellationToken cancellationToken = default)
    {
        var query = _recommendations.Values.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(r =>
                r.Id.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                r.RequestId.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                r.Category.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                r.Action.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                r.Explanation.Contains(s, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(category) && !category.Equals("All recommendation types", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(r => r.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(priority) && !priority.Equals("All priorities", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(r => r.Priority.Equals(priority, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(reviewState) && !reviewState.Equals("Review state", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(r => r.ReviewState.Equals(reviewState, StringComparison.OrdinalIgnoreCase));
        }

        if (minConfidence.HasValue)
        {
            query = query.Where(r => r.Confidence >= minConfidence.Value);
        }

        if (maxConfidence.HasValue)
        {
            query = query.Where(r => r.Confidence <= maxConfidence.Value);
        }

        var list = query
            .OrderByDescending(r => r.CreatedAt)
            .ToList();

        return Task.FromResult<IReadOnlyList<Recommendation>>(list);
    }

    public Task<Recommendation?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        _recommendations.TryGetValue(id, out var rec);
        return Task.FromResult(rec);
    }

    public Task<Recommendation> AddAsync(Recommendation recommendation, CancellationToken cancellationToken = default)
    {
        _recommendations[recommendation.Id] = recommendation;
        return Task.FromResult(recommendation);
    }

    public Task<Recommendation?> UpdateAsync(Recommendation recommendation, CancellationToken cancellationToken = default)
    {
        if (!_recommendations.ContainsKey(recommendation.Id))
        {
            return Task.FromResult<Recommendation?>(null);
        }

        _recommendations[recommendation.Id] = recommendation;
        return Task.FromResult<Recommendation?>(recommendation);
    }

    public Task<RecommendationMetrics> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        var items = _recommendations.Values.ToList();
        var awaitingCount = items.Count(r => r.ReviewState.Equals("Awaiting Review", StringComparison.OrdinalIgnoreCase));
        var highUrgencyCount = items.Count(r =>
            r.Priority.Equals("Critical", StringComparison.OrdinalIgnoreCase) ||
            r.Priority.Equals("High", StringComparison.OrdinalIgnoreCase));

        // Dynamically compute acceptance and override rates
        var liveApproved = items.Count(r => r.ReviewState.Equals("Approved", StringComparison.OrdinalIgnoreCase));
        var liveOverrides = items.Count(r =>
            r.ReviewState.Equals("Modified", StringComparison.OrdinalIgnoreCase) ||
            r.ReviewState.Equals("Rejected", StringComparison.OrdinalIgnoreCase));
        var liveTotalReviewed = liveApproved + liveOverrides + items.Count(r => r.ReviewState.Equals("Deferred", StringComparison.OrdinalIgnoreCase));

        var totalReviewed = BaselineHistoricalReviewed + liveTotalReviewed;
        var totalApproved = BaselineHistoricalApproved + liveApproved;
        var totalOverrides = BaselineHistoricalOverrides + liveOverrides;

        var acceptanceRate = totalReviewed > 0
            ? Math.Round((double)totalApproved / totalReviewed * 100.0, 1)
            : 0.0;

        var overrideRate = totalReviewed > 0
            ? Math.Round((double)totalOverrides / totalReviewed * 100.0, 1)
            : 0.0;

        var metrics = new RecommendationMetrics
        {
            TotalRecommendations = items.Count,
            AwaitingReview = awaitingCount,
            HighUrgency = highUrgencyCount,
            AcceptanceRate = acceptanceRate,
            OverrideRate = overrideRate,
            TotalReviewed = totalReviewed,
            HistoricalReviewedCount = BaselineHistoricalReviewed,
            HistoricalOverridesCount = BaselineHistoricalOverrides
        };

        return Task.FromResult(metrics);
    }

    public Task ResetToDefaultAsync(CancellationToken cancellationToken = default)
    {
        _recommendations.Clear();
        SeedDefaultData();
        return Task.CompletedTask;
    }

    private void SeedDefaultData()
    {
        var baseTime = DateTimeOffset.UtcNow;

        var items = new List<Recommendation>
        {
            new()
            {
                Id = "AI-9041",
                RequestId = "REQ-2841",
                Category = "Emergency shelter",
                Priority = "Critical",
                Confidence = 94,
                Action = "Evacuation + accessible shelter",
                ReviewState = "Awaiting Review",
                Explanation = "Immediate risk to life is indicated by active flooding combined with vulnerable household members and limited self-evacuation capability.",
                Alternative = "High priority – 5% likelihood",
                CreatedAt = baseTime.AddMinutes(-12)
            },
            new()
            {
                Id = "AI-9038",
                RequestId = "REQ-2839",
                Category = "Medical supplies",
                Priority = "High",
                Confidence = 91,
                Action = "24 trauma kits · Central Depot",
                ReviewState = "Awaiting Review",
                Explanation = "Medical supply demand is high and the nearest available medical inventory can support the request.",
                Alternative = "Medium priority – 8% likelihood",
                CreatedAt = baseTime.AddMinutes(-31)
            },
            new()
            {
                Id = "AI-9032",
                RequestId = "REQ-2836",
                Category = "Food & water",
                Priority = "High",
                Confidence = 87,
                Action = "3 family packs · Eastbank Hub",
                ReviewState = "Modified",
                Decision = "Modified",
                Rationale = "Adjusted ration pack count to cover extended family sheltering together.",
                ReviewedBy = "Emergency Coordinator",
                ReviewedAt = baseTime.AddMinutes(-40),
                Explanation = "The affected household requires immediate food and drinking water assistance.",
                Alternative = "Medium priority – 12% likelihood",
                CreatedAt = baseTime.AddMinutes(-55)
            },
            new()
            {
                Id = "AI-9024",
                RequestId = "REQ-2828",
                Category = "Accessible transport",
                Priority = "High",
                Confidence = 82,
                Action = "Mobility Vehicle 4",
                ReviewState = "Approved",
                Decision = "Approved",
                Rationale = "Verified with disability service coordinator; vehicle dispatched.",
                ReviewedBy = "Emergency Coordinator",
                ReviewedAt = baseTime.AddMinutes(-65),
                Explanation = "A mobility vehicle is recommended based on the applicant's accessibility requirements.",
                Alternative = "Medium priority – 15% likelihood",
                CreatedAt = baseTime.AddMinutes(-78)
            },
            new()
            {
                Id = "AI-9019",
                RequestId = "REQ-2821",
                Category = "Rescue assistance",
                Priority = "Critical",
                Confidence = 96,
                Action = "Deploy Rescue Unit 2",
                ReviewState = "Awaiting Review",
                Explanation = "High urgency rescue conditions require immediate human assessment and emergency response.",
                Alternative = "High priority – 3% likelihood",
                CreatedAt = baseTime.AddMinutes(-101)
            },
            new()
            {
                Id = "AI-9015",
                RequestId = "REQ-2815",
                Category = "Emergency shelter",
                Priority = "Critical",
                Confidence = 93,
                Action = "Shelter Unit 7 · North Sector Hub",
                ReviewState = "Awaiting Review",
                Explanation = "Rising flood waters compromised ground floor of residential complex housing multi-family unit.",
                Alternative = "High priority – 7% likelihood",
                CreatedAt = baseTime.AddHours(-2).AddMinutes(-10)
            },
            new()
            {
                Id = "AI-9011",
                RequestId = "REQ-2808",
                Category = "Medical supplies",
                Priority = "High",
                Confidence = 89,
                Action = "Insulin & cold-chain portable storage",
                ReviewState = "Awaiting Review",
                Explanation = "Critical chronic illness patient without power supply for refrigeration of life-saving medicine.",
                Alternative = "Critical priority – 11% likelihood",
                CreatedAt = baseTime.AddHours(-2).AddMinutes(-45)
            },
            new()
            {
                Id = "AI-9008",
                RequestId = "REQ-2799",
                Category = "Rescue assistance",
                Priority = "Critical",
                Confidence = 95,
                Action = "Deploy Swift Water Rescue Boat 1",
                ReviewState = "Awaiting Review",
                Explanation = "Multiple individuals isolated on elevated roadway embankment surrounded by fast-moving surge.",
                Alternative = "High priority – 4% likelihood",
                CreatedAt = baseTime.AddHours(-3).AddMinutes(-15)
            },
            new()
            {
                Id = "AI-9005",
                RequestId = "REQ-2792",
                Category = "Food & water",
                Priority = "High",
                Confidence = 85,
                Action = "Bulk clean water delivery (2000L tanker)",
                ReviewState = "Awaiting Review",
                Explanation = "Community water supply contamination reported in Sector 3 relief camp.",
                Alternative = "Medium priority – 14% likelihood",
                CreatedAt = baseTime.AddHours(-3).AddMinutes(-50)
            },
            new()
            {
                Id = "AI-9002",
                RequestId = "REQ-2785",
                Category = "Sanitation & Hygiene",
                Priority = "Medium",
                Confidence = 84,
                Action = "50 Hygiene kits + Water purification tablets",
                ReviewState = "Awaiting Review",
                Explanation = "Preventative sanitation supplies needed to mitigate waterborne vector risk following storm surge.",
                Alternative = "Low priority – 16% likelihood",
                CreatedAt = baseTime.AddHours(-4).AddMinutes(-20)
            }
        };

        foreach (var item in items)
        {
            _recommendations[item.Id] = item;
        }
    }
}
