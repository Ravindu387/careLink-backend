namespace CareLink.Application.DTOs;

public class RecommendationQueryParameters
{
    public string? Search { get; set; }
    public string? Category { get; set; }
    public string? Priority { get; set; }
    public string? ReviewState { get; set; }
    public int? MinConfidence { get; set; }
    public int? MaxConfidence { get; set; }
}
