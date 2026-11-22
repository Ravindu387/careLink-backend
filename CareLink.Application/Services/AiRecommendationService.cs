using System.Collections.Concurrent;
using System.Text.Json;
using CareLink.Application.Common;
using CareLink.Application.DTOs.AiRecommendations;
using CareLink.Application.Interfaces;
using Microsoft.Extensions.Logging;

using CareLink.Core.Entities;

namespace CareLink.Application.Services;

public class AiRecommendationService : IAiRecommendationService
{
    private readonly ConcurrentDictionary<string, AiRecommendationDetailDto> _recommendations = new(StringComparer.OrdinalIgnoreCase);
    private readonly IGeminiService _geminiService;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<AiRecommendationService> _logger;
    private readonly IAiRecommendationRepository? _repository;
    private int _sequence = 9050;

    public AiRecommendationService(
        IGeminiService geminiService,
        IAuditLogService auditLogService,
        ILogger<AiRecommendationService> logger,
        IAiRecommendationRepository? repository = null)
    {
        _geminiService = geminiService;
        _auditLogService = auditLogService;
        _logger = logger;
        _repository = repository;
        InitializeDefaultRecommendations();
    }

    private void InitializeDefaultRecommendations()
    {
        var seedItems = new List<AiRecommendationDetailDto>
        {
            new()
            {
                Id = "AI-9041",
                CrisisId = "CR-2025-018",
                CrisisName = "Monsoon Floods 2025",
                RequestId = "REQ-2841",
                Category = "AssistanceTriage",
                Title = "Emergency Evacuation & Accessible Shelter Triage",
                Summary = "Household of 6 with vulnerable occupants facing active residential inundation in North Ward.",
                CreatedAt = DateTime.UtcNow.AddMinutes(-12),
                RequestCategory = "Emergency shelter",
                ReportedUrgency = "Reported urgent",
                Description = "Household of six requires evacuation as flood water enters their residence.",
                Location = "Riverside, North Ward",
                VulnerabilityFactors = "Children, limited mobility",
                AffectedPeople = 6,
                SubmittedTime = "12 minutes ago",
                VerificationStatus = "Contact verified",
                EvidenceNote = "Citizen description and location boundary are verified. Water level confirmed exceeding 0.5m.",
                RelevantInformation =
                [
                    "Flood water reported inside residence",
                    "6 affected people, including 2 children",
                    "1 household member needs mobility support",
                    "1.4 km from active evacuation zone"
                ],
                ModelName = "Priority Assist v2.4 (Gemini)",
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
                ReviewState = "Awaiting Review",
                ImmediatePriorities =
                [
                    "Deploy high-clearance rescue vehicle to Riverside",
                    "Reserve 2 ground-floor accessible beds at North Community Hall",
                    "Dispatch pediatric oral rehydration rations"
                ],
                Risks =
                [
                    "Rising river tributary may cut off Riverside road within 45 minutes",
                    "Submerged debris hazards for standard emergency vehicles"
                ],
                Assumptions =
                [
                    "Family remains gathered inside the registered residential coordinates",
                    "No immediate life-threatening trauma reported"
                ],
                MissingInformation =
                [
                    "Exact height of inundation above the floor level",
                    "Specialist medical equipment requirements"
                ],
                ResourceRecommendations =
                [
                    new()
                    {
                        ResourceName = "Water - 10L",
                        Category = "Food & Water",
                        RecommendedQuantity = 6,
                        AvailableQuantity = 8420,
                        IsShortage = false,
                        ShortageQuantity = 0,
                        TargetCrisisLocation = "North Community Hall",
                        Priority = "Critical",
                        Rationale = "Mandatory 72-hour clean water ration for 6 evacuees",
                        AlternativeResource = "Water Purification Tablets (box of 50)"
                    },
                    new()
                    {
                        ResourceName = "Shelter Kits",
                        Category = "Shelter & Logistics",
                        RecommendedQuantity = 1,
                        AvailableQuantity = 450,
                        IsShortage = false,
                        ShortageQuantity = 0,
                        TargetCrisisLocation = "Riverside Evac Point",
                        Priority = "High",
                        Rationale = "Temporary bedding and dry protection pack",
                        AlternativeResource = "Thermal Emergency Blankets"
                    }
                ],
                ShelterRecommendations =
                [
                    new()
                    {
                        ShelterName = "North Community Hall",
                        Location = "North Ward",
                        Capacity = 500,
                        CurrentOccupancy = 284,
                        RemainingCapacity = 216,
                        RecommendedEvacueeCount = 6,
                        SuitabilityReason = "Located within 2.1 km of Riverside, certified accessible facilities, operational food station",
                        SafetyAndAccessibilityNotes = "Ramp access verified; dedicated medical station on site"
                    }
                ],
                VolunteerRecommendations =
                [
                    new()
                    {
                        SkillGroup = "Search & Rescue",
                        RecommendedCount = 2,
                        SuggestedTask = "Assisted evacuation of mobility-impaired resident",
                        Priority = "Critical",
                        Rationale = "Specialized transfer technique required due to standing water"
                    }
                ]
            },
            new()
            {
                Id = "AI-9038",
                CrisisId = "CR-2025-018",
                CrisisName = "Monsoon Floods 2025",
                RequestId = "REQ-2839",
                Category = "ResourceAllocation",
                Title = "Medical Supplies Dispatch & Dispensary Linkage",
                Summary = "Isolated patients with chronic prescription requirements and sterile dressing needs in Sector 4.",
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
                ModelName = "Priority Assist v2.4 (Gemini)",
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
                ReviewState = "Awaiting Review",
                ImmediatePriorities =
                [
                    "Cold-chain insulin transport dispatch from Central Depot",
                    "Deliver sterile wound dressings and anti-diarrheal packs"
                ],
                Risks =
                [
                    "Temperature degradation of biological medications during transit",
                    "Bridge crossing weight restrictions"
                ],
                Assumptions =
                [
                    "Patient has access to clean storage container"
                ],
                MissingInformation =
                [
                    "Specific insulin brand and dosage prescription details"
                ],
                ResourceRecommendations =
                [
                    new()
                    {
                        ResourceName = "Emergency Trauma Kits",
                        Category = "Medical Supplies",
                        RecommendedQuantity = 2,
                        AvailableQuantity = 120,
                        IsShortage = false,
                        ShortageQuantity = 0,
                        TargetCrisisLocation = "Sector 4 Clinic",
                        Priority = "High",
                        Rationale = "Sterile wound management for 3 elderly flood victims",
                        AlternativeResource = "First Aid Responder Pack"
                    }
                ]
            },
            new()
            {
                Id = "AI-9032",
                CrisisId = "CR-2025-018",
                CrisisName = "Monsoon Floods 2025",
                RequestId = "REQ-2836",
                Category = "ResourceAllocation",
                Title = "Potable Drinking Water & Nutritional Relief Rations",
                Summary = "Family stranded in Eastbank without access to safe drinking water and power.",
                CreatedAt = DateTime.UtcNow.AddMinutes(-41),
                RequestCategory = "Food & water",
                ReportedUrgency = "High urgency",
                Description = "Potable drinking water and non-perishable rations needed for stranded family.",
                Location = "Eastbank, Sector 2",
                VulnerabilityFactors = "Infants, expectant mother",
                AffectedPeople = 4,
                SubmittedTime = "41 minutes ago",
                VerificationStatus = "Contact verified",
                EvidenceNote = "Sub-divisional officer reported complete municipal tap water failure in Sector 2.",
                RelevantInformation =
                [
                    "4 household members including 8-month-old infant",
                    "No access to clean drinking water for past 18 hours",
                    "Local store submerged"
                ],
                ModelName = "Priority Assist v2.4 (Gemini)",
                ModelTimestamp = "12:15 UTC",
                RecommendedPriority = "High",
                Confidence = 87,
                Explanation = "Vulnerability indicators are elevated due to infant nutrition needs and lack of potable water.",
                RecommendedAction = "Family Food Packs & Water 10L dispatch",
                Alternatives =
                [
                    new() { Priority = "Critical", LikelihoodPercentage = 9 },
                    new() { Priority = "Medium", LikelihoodPercentage = 4 }
                ],
                ReviewState = "Awaiting Review",
                ImmediatePriorities =
                [
                    "Deliver 2 cartons of 10L clean water",
                    "Provide infant-safe ready-to-eat nutritional packs"
                ],
                Risks =
                [
                    "Water-borne gastroenteritis if contaminated flood water is consumed"
                ],
                Assumptions =
                [
                    "Household has safe dry space above flood level for food storage"
                ],
                MissingInformation =
                [
                    "Whether infant formula or boiled water is preferred"
                ],
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
                        TargetCrisisLocation = "Eastbank Distribution Hub",
                        Priority = "High",
                        Rationale = "Immediate potable water for nursing mother and family",
                        AlternativeResource = "Water Purification Sachet (box of 100)"
                    },
                    new()
                    {
                        ResourceName = "Family Food Packs",
                        Category = "Food & Water",
                        RecommendedQuantity = 2,
                        AvailableQuantity = 1284,
                        IsShortage = false,
                        ShortageQuantity = 0,
                        TargetCrisisLocation = "Eastbank Distribution Hub",
                        Priority = "High",
                        Rationale = "Shelf-stable 7-day provisions for 4 persons",
                        AlternativeResource = "High-energy biscuits"
                    }
                ]
            },
            new()
            {
                Id = "AI-9024",
                CrisisId = "CR-2025-018",
                CrisisName = "Monsoon Floods 2025",
                RequestId = "REQ-2828",
                Category = "VolunteerAssignment",
                Title = "Specialized Assisted Mobility Evacuation",
                Summary = "Wheelchair accessible evacuation required for senior resident from second-floor shelter.",
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
                ModelName = "Priority Assist v2.4 (Gemini)",
                ModelTimestamp = "11:55 UTC",
                RecommendedPriority = "High",
                Confidence = 82,
                Explanation = "A mobility vehicle is recommended based on the applicant's accessibility requirements.",
                RecommendedAction = "Deploy Mobility Vehicle 4 + 2 Trained Volunteers",
                Alternatives =
                [
                    new() { Priority = "Medium", LikelihoodPercentage = 15 },
                    new() { Priority = "Low", LikelihoodPercentage = 3 }
                ],
                ReviewState = "Awaiting Review",
                ImmediatePriorities =
                [
                    "Dispatch accessible van with hydraulic lift",
                    "Assign 2 mobility-trained volunteer assistants"
                ],
                Risks =
                [
                    "Narrow doorway clearance on second floor landing"
                ],
                Assumptions =
                [
                    "Power restored to lobby entrance"
                ],
                MissingInformation =
                [
                    "Wheelchair battery charge level"
                ],
                VolunteerRecommendations =
                [
                    new()
                    {
                        SkillGroup = "Mobility & Disability Support",
                        RecommendedCount = 2,
                        SuggestedTask = "Assist wheelchair transfer down stairwell",
                        Priority = "High",
                        Rationale = "Certified physical transfer technique required"
                    }
                ]
            },
            new()
            {
                Id = "AI-9019",
                CrisisId = "CR-2025-018",
                CrisisName = "Monsoon Floods 2025",
                RequestId = "REQ-2821",
                Category = "ResponsePlan",
                Title = "Rooftop Fast-Flow Flood Rescue",
                Summary = "Two adults stranded on roof of single-story home surrounded by fast-flowing flood currents.",
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
                ModelName = "Priority Assist v2.4 (Gemini)",
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
                ReviewState = "Awaiting Review",
                ImmediatePriorities =
                [
                    "Launch motorized swiftwater inflatable boat",
                    "Evacuate rooftop occupants to Basin Relief Depot"
                ],
                Risks =
                [
                    "Submerged utility wires and fencing along approach channel"
                ],
                Assumptions =
                [
                    "Structural roof integrity will hold for minimum 60 minutes"
                ],
                MissingInformation =
                [
                    "Presence of pets or domestic animals requiring transport"
                ],
                VolunteerRecommendations =
                [
                    new()
                    {
                        SkillGroup = "Search & Rescue",
                        RecommendedCount = 4,
                        SuggestedTask = "Swiftwater boat operations and perimeter guidance",
                        Priority = "Critical",
                        Rationale = "Class 3 swiftwater certified rescue specialists required"
                    }
                ]
            },
            new()
            {
                Id = "AI-9015",
                CrisisId = "CR-2025-018",
                CrisisName = "Monsoon Floods 2025",
                RequestId = "REQ-2815",
                Category = "ResourceAllocation",
                Title = "Cold-Chain Backup Generator Fuel Dispatch",
                Summary = "Backup generator fuel needed for community relief dispensary running medical refrigeration.",
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
                ModelName = "Priority Assist v2.4 (Gemini)",
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
                ReviewState = "Awaiting Review",
                ImmediatePriorities =
                [
                    "Transport 200L stabilized diesel fuel",
                    "Verify generator fuel intake and transfer pump"
                ],
                Risks =
                [
                    "Flooded secondary access road to dispensary depot"
                ],
                Assumptions =
                [
                    "Generator filter was serviced within past 90 days"
                ],
                MissingInformation =
                [
                    "Secondary fuel tank capacity"
                ],
                ResourceRecommendations =
                [
                    new()
                    {
                        ResourceName = "Diesel Fuel - 200L Drum",
                        Category = "Fuel & Power",
                        RecommendedQuantity = 1,
                        AvailableQuantity = 38,
                        IsShortage = false,
                        ShortageQuantity = 0,
                        TargetCrisisLocation = "South Ward Community Centre",
                        Priority = "High",
                        Rationale = "72 hours continuous generator power for cold storage",
                        AlternativeResource = "Portable Solar Inverter 5kW"
                    }
                ]
            },
            new()
            {
                Id = "AI-9012",
                CrisisId = "CR-2025-018",
                CrisisName = "Monsoon Floods 2025",
                RequestId = "REQ-2809",
                Category = "ShelterMatching",
                Title = "Shelter Sanitation & Hygiene Station Augmentation",
                Summary = "Emergency sanitation units and water purification tablets for temporary community hub.",
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
                ModelName = "Priority Assist v2.4 (Gemini)",
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
                ReviewState = "Awaiting Review",
                ImmediatePriorities =
                [
                    "Deliver 4 chemical sanitation mobile cabins",
                    "Supply 200 hygiene amenity packs"
                ],
                Risks =
                [
                    "Greywater runoff contaminating playground boundary"
                ],
                Assumptions =
                [
                    "Adequate flat ground near north entrance for placement"
                ],
                MissingInformation =
                [
                    "Water hookup pressure from main school line"
                ],
                ShelterRecommendations =
                [
                    new()
                    {
                        ShelterName = "Highland School Evacuation Point",
                        Location = "Highland Sector",
                        Capacity = 150,
                        CurrentOccupancy = 120,
                        RemainingCapacity = 30,
                        RecommendedEvacueeCount = 0,
                        SuitabilityReason = "Operational school with gym facility; urgent sanitation relief required",
                        SafetyAndAccessibilityNotes = "Separate facilities for families and elderly"
                    }
                ]
            },
            new()
            {
                Id = "AI-9008",
                CrisisId = "CR-2025-018",
                CrisisName = "Monsoon Floods 2025",
                RequestId = "REQ-2804",
                Category = "ResourceAllocation",
                Title = "Tarpaulins & Temporary Weatherproofing Deployment",
                Summary = "Roof tiles displaced by gale-force winds causing rainwater leakage.",
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
                ModelName = "Priority Assist v2.4 (Gemini)",
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
                ReviewState = "Awaiting Review",
                ImmediatePriorities =
                [
                    "Deliver 2 reinforced poly-tarpaulins",
                    "Install temporary tie-down weights on exterior roof truss"
                ],
                Risks =
                [
                    "Slippery clay roof pitch during active drizzle"
                ],
                Assumptions =
                [
                    "Interior ceilings have not suffered sheetrock collapse"
                ],
                MissingInformation =
                [
                    "Ladder height needed to reach lower roof valley"
                ],
                ResourceRecommendations =
                [
                    new()
                    {
                        ResourceName = "Heavy-Duty Tarpaulins (12x18ft)",
                        Category = "Shelter & Logistics",
                        RecommendedQuantity = 2,
                        AvailableQuantity = 240,
                        IsShortage = false,
                        ShortageQuantity = 0,
                        TargetCrisisLocation = "East Hillside",
                        Priority = "Medium",
                        Rationale = "External roof leak mitigation for isolated home",
                        AlternativeResource = "Plastic Waterproof Sheeting Roll"
                    }
                ]
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
        string? category = null,
        string? crisisId = null,
        string? search = null)
    {
        var query = _recommendations.Values.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(status) && !status.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(r => r.ReviewState.Equals(status, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(priority) && !priority.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(r => r.RecommendedPriority.Equals(priority, StringComparison.OrdinalIgnoreCase)
                                     || (r.DecidedPriority != null && r.DecidedPriority.Equals(priority, StringComparison.OrdinalIgnoreCase)));
        }

        if (!string.IsNullOrWhiteSpace(category) && !category.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(r => r.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(crisisId) && !crisisId.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(r => (r.CrisisId != null && r.CrisisId.Equals(crisisId, StringComparison.OrdinalIgnoreCase))
                                     || (r.CrisisName != null && r.CrisisName.Contains(crisisId, StringComparison.OrdinalIgnoreCase)));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(r => r.Id.Contains(s, StringComparison.OrdinalIgnoreCase)
                                     || r.RequestId.Contains(s, StringComparison.OrdinalIgnoreCase)
                                     || r.Title.Contains(s, StringComparison.OrdinalIgnoreCase)
                                     || r.Location.Contains(s, StringComparison.OrdinalIgnoreCase)
                                     || r.RecommendedAction.Contains(s, StringComparison.OrdinalIgnoreCase)
                                     || r.Explanation.Contains(s, StringComparison.OrdinalIgnoreCase));
        }

        var results = query
            .OrderByDescending(r => r.CreatedAt)
            .Select(MapToSummary)
            .ToList();

        return Task.FromResult(ApiResponse<IReadOnlyList<AiRecommendationSummaryDto>>.SuccessResult(results));
    }

    public async Task<ApiResponse<AiRecommendationDetailDto>> GetRecommendationByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return ApiResponse<AiRecommendationDetailDto>.FailureResult("Recommendation ID is required.");
        }

        if (_recommendations.TryGetValue(id.Trim(), out var detail))
        {
            return ApiResponse<AiRecommendationDetailDto>.SuccessResult(detail);
        }

        if (_repository != null)
        {
            var entity = await _repository.GetByIdAsync(id.Trim());
            if (entity != null)
            {
                var dto = MapEntityToDto(entity);
                _recommendations[dto.Id] = dto;
                return ApiResponse<AiRecommendationDetailDto>.SuccessResult(dto);
            }
        }

        return ApiResponse<AiRecommendationDetailDto>.FailureResult($"AI Recommendation '{id}' not found.");
    }

    public async Task<ApiResponse<AiRecommendationDetailDto>> GetRecommendationByRequestIdAsync(string requestId)
    {
        if (string.IsNullOrWhiteSpace(requestId))
        {
            return ApiResponse<AiRecommendationDetailDto>.FailureResult("Request ID is required.");
        }

        var match = _recommendations.Values.FirstOrDefault(r =>
            r.RequestId != null && r.RequestId.Equals(requestId.Trim(), StringComparison.OrdinalIgnoreCase));

        if (match != null)
        {
            return ApiResponse<AiRecommendationDetailDto>.SuccessResult(match);
        }

        if (_repository != null)
        {
            var entity = await _repository.GetByRequestIdAsync(requestId.Trim());
            if (entity != null)
            {
                var dto = MapEntityToDto(entity);
                _recommendations[dto.Id] = dto;
                return ApiResponse<AiRecommendationDetailDto>.SuccessResult(dto);
            }
        }

        return ApiResponse<AiRecommendationDetailDto>.FailureResult(
            $"No AI recommendation found for request '{requestId}'.");
    }

    public Task<ApiResponse<AiRecommendationMetricsDto>> GetMetricsAsync()
    {
        var items = _recommendations.Values.ToList();

        var awaitingCount = items.Count(r => r.ReviewState.Equals("Awaiting Review", StringComparison.OrdinalIgnoreCase));
        var approvedCount = items.Count(r => r.ReviewState.Equals("Approved", StringComparison.OrdinalIgnoreCase));
        var modifiedCount = items.Count(r => r.ReviewState.Equals("Modified", StringComparison.OrdinalIgnoreCase));
        var deferredCount = items.Count(r => r.ReviewState.Equals("Deferred", StringComparison.OrdinalIgnoreCase));
        var rejectedCount = items.Count(r => r.ReviewState.Equals("Rejected", StringComparison.OrdinalIgnoreCase));

        var avgConfidence = items.Count > 0
            ? Math.Round(items.Average(r => r.Confidence), 1)
            : 87.5;

        var metrics = new AiRecommendationMetricsDto
        {
            TotalCases = items.Count,
            AwaitingReviewCount = awaitingCount,
            ApprovedCount = approvedCount,
            ModifiedCount = modifiedCount,
            DeferredCount = deferredCount,
            AverageConfidencePercentage = avgConfidence
        };

        return Task.FromResult(ApiResponse<AiRecommendationMetricsDto>.SuccessResult(metrics));
    }

    public async Task<ApiResponse<AiRecommendationDetailDto>> RecordDecisionAsync(
        string id,
        RecordDecisionRequestDto request,
        string reviewerName)
    {
        if (!_recommendations.TryGetValue(id, out var rec))
        {
            return ApiResponse<AiRecommendationDetailDto>.FailureResult($"Recommendation '{id}' not found.");
        }

        if (rec.ReviewState.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
        {
            return ApiResponse<AiRecommendationDetailDto>.FailureResult(
                "Cannot modify or record decisions on a rejected recommendation. Reset the recommendation first.");
        }

        rec.DecidedPriority = request.Priority;
        rec.DecisionRationale = request.Rationale;
        rec.ReviewedBy = reviewerName;
        rec.ReviewedAt = DateTime.UtcNow;
        rec.IsDecisionRecorded = true;

        if (request.Priority.Equals(rec.RecommendedPriority, StringComparison.OrdinalIgnoreCase))
        {
            rec.ReviewState = "Approved";
        }
        else
        {
            rec.ReviewState = "Modified";
        }

        await PersistToRepositoryAsync(rec);

        await LogAuditAsync(
            action: $"AI Recommendation {rec.ReviewState}",
            actor: reviewerName,
            record: rec.Id,
            change: $"Priority: {rec.RecommendedPriority} -> {rec.DecidedPriority}. Status: {rec.ReviewState}",
            rationale: rec.DecisionRationale ?? "Evaluated and signed off by authorized coordinator.",
            type: "System");

        return ApiResponse<AiRecommendationDetailDto>.SuccessResult(rec);
    }

    public async Task<ApiResponse<AiRecommendationDetailDto>> ApproveRecommendationAsync(
        string id,
        ApproveRecommendationRequestDto? request,
        string reviewerName)
    {
        if (!_recommendations.TryGetValue(id, out var rec))
        {
            return ApiResponse<AiRecommendationDetailDto>.FailureResult($"Recommendation '{id}' not found.");
        }

        if (rec.ReviewState.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
        {
            return ApiResponse<AiRecommendationDetailDto>.FailureResult(
                "Cannot approve a recommendation that is currently rejected. Reset the recommendation first.");
        }

        rec.ReviewState = "Approved";
        rec.DecidedPriority = request?.DecidedPriority ?? rec.RecommendedPriority;
        rec.DecisionRationale = request?.Notes ?? "Approved as recommended by coordinator.";
        rec.ReviewedBy = reviewerName;
        rec.ReviewedAt = DateTime.UtcNow;
        rec.IsDecisionRecorded = true;

        await PersistToRepositoryAsync(rec);

        await LogAuditAsync(
            action: "AI Recommendation Approved",
            actor: reviewerName,
            record: rec.Id,
            change: $"Approved priority: {rec.DecidedPriority}",
            rationale: rec.DecisionRationale,
            type: "System");

        return ApiResponse<AiRecommendationDetailDto>.SuccessResult(rec);
    }

    public async Task<ApiResponse<AiRecommendationDetailDto>> RejectRecommendationAsync(
        string id,
        RejectRecommendationRequestDto request,
        string reviewerName)
    {
        if (!_recommendations.TryGetValue(id, out var rec))
        {
            return ApiResponse<AiRecommendationDetailDto>.FailureResult($"Recommendation '{id}' not found.");
        }

        if (string.IsNullOrWhiteSpace(request?.RejectionReason))
        {
            return ApiResponse<AiRecommendationDetailDto>.FailureResult("A rejection reason is required.");
        }

        if (rec.ReviewState.Equals("Approved", StringComparison.OrdinalIgnoreCase))
        {
            return ApiResponse<AiRecommendationDetailDto>.FailureResult(
                "Cannot reject a recommendation that is already approved. Reset the recommendation first.");
        }

        rec.ReviewState = "Rejected";
        rec.RejectionReason = request.RejectionReason;
        rec.DecisionRationale = request.Notes ?? request.RejectionReason;
        rec.ReviewedBy = reviewerName;
        rec.ReviewedAt = DateTime.UtcNow;
        rec.IsDecisionRecorded = true;

        await PersistToRepositoryAsync(rec);

        await LogAuditAsync(
            action: "AI Recommendation Rejected",
            actor: reviewerName,
            record: rec.Id,
            change: "ReviewState -> Rejected",
            rationale: rec.RejectionReason,
            type: "System");

        return ApiResponse<AiRecommendationDetailDto>.SuccessResult(rec);
    }

    public async Task<ApiResponse<AiRecommendationDetailDto>> DeferRecommendationAsync(
        string id,
        DeferRecommendationRequestDto? request,
        string reviewerName)
    {
        if (!_recommendations.TryGetValue(id, out var rec))
        {
            return ApiResponse<AiRecommendationDetailDto>.FailureResult($"Recommendation '{id}' not found.");
        }

        rec.ReviewState = "Deferred";
        rec.DecisionRationale = request?.Reason ?? "Deferred pending field verification.";
        rec.ReviewedBy = reviewerName;
        rec.ReviewedAt = DateTime.UtcNow;
        rec.IsDecisionRecorded = true;

        await PersistToRepositoryAsync(rec);

        await LogAuditAsync(
            action: "AI Recommendation Deferred",
            actor: reviewerName,
            record: rec.Id,
            change: "ReviewState -> Deferred",
            rationale: rec.DecisionRationale,
            type: "System");

        return ApiResponse<AiRecommendationDetailDto>.SuccessResult(rec);
    }

    public async Task<ApiResponse<AiRecommendationDetailDto>> ResetRecommendationAsync(string id)
    {
        if (!_recommendations.TryGetValue(id, out var rec))
        {
            return ApiResponse<AiRecommendationDetailDto>.FailureResult($"Recommendation '{id}' not found.");
        }

        rec.ReviewState = "Awaiting Review";
        rec.DecidedPriority = null;
        rec.DecisionRationale = null;
        rec.ReviewedBy = null;
        rec.ReviewedAt = null;
        rec.RejectionReason = null;
        rec.IsDecisionRecorded = false;

        await PersistToRepositoryAsync(rec);

        return ApiResponse<AiRecommendationDetailDto>.SuccessResult(rec);
    }

    private async Task PersistToRepositoryAsync(AiRecommendationDetailDto dto, CancellationToken cancellationToken = default)
    {
        if (_repository == null) return;
        try
        {
            var existing = await _repository.GetByIdAsync(dto.Id, cancellationToken);
            if (existing != null)
            {
                existing.ReviewState = dto.ReviewState;
                existing.DecidedPriority = dto.DecidedPriority;
                existing.DecisionRationale = dto.DecisionRationale;
                existing.ReviewedBy = dto.ReviewedBy;
                existing.ReviewedAt = dto.ReviewedAt;
                existing.RejectionReason = dto.RejectionReason;
                existing.StructuredDataJson = JsonSerializer.Serialize(dto);
                await _repository.UpdateAsync(existing, cancellationToken);
            }
            else
            {
                var entity = new AiRecommendation
                {
                    Id = dto.Id,
                    CrisisId = dto.CrisisId,
                    CrisisName = dto.CrisisName,
                    RequestId = dto.RequestId,
                    Category = dto.Category,
                    Title = dto.Title,
                    Summary = dto.Summary,
                    Description = dto.Description,
                    Location = dto.Location,
                    RecommendedPriority = dto.RecommendedPriority,
                    Confidence = dto.Confidence,
                    Explanation = dto.Explanation,
                    RecommendedAction = dto.RecommendedAction,
                    ModelName = dto.ModelName,
                    ModelTimestamp = dto.ModelTimestamp,
                    ReviewState = dto.ReviewState,
                    DecidedPriority = dto.DecidedPriority,
                    DecisionRationale = dto.DecisionRationale,
                    ReviewedBy = dto.ReviewedBy,
                    ReviewedAt = dto.ReviewedAt,
                    RejectionReason = dto.RejectionReason,
                    IsLiveAiGenerated = dto.IsLiveAiGenerated,
                    StructuredDataJson = JsonSerializer.Serialize(dto),
                    CreatedAt = dto.CreatedAt
                };
                await _repository.AddAsync(entity, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to persist recommendation {Id} to repository.", dto.Id);
        }
    }

    private static AiRecommendationDetailDto MapEntityToDto(AiRecommendation entity)
    {
        if (!string.IsNullOrWhiteSpace(entity.StructuredDataJson))
        {
            try
            {
                var parsed = JsonSerializer.Deserialize<AiRecommendationDetailDto>(entity.StructuredDataJson);
                if (parsed != null)
                {
                    parsed.ReviewState = entity.ReviewState;
                    parsed.DecidedPriority = entity.DecidedPriority;
                    parsed.DecisionRationale = entity.DecisionRationale;
                    parsed.ReviewedBy = entity.ReviewedBy;
                    parsed.ReviewedAt = entity.ReviewedAt;
                    parsed.RejectionReason = entity.RejectionReason;
                    return parsed;
                }
            }
            catch
            {
                // Fallback to direct mapping
            }
        }

        return new AiRecommendationDetailDto
        {
            Id = entity.Id,
            CrisisId = entity.CrisisId,
            CrisisName = entity.CrisisName ?? string.Empty,
            RequestId = entity.RequestId ?? string.Empty,
            Category = entity.Category,
            Title = entity.Title,
            Summary = entity.Summary,
            Description = entity.Description,
            Location = entity.Location,
            RecommendedPriority = entity.RecommendedPriority,
            Confidence = entity.Confidence,
            Explanation = entity.Explanation,
            RecommendedAction = entity.RecommendedAction,
            ModelName = entity.ModelName,
            ModelTimestamp = entity.ModelTimestamp,
            ReviewState = entity.ReviewState,
            DecidedPriority = entity.DecidedPriority,
            DecisionRationale = entity.DecisionRationale,
            ReviewedBy = entity.ReviewedBy,
            ReviewedAt = entity.ReviewedAt,
            RejectionReason = entity.RejectionReason,
            IsLiveAiGenerated = entity.IsLiveAiGenerated,
            CreatedAt = entity.CreatedAt
        };
    }

    public async Task<ApiResponse<AiRecommendationDetailDto>> GenerateCrisisAnalysisAsync(
        string crisisId,
        GenerateCrisisAnalysisRequestDto request,
        string requestedBy,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(crisisId))
        {
            return ApiResponse<AiRecommendationDetailDto>.FailureResult("Crisis ID is required.");
        }

        var id = $"AI-{Interlocked.Increment(ref _sequence)}";
        var crisisName = request.CrisisName ?? "Monsoon Floods 2025";

        var detail = new AiRecommendationDetailDto
        {
            Id = id,
            CrisisId = crisisId,
            CrisisName = crisisName,
            Category = "CrisisAnalysis",
            Title = $"Crisis Situation Assessment · {crisisName}",
            Summary = "Comprehensive situational severity and operational vulnerability triage.",
            CreatedAt = DateTime.UtcNow,
            RequestCategory = "Crisis Analysis",
            ReportedUrgency = "Critical Urgency",
            Location = "Greater Riverton",
            ModelTimestamp = DateTime.UtcNow.ToString("HH:mm UTC"),
            ModelName = _geminiService.IsConfigured ? _geminiService.ModelName : "Priority Assist v2.4 (Grounded)",
            ReviewState = "Awaiting Review"
        };

        if (_geminiService.IsConfigured)
        {
            try
            {
                var prompt = $@"Analyze this humanitarian crisis situation using grounded facts:
Crisis ID: {crisisId}
Crisis Title: {crisisName}
Operational Area: Greater Riverton
Focus Area: {request.FocusArea ?? "All sectors"}
Context: {request.AdditionalContext ?? "Monsoon flooding affecting 1,284 recorded assistance cases across 4 sectors."}

Return a valid JSON object matching this schema:
{{
  ""summary"": ""concise situation summary"",
  ""recommendedPriority"": ""Critical|High|Medium"",
  ""confidence"": 92,
  ""explanation"": ""explanation distinguishing facts from estimates"",
  ""recommendedAction"": ""primary immediate coordination action"",
  ""immediatePriorities"": [""priority 1"", ""priority 2"", ""priority 3""],
  ""risks"": [""key risk 1"", ""key risk 2""],
  ""assumptions"": [""assumption 1""],
  ""missingInformation"": [""missing piece of info""]
}}";

                var aiResult = await _geminiService.GenerateStructuredContentAsync<GeminiAnalysisResponse>(
                    prompt,
                    "You are the CareLink crisis response intelligence engine. Adhere strictly to verified facts. Return only clean JSON.",
                    cancellationToken);

                if (aiResult != null)
                {
                    detail.Summary = aiResult.Summary;
                    detail.RecommendedPriority = aiResult.RecommendedPriority;
                    detail.Confidence = Math.Clamp(aiResult.Confidence, 60, 99);
                    detail.Explanation = aiResult.Explanation;
                    detail.RecommendedAction = aiResult.RecommendedAction;
                    detail.ImmediatePriorities = aiResult.ImmediatePriorities;
                    detail.Risks = aiResult.Risks;
                    detail.Assumptions = aiResult.Assumptions;
                    detail.MissingInformation = aiResult.MissingInformation;
                    detail.IsLiveAiGenerated = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Gemini generation failed for Crisis Analysis. Falling back to grounded deterministic baseline.");
                ApplyDeterministicCrisisAnalysis(detail);
            }
        }
        else
        {
            ApplyDeterministicCrisisAnalysis(detail);
        }

        _recommendations[id] = detail;
        await PersistToRepositoryAsync(detail, cancellationToken);

        await LogAuditAsync(
            action: "AI Crisis Analysis Generated",
            actor: requestedBy,
            record: id,
            change: $"Generated for {crisisName} ({crisisId})",
            rationale: $"Requested by coordinator. Provider: {detail.ModelName}. Live AI: {detail.IsLiveAiGenerated}",
            type: "System");

        return ApiResponse<AiRecommendationDetailDto>.SuccessResult(detail);
    }

    public async Task<ApiResponse<AiRecommendationDetailDto>> GenerateResourceRecommendationsAsync(
        string crisisId,
        GenerateResourceRecommendationRequestDto request,
        string requestedBy,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(crisisId))
        {
            return ApiResponse<AiRecommendationDetailDto>.FailureResult("Crisis ID is required.");
        }

        var id = $"AI-{Interlocked.Increment(ref _sequence)}";
        var crisisName = request.CrisisName ?? "Monsoon Floods 2025";

        var detail = new AiRecommendationDetailDto
        {
            Id = id,
            CrisisId = crisisId,
            CrisisName = crisisName,
            Category = "ResourceAllocation",
            Title = $"Optimized Resource Allocation Plan · {crisisName}",
            Summary = "Inventory-grounded supply matching with shortage detection and depot dispatch routing.",
            CreatedAt = DateTime.UtcNow,
            RequestCategory = "Logistics & Resources",
            ReportedUrgency = request.Urgency ?? "High Urgency",
            Location = request.TargetLocation ?? "Greater Riverton Depots",
            ModelTimestamp = DateTime.UtcNow.ToString("HH:mm UTC"),
            ModelName = _geminiService.IsConfigured ? _geminiService.ModelName : "Logistics Matcher (Grounded)",
            ReviewState = "Awaiting Review"
        };

        ApplyDeterministicResourceAllocation(detail);

        if (_geminiService.IsConfigured)
        {
            try
            {
                var prompt = $@"Generate resource allocation recommendations for {crisisName} ({crisisId}):
Target Area: {detail.Location}
Verified Available Depot Stock:
- Water 10L: 8,420 units available (Central Depot)
- Family Food Packs: 1,284 units available (North Hub)
- Shelter Kits: 450 units available (Central Depot)
- Emergency Trauma Kits: 120 units available (Medical Depot)
- Hygiene Packs: 850 units available (West Depot)

Return a valid JSON object matching:
{{
  ""summary"": ""summary of resource distribution plan"",
  ""recommendedPriority"": ""Critical|High"",
  ""confidence"": 89,
  ""explanation"": ""reasoning considering depot availability and delivery constraints"",
  ""recommendedAction"": ""immediate batch dispatch instruction"",
  ""immediatePriorities"": [""action 1"", ""action 2""],
  ""risks"": [""supply risk 1""],
  ""assumptions"": [""logistics assumption 1""],
  ""missingInformation"": [""unverified road access""],
  ""resourcePlan"": [
    {{
      ""resourceName"": ""Water - 10L"",
      ""category"": ""Food & Water"",
      ""recommendedQuantity"": 2500,
      ""availableQuantity"": 8420,
      ""isShortage"": false,
      ""shortageQuantity"": 0,
      ""targetCrisisLocation"": ""North Ward Hub"",
      ""priority"": ""Critical"",
      ""rationale"": ""covers immediate hydration needs for 1200 evacuees for 48h"",
      ""alternativeResource"": ""Water purification sachets""
    }}
  ]
}}";

                var aiResult = await _geminiService.GenerateStructuredContentAsync<GeminiResourceResponse>(
                    prompt,
                    "You are CareLink's humanitarian logistics optimizer. Never allocate more than available without marking shortage. Return valid JSON.",
                    cancellationToken);

                if (aiResult != null && aiResult.ResourcePlan.Count > 0)
                {
                    detail.Summary = aiResult.Summary;
                    detail.RecommendedPriority = aiResult.RecommendedPriority;
                    detail.Confidence = Math.Clamp(aiResult.Confidence, 60, 99);
                    detail.Explanation = aiResult.Explanation;
                    detail.RecommendedAction = aiResult.RecommendedAction;
                    detail.ImmediatePriorities = aiResult.ImmediatePriorities;
                    detail.Risks = aiResult.Risks;
                    detail.Assumptions = aiResult.Assumptions;
                    detail.MissingInformation = aiResult.MissingInformation;
                    detail.ResourceRecommendations = aiResult.ResourcePlan;
                    detail.IsLiveAiGenerated = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Gemini generation failed for Resource Plan. Preserving deterministic grounded plan.");
            }
        }

        _recommendations[id] = detail;
        await PersistToRepositoryAsync(detail, cancellationToken);

        await LogAuditAsync(
            action: "AI Resource Recommendations Generated",
            actor: requestedBy,
            record: id,
            change: $"Generated for {crisisName}",
            rationale: $"Decision-support only. No inventory changed. Live AI: {detail.IsLiveAiGenerated}",
            type: "System");

        return ApiResponse<AiRecommendationDetailDto>.SuccessResult(detail);
    }

    public async Task<ApiResponse<AiRecommendationDetailDto>> GenerateShelterRecommendationsAsync(
        string crisisId,
        GenerateShelterRecommendationRequestDto request,
        string requestedBy,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(crisisId))
        {
            return ApiResponse<AiRecommendationDetailDto>.FailureResult("Crisis ID is required.");
        }

        var id = $"AI-{Interlocked.Increment(ref _sequence)}";
        var crisisName = request.CrisisName ?? "Monsoon Floods 2025";
        var evacCount = request.EvacueesCount ?? 120;

        var detail = new AiRecommendationDetailDto
        {
            Id = id,
            CrisisId = crisisId,
            CrisisName = crisisName,
            Category = "ShelterMatching",
            Title = $"Shelter Capacity & Occupancy Match · {crisisName}",
            Summary = $"Matching {evacCount} evacuees to verified active shelters with available bed capacity.",
            CreatedAt = DateTime.UtcNow,
            RequestCategory = "Emergency Shelter",
            ReportedUrgency = "High Urgency",
            Location = "Greater Riverton Shelters",
            ModelTimestamp = DateTime.UtcNow.ToString("HH:mm UTC"),
            ModelName = _geminiService.IsConfigured ? _geminiService.ModelName : "Shelter Matcher (Grounded)",
            ReviewState = "Awaiting Review"
        };

        ApplyDeterministicShelterMatching(detail, evacCount);

        _recommendations[id] = detail;
        await PersistToRepositoryAsync(detail, cancellationToken);

        await LogAuditAsync(
            action: "AI Shelter Recommendations Generated",
            actor: requestedBy,
            record: id,
            change: $"Matched for {evacCount} persons in {crisisName}",
            rationale: "Decision-support recommendation. No shelter occupant transfers automated.",
            type: "Shelter");

        return ApiResponse<AiRecommendationDetailDto>.SuccessResult(detail);
    }

    public async Task<ApiResponse<AiRecommendationDetailDto>> GenerateVolunteerRecommendationsAsync(
        string crisisId,
        GenerateVolunteerRecommendationRequestDto request,
        string requestedBy,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(crisisId))
        {
            return ApiResponse<AiRecommendationDetailDto>.FailureResult("Crisis ID is required.");
        }

        var id = $"AI-{Interlocked.Increment(ref _sequence)}";
        var crisisName = request.CrisisName ?? "Monsoon Floods 2025";

        var detail = new AiRecommendationDetailDto
        {
            Id = id,
            CrisisId = crisisId,
            CrisisName = crisisName,
            Category = "VolunteerAssignment",
            Title = $"Volunteer Deployment & Skill Alignment · {crisisName}",
            Summary = "Aligning verified volunteer capabilities against field priority operations.",
            CreatedAt = DateTime.UtcNow,
            RequestCategory = "Volunteer Mobilization",
            ReportedUrgency = "High Urgency",
            Location = "Riverton Emergency Zone",
            ModelTimestamp = DateTime.UtcNow.ToString("HH:mm UTC"),
            ModelName = _geminiService.IsConfigured ? _geminiService.ModelName : "Volunteer Allocator (Grounded)",
            ReviewState = "Awaiting Review"
        };

        ApplyDeterministicVolunteerPlan(detail);

        _recommendations[id] = detail;
        await PersistToRepositoryAsync(detail, cancellationToken);

        await LogAuditAsync(
            action: "AI Volunteer Recommendations Generated",
            actor: requestedBy,
            record: id,
            change: $"Deployment plan created for {crisisName}",
            rationale: "Requires coordinator sign-off prior to task notification dispatch.",
            type: "System");

        return ApiResponse<AiRecommendationDetailDto>.SuccessResult(detail);
    }

    public async Task<ApiResponse<AiRecommendationDetailDto>> GenerateResponsePlanAsync(
        string crisisId,
        GenerateResponsePlanRequestDto request,
        string requestedBy,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(crisisId))
        {
            return ApiResponse<AiRecommendationDetailDto>.FailureResult("Crisis ID is required.");
        }

        var id = $"AI-{Interlocked.Increment(ref _sequence)}";
        var crisisName = request.CrisisName ?? "Monsoon Floods 2025";

        var detail = new AiRecommendationDetailDto
        {
            Id = id,
            CrisisId = crisisId,
            CrisisName = crisisName,
            Category = "ResponsePlan",
            Title = $"Combined Crisis Response Plan · {crisisName}",
            Summary = "Holistic multi-agency coordination plan combining resources, shelters, and volunteer squads.",
            CreatedAt = DateTime.UtcNow,
            RequestCategory = "Multi-Domain Strategy",
            ReportedUrgency = "Critical Urgency",
            Location = "Greater Riverton Crisis Zone",
            ModelTimestamp = DateTime.UtcNow.ToString("HH:mm UTC"),
            ModelName = _geminiService.IsConfigured ? _geminiService.ModelName : "Strategic Coordinator (Grounded)",
            ReviewState = "Awaiting Review"
        };

        ApplyDeterministicCrisisAnalysis(detail);
        ApplyDeterministicResourceAllocation(detail);
        ApplyDeterministicShelterMatching(detail, 250);
        ApplyDeterministicVolunteerPlan(detail);

        detail.Title = $"Combined Crisis Response Plan · {crisisName}";
        detail.Summary = "Integrated strategic overview uniting logistics, shelter intake, and search/medical field units.";

        _recommendations[id] = detail;
        await PersistToRepositoryAsync(detail, cancellationToken);

        await LogAuditAsync(
            action: "AI Response Plan Generated",
            actor: requestedBy,
            record: id,
            change: $"Strategic plan created for {crisisName}",
            rationale: "Integrated human-in-the-loop crisis roadmap.",
            type: "System");

        return ApiResponse<AiRecommendationDetailDto>.SuccessResult(detail);
    }

    private static void ApplyDeterministicCrisisAnalysis(AiRecommendationDetailDto detail)
    {
        detail.Summary = "Monsoon flooding across Greater Riverton has resulted in 1,284 logged assistance requests, with 142 urgent cases requiring immediate triage.";
        detail.RecommendedPriority = "Critical";
        detail.Confidence = 91;
        detail.Explanation = "Active water rise in North Ward and Sector 4 combined with localized power disruption elevates immediate life-safety risk for isolated households.";
        detail.RecommendedAction = "Activate Emergency Operations Center Stage 3 and prioritize North Ward evacuations.";
        detail.ImmediatePriorities =
        [
            "Reinforce Riverside levee with sandbag logistics",
            "Establish rapid medical triage outpost at Old Town Bridge",
            "Relocate vulnerable evacuees to North Community Hall & West Stadium"
        ];
        detail.Risks =
        [
            "Submerged access roads may isolate Sector 4 if rainfall exceeds 35mm in next 6 hours",
            "Municipal tap water contamination in Eastbank requires immediate alternative drinking water"
        ];
        detail.Assumptions =
        [
            "Depot road infrastructure remains passable for high-clearance supply trucks",
            "Volunteer communications network remains operational on primary cellular frequencies"
        ];
        detail.MissingInformation =
        [
            "Water levels on South Riverton railway bridge",
            "Backup generator fuel status at Central Community Clinic"
        ];
    }

    private static void ApplyDeterministicResourceAllocation(AiRecommendationDetailDto detail)
    {
        detail.ResourceRecommendations =
        [
            new()
            {
                ResourceName = "Water - 10L",
                Category = "Food & Water",
                RecommendedQuantity = 2400,
                AvailableQuantity = 8420,
                IsShortage = false,
                ShortageQuantity = 0,
                TargetCrisisLocation = "North Ward Depot",
                Priority = "Critical",
                Rationale = "Covers essential hydration for 800 affected families for 72 hours",
                AlternativeResource = "Water purification tablets (100-pack)"
            },
            new()
            {
                ResourceName = "Family Food Packs",
                Category = "Food & Water",
                RecommendedQuantity = 600,
                AvailableQuantity = 1284,
                IsShortage = false,
                ShortageQuantity = 0,
                TargetCrisisLocation = "Sector 4 Relief Center",
                Priority = "High",
                Rationale = "Shelf-stable meal packs for marooned communities",
                AlternativeResource = "Bulk grains and canned pulses"
            },
            new()
            {
                ResourceName = "Emergency Trauma Kits",
                Category = "Medical Supplies",
                RecommendedQuantity = 40,
                AvailableQuantity = 120,
                IsShortage = false,
                ShortageQuantity = 0,
                TargetCrisisLocation = "Old Town First Aid Post",
                Priority = "High",
                Rationale = "Laceration and infection treatment in waterlogged conditions",
                AlternativeResource = "Standard first aid responder packs"
            },
            new()
            {
                ResourceName = "Specialist Water Rescue Boats",
                Category = "Search & Rescue Equipment",
                RecommendedQuantity = 8,
                AvailableQuantity = 3,
                IsShortage = true,
                ShortageQuantity = 5,
                TargetCrisisLocation = "Riverside Landing",
                Priority = "Critical",
                Rationale = "Required to navigate residential alleys; existing stock is insufficient",
                AlternativeResource = "Inflatable rafts from military auxiliary partner"
            }
        ];
    }

    private static void ApplyDeterministicShelterMatching(AiRecommendationDetailDto detail, int evacuees)
    {
        detail.ShelterRecommendations =
        [
            new()
            {
                ShelterName = "West District Stadium",
                Location = "West Ward",
                Capacity = 1200,
                CurrentOccupancy = 750,
                RemainingCapacity = 450,
                RecommendedEvacueeCount = Math.Min(evacuees, 300),
                SuitabilityReason = "Highest remaining capacity (450 beds), auxiliary generator power, dedicated industrial kitchen",
                SafetyAndAccessibilityNotes = "Full wheelchair accessibility, separate family and senior accommodation wings"
            },
            new()
            {
                ShelterName = "North Community Hall",
                Location = "North Ward",
                Capacity = 500,
                CurrentOccupancy = 284,
                RemainingCapacity = 216,
                RecommendedEvacueeCount = Math.Min(Math.Max(0, evacuees - 300), 150),
                SuitabilityReason = "Proximity to active flood zone reduces transport travel time to under 12 minutes",
                SafetyAndAccessibilityNotes = "Certified emergency medical staff stationed 24/7"
            }
        ];
    }

    private static void ApplyDeterministicVolunteerPlan(AiRecommendationDetailDto detail)
    {
        detail.VolunteerRecommendations =
        [
            new()
            {
                SkillGroup = "Search & Rescue",
                RecommendedCount = 24,
                SuggestedTask = "Conduct door-to-door welfare checks and assist boat evacuations",
                Priority = "Critical",
                Rationale = "Essential for swift residential extraction prior to sunset"
            },
            new()
            {
                SkillGroup = "First Aid & Nursing",
                RecommendedCount = 18,
                SuggestedTask = "Operate intake triage and medication reconciliation at West Stadium",
                Priority = "High",
                Rationale = "Prevents hospital overcrowding by treating minor injuries on site"
            },
            new()
            {
                SkillGroup = "Logistics & Warehousing",
                RecommendedCount = 30,
                SuggestedTask = "Unload palletized water deliveries and pack family relief kits",
                Priority = "High",
                Rationale = "Central Depot dispatch bottleneck requires manual staging support"
            }
        ];
    }

    private static AiRecommendationSummaryDto MapToSummary(AiRecommendationDetailDto detail)
    {
        return new AiRecommendationSummaryDto
        {
            Id = detail.Id,
            CrisisId = detail.CrisisId,
            CrisisName = detail.CrisisName,
            RequestId = detail.RequestId,
            Category = detail.Category,
            Title = detail.Title,
            RecommendedPriority = detail.RecommendedPriority,
            Confidence = detail.Confidence,
            Action = detail.RecommendedAction,
            Timestamp = detail.SubmittedTime.Length > 0 ? detail.SubmittedTime : detail.CreatedAt.ToString("HH:mm UTC"),
            ReviewState = detail.ReviewState,
            Explanation = detail.Explanation,
            Alternative = detail.Alternatives.Count > 0 ? $"{detail.Alternatives[0].Priority} ({detail.Alternatives[0].LikelihoodPercentage}%)" : "None",
            Location = detail.Location,
            DecidedPriority = detail.DecidedPriority,
            ReviewedBy = detail.ReviewedBy,
            ReviewedAt = detail.ReviewedAt,
            IsLiveAiGenerated = detail.IsLiveAiGenerated
        };
    }

    private async Task LogAuditAsync(string action, string actor, string record, string change, string rationale, string type = "System")
    {
        try
        {
            await _auditLogService.CreateAuditLogAsync(new CareLink.Application.DTOs.AuditLogs.CreateAuditLogRequestDto
            {
                Action = action,
                Actor = actor,
                Record = record,
                Change = change,
                Rationale = rationale,
                Type = type
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to log audit event: {Action}", action);
        }
    }
}
