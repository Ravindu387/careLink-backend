namespace CareLink.Application.DTOs.AiRecommendations;

public class AiRecommendationDetailDto
{
    // Recommendation Identity
    public string Id { get; set; } = string.Empty;
    public string RequestId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Associated Request Information
    public string RequestCategory { get; set; } = string.Empty;
    public string ReportedUrgency { get; set; } = "Reported urgent";
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string VulnerabilityFactors { get; set; } = string.Empty;
    public int AffectedPeople { get; set; }
    public string SubmittedTime { get; set; } = string.Empty;
    public string VerificationStatus { get; set; } = "Contact verified";
    public string EvidenceNote { get; set; } = string.Empty;
    public List<string> RelevantInformation { get; set; } = [];

    // AI Model Output
    public string ModelName { get; set; } = "Priority Assist v2.4";
    public string ModelTimestamp { get; set; } = string.Empty;
    public string RecommendedPriority { get; set; } = "Critical";
    public int Confidence { get; set; } = 94;
    public string Explanation { get; set; } = string.Empty;
    public string RecommendedAction { get; set; } = string.Empty;
    public List<AiAlternativeDto> Alternatives { get; set; } = [];
    public string WarningMessage { get; set; } =
        "This is decision support, not an automated decision. An authorised coordinator must approve or modify it.";

    // Human Review State
    public string ReviewState { get; set; } = "Awaiting Review"; // Awaiting Review | Approved | Modified | Deferred
    public string? DecidedPriority { get; set; }
    public string? DecisionRationale { get; set; }
    public string? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public bool IsDecisionRecorded { get; set; } = false;
}
