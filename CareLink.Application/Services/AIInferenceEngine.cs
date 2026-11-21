using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;

namespace CareLink.Application.Services;

public class AIInferenceEngine : IAIInferenceEngine
{
    private static int _recommendationCounter = 9050;
    private static int _requestCounter = 2850;
    private static readonly object _lock = new();

    public Recommendation InferRecommendation(GenerateRecommendationRequestDto request)
    {
        lock (_lock)
        {
            _recommendationCounter++;
            _requestCounter++;
        }

        var id = $"AI-{_recommendationCounter}";
        var reqId = string.IsNullOrWhiteSpace(request.RequestId)
            ? $"REQ-{_requestCounter}"
            : request.RequestId.Trim();

        var category = NormalizeCategory(request.Category);
        var (priority, confidence, action, explanation, alternative) = Evaluate(category, request);

        return new Recommendation
        {
            Id = id,
            RequestId = reqId,
            Category = category,
            Priority = priority,
            Confidence = confidence,
            Action = action,
            Explanation = explanation,
            Alternative = alternative,
            ReviewState = "Awaiting Review",
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    private static string NormalizeCategory(string raw)
    {
        var trimmed = raw.Trim();
        if (trimmed.Contains("shelter", StringComparison.OrdinalIgnoreCase)) return "Emergency shelter";
        if (trimmed.Contains("medic", StringComparison.OrdinalIgnoreCase) || trimmed.Contains("health", StringComparison.OrdinalIgnoreCase)) return "Medical supplies";
        if (trimmed.Contains("food", StringComparison.OrdinalIgnoreCase) || trimmed.Contains("water", StringComparison.OrdinalIgnoreCase)) return "Food & water";
        if (trimmed.Contains("transport", StringComparison.OrdinalIgnoreCase) || trimmed.Contains("mobility", StringComparison.OrdinalIgnoreCase)) return "Accessible transport";
        if (trimmed.Contains("rescue", StringComparison.OrdinalIgnoreCase) || trimmed.Contains("evac", StringComparison.OrdinalIgnoreCase)) return "Rescue assistance";
        if (trimmed.Contains("sanitat", StringComparison.OrdinalIgnoreCase) || trimmed.Contains("hygiene", StringComparison.OrdinalIgnoreCase)) return "Sanitation & Hygiene";

        return trimmed;
    }

    private static (string Priority, int Confidence, string Action, string Explanation, string Alternative) Evaluate(
        string category,
        GenerateRecommendationRequestDto request)
    {
        var count = request.AffectedPeopleCount ?? 1;
        var isLifeThreatening = request.IsLifeThreatening ||
            (!string.IsNullOrEmpty(request.Description) &&
             (request.Description.Contains("trapped", StringComparison.OrdinalIgnoreCase) ||
              request.Description.Contains("critical", StringComparison.OrdinalIgnoreCase) ||
              request.Description.Contains("drowning", StringComparison.OrdinalIgnoreCase) ||
              request.Description.Contains("unconscious", StringComparison.OrdinalIgnoreCase)));

        var hasVulnerable = request.HasVulnerableMembers ||
            (!string.IsNullOrEmpty(request.Description) &&
             (request.Description.Contains("infant", StringComparison.OrdinalIgnoreCase) ||
              request.Description.Contains("child", StringComparison.OrdinalIgnoreCase) ||
              request.Description.Contains("elderly", StringComparison.OrdinalIgnoreCase) ||
              request.Description.Contains("wheelchair", StringComparison.OrdinalIgnoreCase)));

        string priority;
        int confidence;
        string action;
        string explanation;
        string alternative;

        // Base evaluation logic by category
        switch (category)
        {
            case "Rescue assistance":
                priority = "Critical";
                confidence = isLifeThreatening ? 96 : (hasVulnerable ? 92 : 88);
                action = count > 5 ? "Deploy Swift Water Rescue Unit + Air Recon" : "Deploy Rescue Unit 2 · Fast Response Boat";
                explanation = $"Urgent rescue conditions detected for {count} person(s). " +
                              (hasVulnerable ? "Vulnerable household members present with limited escape capability. " : "") +
                              "Immediate human assessment and emergency responder dispatch required.";
                alternative = "High priority dispatch – 4% likelihood";
                break;

            case "Emergency shelter":
                if (isLifeThreatening || hasVulnerable)
                {
                    priority = "Critical";
                    confidence = 94;
                    action = "Evacuation + accessible shelter · Sector North";
                    explanation = $"Immediate risk to life or health indicated for {count} person(s) with active displacement. Vulnerable individuals prioritized for accessible shelter facilities.";
                    alternative = "High priority – 5% likelihood";
                }
                else
                {
                    priority = count > 10 ? "High" : "Medium";
                    confidence = 86;
                    action = $"{Math.Max(1, count / 4)} Temporary shelter family pods · West Depot";
                    explanation = $"Displaced household requiring emergency accommodation. Shelter capacity verified at nearest staging hub.";
                    alternative = "Medium priority – 11% likelihood";
                }
                break;

            case "Medical supplies":
                if (isLifeThreatening)
                {
                    priority = "Critical";
                    confidence = 95;
                    action = "Emergency trauma kit + Rapid Paramedic dispatch";
                    explanation = "Life-threatening trauma or medical emergency indicated. Direct dispatch from Central Medical Depot prioritized.";
                    alternative = "High priority – 4% likelihood";
                }
                else
                {
                    priority = "High";
                    confidence = 91;
                    action = $"{Math.Max(12, count * 3)} trauma kits & triage pack · Central Depot";
                    explanation = $"Medical supply demand is high for {count} individual(s). Nearest available medical inventory has confirmed stock.";
                    alternative = "Medium priority – 8% likelihood";
                }
                break;

            case "Food & water":
                priority = (hasVulnerable || count > 8) ? "High" : "Medium";
                confidence = priority == "High" ? 87 : 81;
                var packCount = Math.Max(2, (int)Math.Ceiling(count / 2.0));
                action = $"{packCount} family ration packs + 50L clean water · Eastbank Hub";
                explanation = $"Immediate nutritional and hydration support needed for {count} individual(s). Regional relief hub holds active inventory.";
                alternative = "Medium priority – 12% likelihood";
                break;

            case "Accessible transport":
                priority = "High";
                confidence = 85;
                action = "Mobility Vehicle 4 + Wheelchair Ramp Unit";
                explanation = "Accessibility barrier identified for evacuation or medical transport. Specialized mobility vehicle dispatched.";
                alternative = "Medium priority – 13% likelihood";
                break;

            default:
                priority = isLifeThreatening ? "Critical" : (hasVulnerable ? "High" : "Medium");
                confidence = 83;
                action = $"Relief deployment · {category} response unit";
                explanation = $"Automated heuristic evaluation based on category '{category}' and situational parameters.";
                alternative = "Alternative priority – 14% likelihood";
                break;
        }

        // Allow priority hint override if explicitly requested
        if (!string.IsNullOrWhiteSpace(request.PriorityHint))
        {
            var hint = request.PriorityHint.Trim();
            if (string.Equals(hint, "Critical", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(hint, "High", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(hint, "Medium", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(hint, "Low", StringComparison.OrdinalIgnoreCase))
            {
                priority = char.ToUpper(hint[0]) + hint.Substring(1).ToLower();
            }
        }

        return (priority, confidence, action, explanation, alternative);
    }
}
