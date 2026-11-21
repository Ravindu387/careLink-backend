namespace CareLink.Application.DTOs.AiRecommendations;

public class AiRecommendationMetricsDto
{
    public int TotalRecommendations { get; set; }
    public int AwaitingReviewCount { get; set; }
    public int ApprovedCount { get; set; }
    public int ModifiedCount { get; set; }
    public int DeferredCount { get; set; }
    public double AverageConfidence { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
