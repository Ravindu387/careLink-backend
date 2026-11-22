namespace CareLink.Application.DTOs;

public class RecommendationMetricsResponseDto
{
    public int AwaitingReview { get; set; }
    public int HighUrgency { get; set; }
    public double AcceptanceRate { get; set; }
    public double OverrideRate { get; set; }
    public int TotalReviewed { get; set; }
    public int TotalRecommendations { get; set; }
}
