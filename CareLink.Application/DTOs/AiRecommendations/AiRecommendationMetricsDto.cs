namespace CareLink.Application.DTOs.AiRecommendations;

public class AiRecommendationMetricsDto
{
    public int TotalRecommendations { get; set; }
    public int TotalCases
    {
        get => TotalRecommendations;
        set => TotalRecommendations = value;
    }

    public int AwaitingReviewCount { get; set; }
    public int ApprovedCount { get; set; }
    public int ModifiedCount { get; set; }
    public int DeferredCount { get; set; }
    public int RejectedCount { get; set; }

    public double AverageConfidence { get; set; }
    public double AverageConfidencePercentage
    {
        get => AverageConfidence;
        set => AverageConfidence = value;
    }

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
