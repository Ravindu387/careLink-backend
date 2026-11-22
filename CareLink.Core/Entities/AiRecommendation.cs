namespace CareLink.Core.Entities;

public class AiRecommendation
{
    public string Id { get; set; } = string.Empty; // e.g. AI-9041
    public string? CrisisId { get; set; } // e.g. CR-2025-018
    public string? CrisisName { get; set; } = "Monsoon Floods 2025";
    public string? RequestId { get; set; } // e.g. REQ-2841
    public string Category { get; set; } = "CrisisAnalysis"; // CrisisAnalysis | ResourceAllocation | ShelterMatching | VolunteerAssignment | ResponsePlan | AssistanceTriage
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string RecommendedPriority { get; set; } = "High"; // Critical | High | Medium | Low
    public int Confidence { get; set; } = 85;
    public string Explanation { get; set; } = string.Empty;
    public string RecommendedAction { get; set; } = string.Empty;
    public string ModelName { get; set; } = "gemini-1.5-flash";
    public string ModelTimestamp { get; set; } = string.Empty;

    // Review & Human-in-the-loop workflow
    public string ReviewState { get; set; } = "Awaiting Review"; // Awaiting Review | Approved | Rejected | Deferred | Modified
    public string? DecidedPriority { get; set; }
    public string? DecisionRationale { get; set; }
    public string? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? RejectionReason { get; set; }
    public bool IsLiveAiGenerated { get; set; } = false;

    // Rich JSON payload for multi-entity structures (plans, allocations, shelters, volunteers, risks)
    public string? StructuredDataJson { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
