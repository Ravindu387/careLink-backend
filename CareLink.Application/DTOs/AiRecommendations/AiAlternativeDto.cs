namespace CareLink.Application.DTOs.AiRecommendations;

public class AiAlternativeDto
{
    public string Priority { get; set; } = string.Empty;
    public int LikelihoodPercentage { get; set; }
}
