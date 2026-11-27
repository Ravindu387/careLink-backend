namespace CareLink.Application.DTOs.AiRecommendations;

public class AiRecommendationSummaryDto
{
    public string Id { get; set; } = string.Empty;
    public string? CrisisId { get; set; }
    public string? CrisisName { get; set; }
    public string RequestId { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string RecommendedPriority { get; set; } = string.Empty;
    public int Confidence { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Timestamp { get; set; } = string.Empty;
    public string ReviewState { get; set; } = "Awaiting Review";
    public string Explanation { get; set; } = string.Empty;
    public string Alternative { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? DecidedPriority { get; set; }
    public string? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public bool IsLiveAiGenerated { get; set; } = false;
}
