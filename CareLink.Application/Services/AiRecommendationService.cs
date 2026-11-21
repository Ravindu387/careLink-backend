using System.Collections.Concurrent;
using CareLink.Application.Common;
using CareLink.Application.DTOs.AiRecommendations;
using CareLink.Application.Interfaces;

namespace CareLink.Application.Services;

public class AiRecommendationService : IAiRecommendationService
{
    private readonly ConcurrentDictionary<string, AiRecommendationDetailDto> _recommendations = new(StringComparer.OrdinalIgnoreCase);

    public AiRecommendationService()
    {
        InitializeDefaultRecommendations();
    }

    private void InitializeDefaultRecommendations()
    {
        var seedItems = new List<AiRecommendationDetailDto>
        {
            new()
            {
                Id = "AI-9041",
                RequestId = "REQ-2841",
                CreatedAt = DateTime.UtcNow.AddMinutes(-12),
                RequestCategory = "Emergency shelter",
                ReportedUrgency = "Reported urgent",
                Description = "Household of six requires evacuation as flood water enters their residence.",
                Location = "Riverside, North Ward",
                VulnerabilityFactors = "Children, limited mobility",
                AffectedPeople = 6,
                SubmittedTime = "12 minutes ago",
                VerificationStatus = "Contact verified",
                EvidenceNote = "Citizen description and location boundary are verified. Image evidence is pending manual review.",
                RelevantInformation =
                [
                    "Flood water reported inside residence",
                    "6 affected people, including 2 children",
                    "1 household member needs mobility support",
                    "1.4 km from active evacuation zone"
                ],
                ModelName = "Priority Assist v2.4",
                ModelTimestamp = "13:01 UTC",
                RecommendedPriority = "Critical",
                Confidence = 94,
                Explanation = "Immediate risk to life is indicated by active inundation combined with vulnerable household members and limited self-evacuation capability.",
                RecommendedAction = "Evacuation + accessible shelter",
                Alternatives =
                [
                    new() { Priority = "High", LikelihoodPercentage = 5 },
                    new() { Priority = "Medium", LikelihoodPercentage = 1 }
                ],
                ReviewState = "Awaiting Review"
            },
            new()
            {
                Id = "AI-9038",
                RequestId = "REQ-2839",
                CreatedAt = DateTime.UtcNow.AddMinutes(-26),
                RequestCategory = "Medical supplies",
                ReportedUrgency = "High urgency",
                Description = "Urgent medical supplies needed for individuals isolated by flood waters in Sector 4.",
                Location = "Old Town, Sector 4",
                VulnerabilityFactors = "Elderly, chronic condition",
                AffectedPeople = 3,
                SubmittedTime = "26 minutes ago",
                VerificationStatus = "Contact verified",
                EvidenceNote = "Local health clinic confirmed supply shortages. Route accessible by high-clearance vehicle.",
                RelevantInformation =
                [
                    "3 isolated individuals requiring insulin and bandages",
                    "Power outage affecting local refrigeration",
                    "Nearest dispensary is 3.8 km away across flooded bridge"
                ],
                ModelName = "Priority Assist v2.4",
                ModelTimestamp = "12:42 UTC",
                RecommendedPriority = "High",
                Confidence = 91,
                Explanation = "Medical supply demand is high and the nearest available medical inventory can support the request.",
                RecommendedAction = "24 trauma kits · Central Depot dispatch",
                Alternatives =
                [
                    new() { Priority = "Medium", LikelihoodPercentage = 8 },
                    new() { Priority = "Critical", LikelihoodPercentage = 1 }
                ],
                ReviewState = "Awaiting Review"
            },
            new()
            {
                Id = "AI-9032",
                RequestId = "REQ-2836",
                CreatedAt = DateTime.UtcNow.AddMinutes(-41),
                RequestCategory = "Food & water",
                ReportedUrgency = "High urgency",
                Description = "Potable drinking water and non-perishable rations needed for stranded family.",
                Location = "Eastbank, Sector 2",
                VulnerabilityFactors = "Infants, expectant mother",
                AffectedPeople = 4,
                SubmittedTime = "41 minutes ago",
                VerificationStatus = "Contact verified",
                EvidenceNote = "Groundwater contamination reported in Eastbank ward. Public utility tap offline.",
                RelevantInformation =
                [
                    "Family of 4 without clean drinking water for >18 hours",
                    "Infant present requiring clean water for formula",
                    "Distribution hub located 800m away"
                ],
                ModelName = "Priority Assist v2.4",
                ModelTimestamp = "12:18 UTC",
                RecommendedPriority = "High",
                Confidence = 87,
                Explanation = "The affected household requires immediate food and clean drinking water assistance due to municipal contamination.",
                RecommendedAction = "3 family packs · Eastbank Hub",
                Alternatives =
                [
                    new() { Priority = "Medium", LikelihoodPercentage = 12 },
                    new() { Priority = "Low", LikelihoodPercentage = 1 }
                ],
                ReviewState = "Awaiting Review"
            },
            new()
            {
                Id = "AI-9024",
                RequestId = "REQ-2828",
                CreatedAt = DateTime.UtcNow.AddHours(-1),
                RequestCategory = "Accessible transport",
                ReportedUrgency = "Standard report",
                Description = "Wheelchair accessible evacuation required for senior resident from second-floor shelter.",
                Location = "Central Ward",
                VulnerabilityFactors = "Wheelchair dependent",
                AffectedPeople = 2,
                SubmittedTime = "1 hr ago",
                VerificationStatus = "Contact verified",
                EvidenceNote = "Pre-registered vulnerability profile matches citizen registry.",
                RelevantInformation =
                [
                    "Motorized wheelchair transport needed",
                    "Ground floor lobby is dry; stairlift is non-functional"
                ],
                ModelName = "Priority Assist v2.4",
                ModelTimestamp = "11:55 UTC",
                RecommendedPriority = "High",
                Confidence = 82,
                Explanation = "A mobility vehicle is recommended based on the applicant's accessibility requirements.",
                RecommendedAction = "Deploy Mobility Vehicle 4",
                Alternatives =
                [
                    new() { Priority = "Medium", LikelihoodPercentage = 15 },
                    new() { Priority = "Low", LikelihoodPercentage = 3 }
                ],
                ReviewState = "Awaiting Review"
            },
            new()
            {
                Id = "AI-9019",
                RequestId = "REQ-2821",
                CreatedAt = DateTime.UtcNow.AddHours(-1.5),
                RequestCategory = "Rescue assistance",
                ReportedUrgency = "Critical emergency",
                Description = "Two adults stranded on roof of single-story home surrounded by fast-flowing flood currents.",
                Location = "Lowland Basin, Zone 3",
                VulnerabilityFactors = "Immediate structural and drowning threat",
                AffectedPeople = 2,
                SubmittedTime = "1 hr 25 min ago",
                VerificationStatus = "Verified by emergency dispatcher",
                EvidenceNote = "Emergency 911 dispatch cross-referenced coordinates. Drone reconnaissance confirmed two occupants on roof.",
                RelevantInformation =
                [
                    "Water depth surrounding structure exceeds 1.8 meters",
                    "Current speed ~12 km/h",
                    "Rescue boat unit dispatched to staging area"
                ],
                ModelName = "Emergency Dispatch AI v3.1",
                ModelTimestamp = "11:32 UTC",
                RecommendedPriority = "Critical",
                Confidence = 96,
                Explanation = "High urgency rescue conditions require immediate human assessment and emergency response.",
                RecommendedAction = "Deploy Rescue Unit 2 (Zodiac Boat)",
                Alternatives =
                [
                    new() { Priority = "High", LikelihoodPercentage = 3 },
                    new() { Priority = "Medium", LikelihoodPercentage = 1 }
                ],
                ReviewState = "Awaiting Review"
            },
            new()
            {
                Id = "AI-9015",
                RequestId = "REQ-2815",
                CreatedAt = DateTime.UtcNow.AddHours(-2),
                RequestCategory = "Power & generator",
                ReportedUrgency = "Urgent",
                Description = "Backup generator fuel needed for community relief dispensary running medical refrigeration.",
                Location = "South Ward Community Centre",
                VulnerabilityFactors = "Refrigerated vaccine and insulin storage",
                AffectedPeople = 45,
                SubmittedTime = "2 hr ago",
                VerificationStatus = "Partner verified",
                EvidenceNote = "Dispensary manager confirmed remaining generator fuel at under 4 hours.",
                RelevantInformation =
                [
                    "450 doses of insulin at risk of thermal degradation",
                    "Fuel depot is 6 km away"
                ],
                ModelName = "Priority Assist v2.4",
                ModelTimestamp = "10:50 UTC",
                RecommendedPriority = "High",
                Confidence = 89,
                Explanation = "Cold chain maintenance critical for regional medical supplies.",
                RecommendedAction = "Dispatch 200L Diesel fuel tender",
                Alternatives =
                [
                    new() { Priority = "Critical", LikelihoodPercentage = 8 },
                    new() { Priority = "Medium", LikelihoodPercentage = 3 }
                ],
                ReviewState = "Awaiting Review"
            },
            new()
            {
                Id = "AI-9012",
                RequestId = "REQ-2809",
                CreatedAt = DateTime.UtcNow.AddHours(-3),
                RequestCategory = "Sanitation & hygiene",
                ReportedUrgency = "Standard report",
                Description = "Emergency sanitation units and water purification tablets for temporary community hub.",
                Location = "Highland School Evacuation Point",
                VulnerabilityFactors = "High concentration of evacuees",
                AffectedPeople = 120,
                SubmittedTime = "3 hr ago",
                VerificationStatus = "Verified by shelter manager",
                EvidenceNote = "Shelter population exceeded initial capacity by 40%.",
                RelevantInformation =
                [
                    "120 evacuees hosted on site",
                    "Current chemical toilet capacity insufficient"
                ],
                ModelName = "Priority Assist v2.4",
                ModelTimestamp = "09:45 UTC",
                RecommendedPriority = "Medium",
                Confidence = 85,
                Explanation = "Public health risk mitigation for crowded evacuation center.",
                RecommendedAction = "Deploy 4 Portable Sanitation Stations",
                Alternatives =
                [
                    new() { Priority = "High", LikelihoodPercentage = 10 },
                    new() { Priority = "Low", LikelihoodPercentage = 5 }
                ],
                ReviewState = "Awaiting Review"
            },
            new()
            {
                Id = "AI-9008",
                RequestId = "REQ-2804",
                CreatedAt = DateTime.UtcNow.AddHours(-4),
                RequestCategory = "Tarpaulins & temporary roofing",
                ReportedUrgency = "Standard report",
                Description = "Roof tiles displaced by gale-force winds causing rainwater leakage.",
                Location = "East Hillside",
                VulnerabilityFactors = "Elderly resident",
                AffectedPeople = 1,
                SubmittedTime = "4 hr ago",
                VerificationStatus = "Contact verified",
                EvidenceNote = "Weather radar indicates rain continuing for next 6 hours.",
                RelevantInformation =
                [
                    "Single occupant, house structurally sound",
                    "Rain entering master bedroom"
                ],
                ModelName = "Priority Assist v2.4",
                ModelTimestamp = "08:30 UTC",
                RecommendedPriority = "Medium",
                Confidence = 78,
                Explanation = "Property preservation and moisture mitigation.",
                RecommendedAction = "Deliver 2 Heavy-Duty Tarpaulins + tie-down kit",
                Alternatives =
                [
                    new() { Priority = "Low", LikelihoodPercentage = 20 },
                    new() { Priority = "High", LikelihoodPercentage = 2 }
                ],
                ReviewState = "Awaiting Review"
            }
        };

        foreach (var item in seedItems)
        {
            _recommendations[item.Id] = item;
        }
    }

    public Task<ApiResponse<IReadOnlyList<AiRecommendationSummaryDto>>> GetRecommendationsAsync(
        string? status = null,
        string? priority = null,
        string? search = null)
    {
        var query = _recommendations.Values.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(status) && !status.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(r => r.ReviewState.Equals(status, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(priority) && !priority.Equals("All", StringComparison.OrdinalIgnoreCase) && !priority.Equals("All priorities", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(r => r.RecommendedPriority.Equals(priority, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(r =>
                r.Id.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                r.RequestId.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                r.RequestCategory.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                r.Location.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                r.Description.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        var summaries = query
            .OrderByDescending(r => r.CreatedAt)
            .Select(MapToSummary)
            .ToList();

        return Task.FromResult(ApiResponse<IReadOnlyList<AiRecommendationSummaryDto>>.SuccessResult(
            summaries, "AI recommendations retrieved successfully."));
    }

    public Task<ApiResponse<AiRecommendationDetailDto>> GetRecommendationByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return Task.FromResult(ApiResponse<AiRecommendationDetailDto>.FailureResult("Recommendation ID is required."));
        }

        if (_recommendations.TryGetValue(id.Trim(), out var detail))
        {
            return Task.FromResult(ApiResponse<AiRecommendationDetailDto>.SuccessResult(detail));
        }

        return Task.FromResult(ApiResponse<AiRecommendationDetailDto>.FailureResult(
            $"AI Recommendation with ID '{id}' was not found."));
    }

    public Task<ApiResponse<AiRecommendationDetailDto>> GetRecommendationByRequestIdAsync(string requestId)
    {
        if (string.IsNullOrWhiteSpace(requestId))
        {
            return Task.FromResult(ApiResponse<AiRecommendationDetailDto>.FailureResult("Request ID is required."));
        }

        var match = _recommendations.Values.FirstOrDefault(r =>
            r.RequestId.Equals(requestId.Trim(), StringComparison.OrdinalIgnoreCase));

        if (match != null)
        {
            return Task.FromResult(ApiResponse<AiRecommendationDetailDto>.SuccessResult(match));
        }

        return Task.FromResult(ApiResponse<AiRecommendationDetailDto>.FailureResult(
            $"No AI recommendation found for request '{requestId}'."));
    }

    public Task<ApiResponse<AiRecommendationMetricsDto>> GetMetricsAsync()
    {
        var all = _recommendations.Values.ToList();
        var awaitingCount = all.Count(r => r.ReviewState.Equals("Awaiting Review", StringComparison.OrdinalIgnoreCase));
        var approvedCount = all.Count(r => r.ReviewState.Equals("Approved", StringComparison.OrdinalIgnoreCase));
        var modifiedCount = all.Count(r => r.ReviewState.Equals("Modified", StringComparison.OrdinalIgnoreCase));
        var deferredCount = all.Count(r => r.ReviewState.Equals("Deferred", StringComparison.OrdinalIgnoreCase));
        var avgConfidence = all.Count != 0 ? Math.Round(all.Average(r => r.Confidence), 1) : 0;

        var metrics = new AiRecommendationMetricsDto
        {
            TotalRecommendations = all.Count,
            AwaitingReviewCount = awaitingCount,
            ApprovedCount = approvedCount,
            ModifiedCount = modifiedCount,
            DeferredCount = deferredCount,
            AverageConfidence = avgConfidence,
            LastUpdated = DateTime.UtcNow
        };

        return Task.FromResult(ApiResponse<AiRecommendationMetricsDto>.SuccessResult(
            metrics, "AI recommendation metrics calculated successfully."));
    }

    public Task<ApiResponse<AiRecommendationDetailDto>> RecordDecisionAsync(
        string id,
        RecordDecisionRequestDto request,
        string reviewerName)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return Task.FromResult(ApiResponse<AiRecommendationDetailDto>.FailureResult("Recommendation ID is required."));
        }

        if (!_recommendations.TryGetValue(id.Trim(), out var item))
        {
            return Task.FromResult(ApiResponse<AiRecommendationDetailDto>.FailureResult(
                $"AI Recommendation with ID '{id}' was not found."));
        }

        var isModified = !request.Priority.Trim().Equals(item.RecommendedPriority, StringComparison.OrdinalIgnoreCase);

        item.DecidedPriority = request.Priority.Trim();
        item.DecisionRationale = request.Rationale.Trim();
        item.ReviewedBy = string.IsNullOrWhiteSpace(reviewerName) ? "Authorised Officer" : reviewerName;
        item.ReviewedAt = DateTime.UtcNow;
        item.IsDecisionRecorded = true;
        item.ReviewState = isModified ? "Modified" : "Approved";

        _recommendations[item.Id] = item;

        var message = isModified
            ? $"Human decision recorded with modified priority '{item.DecidedPriority}'."
            : $"AI recommendation successfully approved as '{item.DecidedPriority}'.";

        return Task.FromResult(ApiResponse<AiRecommendationDetailDto>.SuccessResult(item, message));
    }

    public Task<ApiResponse<AiRecommendationDetailDto>> DeferRecommendationAsync(
        string id,
        DeferRecommendationRequestDto? request,
        string reviewerName)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return Task.FromResult(ApiResponse<AiRecommendationDetailDto>.FailureResult("Recommendation ID is required."));
        }

        if (!_recommendations.TryGetValue(id.Trim(), out var item))
        {
            return Task.FromResult(ApiResponse<AiRecommendationDetailDto>.FailureResult(
                $"AI Recommendation with ID '{id}' was not found."));
        }

        item.ReviewState = "Deferred";
        item.ReviewedBy = string.IsNullOrWhiteSpace(reviewerName) ? "Authorised Officer" : reviewerName;
        item.ReviewedAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(request?.Reason))
        {
            item.DecisionRationale = $"[DEFERRED]: {request.Reason.Trim()}";
        }

        _recommendations[item.Id] = item;

        return Task.FromResult(ApiResponse<AiRecommendationDetailDto>.SuccessResult(
            item, $"AI recommendation '{item.Id}' deferred for subsequent evaluation."));
    }

    public Task<ApiResponse<AiRecommendationDetailDto>> ResetRecommendationAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return Task.FromResult(ApiResponse<AiRecommendationDetailDto>.FailureResult("Recommendation ID is required."));
        }

        if (!_recommendations.TryGetValue(id.Trim(), out var item))
        {
            return Task.FromResult(ApiResponse<AiRecommendationDetailDto>.FailureResult(
                $"AI Recommendation with ID '{id}' was not found."));
        }

        item.ReviewState = "Awaiting Review";
        item.DecidedPriority = null;
        item.DecisionRationale = null;
        item.ReviewedBy = null;
        item.ReviewedAt = null;
        item.IsDecisionRecorded = false;

        _recommendations[item.Id] = item;

        return Task.FromResult(ApiResponse<AiRecommendationDetailDto>.SuccessResult(
            item, $"AI recommendation '{item.Id}' reset to awaiting review status."));
    }

    private static AiRecommendationSummaryDto MapToSummary(AiRecommendationDetailDto d)
    {
        var primaryAlt = d.Alternatives.FirstOrDefault();
        var altText = primaryAlt != null
            ? $"{primaryAlt.Priority} priority – {primaryAlt.LikelihoodPercentage}% likelihood"
            : string.Empty;

        return new AiRecommendationSummaryDto
        {
            Id = d.Id,
            RequestId = d.RequestId,
            Category = d.RequestCategory,
            RecommendedPriority = d.RecommendedPriority,
            Confidence = d.Confidence,
            Action = d.RecommendedAction,
            Timestamp = d.ModelTimestamp,
            ReviewState = d.ReviewState,
            Explanation = d.Explanation,
            Alternative = altText,
            Location = d.Location,
            DecidedPriority = d.DecidedPriority,
            ReviewedBy = d.ReviewedBy,
            ReviewedAt = d.ReviewedAt
        };
    }
}
