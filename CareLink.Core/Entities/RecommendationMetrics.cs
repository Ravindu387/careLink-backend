namespace CareLink.Core.Entities;

public class RecommendationMetrics
{
    public int TotalRecommendations { get; set; }
    public int AwaitingReview { get; set; }
    public int HighUrgency { get; set; }
    public double AcceptanceRate { get; set; }
    public double OverrideRate { get; set; }
    public int TotalReviewed { get; set; }
    public int HistoricalReviewedCount { get; set; }
    public int HistoricalOverridesCount { get; set; }
}
