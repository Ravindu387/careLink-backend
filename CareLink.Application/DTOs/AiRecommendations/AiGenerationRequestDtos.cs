namespace CareLink.Application.DTOs.AiRecommendations;

public class GenerateCrisisAnalysisRequestDto
{
    public string CrisisId { get; set; } = "CR-2025-018";
    public string? CrisisName { get; set; } = "Monsoon Floods 2025";
    public string? FocusArea { get; set; }
    public string? AdditionalContext { get; set; }
}

public class GenerateResourceRecommendationRequestDto
{
    public string CrisisId { get; set; } = "CR-2025-018";
    public string? CrisisName { get; set; } = "Monsoon Floods 2025";
    public string? TargetLocation { get; set; }
    public string? Urgency { get; set; }
    public string? AdditionalContext { get; set; }
}

public class GenerateShelterRecommendationRequestDto
{
    public string CrisisId { get; set; } = "CR-2025-018";
    public string? CrisisName { get; set; } = "Monsoon Floods 2025";
    public int? EvacueesCount { get; set; }
    public bool NeedsAccessible { get; set; } = true;
    public string? AdditionalContext { get; set; }
}

public class GenerateVolunteerRecommendationRequestDto
{
    public string CrisisId { get; set; } = "CR-2025-018";
    public string? CrisisName { get; set; } = "Monsoon Floods 2025";
    public string? SkillFilter { get; set; }
    public int? TargetVolunteerCount { get; set; }
    public string? AdditionalContext { get; set; }
}

public class GenerateResponsePlanRequestDto
{
    public string CrisisId { get; set; } = "CR-2025-018";
    public string? CrisisName { get; set; } = "Monsoon Floods 2025";
    public string? OperationalContext { get; set; }
}

public class AiResourcePlanItemDto
{
    public string ResourceName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int RecommendedQuantity { get; set; }
    public int AvailableQuantity { get; set; }
    public bool IsShortage { get; set; }
    public int ShortageQuantity { get; set; }
    public string TargetCrisisLocation { get; set; } = string.Empty;
    public string Priority { get; set; } = "High";
    public string Rationale { get; set; } = string.Empty;
    public string AlternativeResource { get; set; } = string.Empty;
}

public class AiShelterPlanItemDto
{
    public string ShelterName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int CurrentOccupancy { get; set; }
    public int RemainingCapacity { get; set; }
    public int RecommendedEvacueeCount { get; set; }
    public string SuitabilityReason { get; set; } = string.Empty;
    public string SafetyAndAccessibilityNotes { get; set; } = string.Empty;
}

public class AiVolunteerPlanItemDto
{
    public string SkillGroup { get; set; } = string.Empty;
    public int RecommendedCount { get; set; }
    public string SuggestedTask { get; set; } = string.Empty;
    public string Priority { get; set; } = "High";
    public string Rationale { get; set; } = string.Empty;
}

public class RejectRecommendationRequestDto
{
    public string RejectionReason { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class ApproveRecommendationRequestDto
{
    public string? DecidedPriority { get; set; }
    public string? Notes { get; set; }
}

public class GeminiAnalysisResponse
{
    public string Summary { get; set; } = string.Empty;
    public string RecommendedPriority { get; set; } = "High";
    public int Confidence { get; set; } = 85;
    public string Explanation { get; set; } = string.Empty;
    public string RecommendedAction { get; set; } = string.Empty;
    public List<string> ImmediatePriorities { get; set; } = [];
    public List<string> Risks { get; set; } = [];
    public List<string> Assumptions { get; set; } = [];
    public List<string> MissingInformation { get; set; } = [];
}

public class GeminiResourceResponse
{
    public string Summary { get; set; } = string.Empty;
    public string RecommendedPriority { get; set; } = "High";
    public int Confidence { get; set; } = 85;
    public string Explanation { get; set; } = string.Empty;
    public string RecommendedAction { get; set; } = string.Empty;
    public List<string> ImmediatePriorities { get; set; } = [];
    public List<string> Risks { get; set; } = [];
    public List<string> Assumptions { get; set; } = [];
    public List<string> MissingInformation { get; set; } = [];
    public List<AiResourcePlanItemDto> ResourcePlan { get; set; } = [];
}
