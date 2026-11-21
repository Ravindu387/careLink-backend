using CareLink.Application.DTOs;
using CareLink.Core.Entities;

namespace CareLink.Application.Interfaces;

public interface IAIInferenceEngine
{
    Recommendation InferRecommendation(GenerateRecommendationRequestDto request);
}
