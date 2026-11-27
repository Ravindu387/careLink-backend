using CareLink.Application.DTOs.AiRecommendations;
using CareLink.Core.Entities;

namespace CareLink.Tests.Fixtures;

public static class AiTestDataBuilder
{
    public static AiRecommendation CreateRecommendationEntity(
        string id = "AI-TEST-001",
        string crisisId = "CR-2025-018",
        string category = "CrisisAnalysis",
        string priority = "Critical",
        string reviewState = "Awaiting Review")
    {
        return new AiRecommendation
        {
            Id = id,
            CrisisId = crisisId,
            CrisisName = "Monsoon Floods 2025",
            RequestId = "REQ-TEST-101",
            Category = category,
            Title = $"Test Assessment · {crisisId}",
            Summary = "Test summary for disaster response validation.",
            Description = "Automated test incident telemetry.",
            Location = "North Ward Sector 1",
            RecommendedPriority = priority,
            Confidence = 92,
            Explanation = "Grounded indicators demonstrate critical threat.",
            RecommendedAction = "Deploy Immediate Relief Unit",
            ModelName = "gemini-1.5-flash-test",
            ModelTimestamp = "10:00 UTC",
            ReviewState = reviewState,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static AiRecommendationDetailDto CreateDetailDto(
        string id = "AI-TEST-002",
        string crisisId = "CR-2025-018",
        string priority = "High",
        string reviewState = "Awaiting Review")
    {
        return new AiRecommendationDetailDto
        {
            Id = id,
            CrisisId = crisisId,
            CrisisName = "Monsoon Floods 2025",
            RequestId = "REQ-TEST-102",
            Category = "ResourceAllocation",
            Title = $"Resource Allocation · {crisisId}",
            Summary = "Optimized dispatch testing.",
            RequestCategory = "Food & Water",
            ReportedUrgency = "Urgent",
            Description = "Test household support request.",
            Location = "Riverside Zone 2",
            VulnerabilityFactors = "Elderly occupants",
            AffectedPeople = 4,
            RecommendedPriority = priority,
            Confidence = 88,
            Explanation = "Adequate stock confirmed at central depot.",
            RecommendedAction = "Dispatch 4 cartons Water 10L",
            ReviewState = reviewState,
            ImmediatePriorities = ["Immediate water distribution", "Check levee status"],
            Risks = ["Road flooding"],
            Assumptions = ["Depot open"],
            MissingInformation = ["Exact house number"],
            ResourceRecommendations =
            [
                new()
                {
                    ResourceName = "Water - 10L",
                    Category = "Food & Water",
                    RecommendedQuantity = 4,
                    AvailableQuantity = 8420,
                    IsShortage = false,
                    ShortageQuantity = 0,
                    TargetCrisisLocation = "Riverside Zone 2",
                    Priority = "High",
                    Rationale = "Potable hydration"
                }
            ],
            ShelterRecommendations =
            [
                new()
                {
                    ShelterName = "North Community Hall",
                    Location = "North Ward",
                    Capacity = 500,
                    CurrentOccupancy = 250,
                    RemainingCapacity = 250,
                    RecommendedEvacueeCount = 4,
                    SuitabilityReason = "Available beds and ramp access",
                    SafetyAndAccessibilityNotes = "Accessible"
                }
            ],
            VolunteerRecommendations =
            [
                new()
                {
                    SkillGroup = "Search & Rescue",
                    RecommendedCount = 2,
                    SuggestedTask = "Boat evacuation support",
                    Priority = "Critical",
                    Rationale = "Water level high"
                }
            ],
            CreatedAt = DateTime.UtcNow
        };
    }
}
