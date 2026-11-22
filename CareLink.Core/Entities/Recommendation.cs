namespace CareLink.Core.Entities;

public class Recommendation
{
    public string Id { get; set; } = string.Empty;
    public string RequestId { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public int Confidence { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public string Alternative { get; set; } = string.Empty;
    public string ReviewState { get; set; } = "Awaiting Review";
    public string? Decision { get; set; }
    public string? Rationale { get; set; }
    public string? ReviewedBy { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
