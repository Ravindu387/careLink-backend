using CareLink.Application.DTOs.AiRecommendations;
using CareLink.Application.Interfaces;
using CareLink.Application.Services;
using CareLink.Tests.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CareLink.Tests.UnitTests.Services;

public class ResourceRecommendationTests
{
    private readonly FakeGeminiService _fakeGemini;
    private readonly Mock<IAuditLogService> _auditLogMock;
    private readonly Mock<IAiRecommendationRepository> _repositoryMock;
    private readonly AiRecommendationService _service;

    public ResourceRecommendationTests()
    {
        _fakeGemini = new FakeGeminiService();
        _auditLogMock = new Mock<IAuditLogService>();
        _repositoryMock = new Mock<IAiRecommendationRepository>();

        _service = new AiRecommendationService(
            _fakeGemini,
            _auditLogMock.Object,
            NullLogger<AiRecommendationService>.Instance,
            _repositoryMock.Object);
    }

    [Fact]
    public async Task GenerateResourceRecommendationsAsync_ValidCrisis_PassesInventoryDataToAi()
    {
        // Arrange
        const string crisisId = "CR-2025-018";
        const string crisisName = "Monsoon Floods 2025";
        var request = new GenerateResourceRecommendationRequestDto
        {
            CrisisId = crisisId,
            CrisisName = crisisName,
            TargetLocation = "North Ward Depot",
            Urgency = "High"
        };

        _fakeGemini.CustomStructuredResult = new GeminiResourceResponse
        {
            Summary = "Optimized dispatch: 2,500 units Water 10L allocated from Central Depot stock of 8,420.",
            RecommendedPriority = "High",
            Confidence = 92,
            Explanation = "Demand fits within depot stock with surplus margin.",
            RecommendedAction = "Dispatch Batch 1 immediately",
            ImmediatePriorities = ["Load 5 supply trucks", "Establish distribution staging"],
            Risks = ["Bridge transit restrictions"],
            Assumptions = ["Road open"],
            MissingInformation = ["Fuel status at North Ward depot"],
            ResourcePlan =
            [
                new()
                {
                    ResourceName = "Water - 10L",
                    Category = "Food & Water",
                    RecommendedQuantity = 2500,
                    AvailableQuantity = 8420,
                    IsShortage = false,
                    ShortageQuantity = 0,
                    TargetCrisisLocation = "North Ward Depot",
                    Priority = "Critical",
                    Rationale = "Hydration ration for evacuees",
                    AlternativeResource = "Water Purification Sachets"
                }
            ]
        };

        // Act
        var result = await _service.GenerateResourceRecommendationsAsync(crisisId, request, "Logistics Officer");

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("ResourceAllocation", result.Data.Category);
        Assert.Equal(crisisId, result.Data.CrisisId);
        Assert.NotEmpty(result.Data.ResourceRecommendations);

        var firstResource = result.Data.ResourceRecommendations[0];
        Assert.Equal("Water - 10L", firstResource.ResourceName);
        Assert.Equal(2500, firstResource.RecommendedQuantity);
        Assert.Equal(8420, firstResource.AvailableQuantity);
        Assert.False(firstResource.IsShortage);
        Assert.Equal(0, firstResource.ShortageQuantity);

        // Verify Gemini prompt contained depot inventory telemetry
        Assert.Equal(1, _fakeGemini.CallCount);
        Assert.Contains("Water 10L: 8,420 units available", _fakeGemini.LastPrompt);
        Assert.Contains("Family Food Packs: 1,284 units available", _fakeGemini.LastPrompt);
    }

    [Fact]
    public async Task GenerateResourceRecommendationsAsync_WhenAllocationExceedsStock_ExplicitlyFlagsShortage()
    {
        // Arrange
        const string crisisId = "CR-2025-018";
        var request = new GenerateResourceRecommendationRequestDto
        {
            CrisisId = crisisId,
            TargetLocation = "Old Town"
        };

        // Simulated AI allocation where demand exceeds available stock
        _fakeGemini.CustomStructuredResult = new GeminiResourceResponse
        {
            Summary = "Severe shortage detected for Emergency Trauma Kits.",
            RecommendedPriority = "Critical",
            Confidence = 90,
            Explanation = "Demand exceeds available inventory.",
            RecommendedAction = "Request regional mutual aid replenishment.",
            ImmediatePriorities = ["Dispatch all available 120 kits", "Procure emergency additional kits"],
            Risks = ["Depleted inventory in adjacent sectors"],
            Assumptions = ["Supplier turnaround within 24h"],
            MissingInformation = ["Local dispensary stockpile"],
            ResourcePlan =
            [
                new()
                {
                    ResourceName = "Emergency Trauma Kits",
                    Category = "Medical Supplies",
                    RecommendedQuantity = 250,
                    AvailableQuantity = 120,
                    IsShortage = true,
                    ShortageQuantity = 130, // 250 - 120 = 130 shortage
                    TargetCrisisLocation = "Old Town Sector 4",
                    Priority = "Critical",
                    Rationale = "Severe casualties requiring surgical and sterile dressing packs",
                    AlternativeResource = "First Aid Responder Pack"
                }
            ]
        };

        // Act
        var result = await _service.GenerateResourceRecommendationsAsync(crisisId, request, "Logistics Officer");

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.NotEmpty(result.Data.ResourceRecommendations);

        var shortageItem = result.Data.ResourceRecommendations[0];
        Assert.True(shortageItem.IsShortage, "Resource allocation exceeding depot inventory must explicitly set IsShortage = true");
        Assert.Equal(130, shortageItem.ShortageQuantity);
        Assert.True(shortageItem.RecommendedQuantity > shortageItem.AvailableQuantity);
    }

    [Fact]
    public async Task GenerateResourceRecommendationsAsync_GeminiFails_MaintainsGroundedDeterministicPlan()
    {
        // Arrange
        _fakeGemini.Behavior = FakeGeminiBehavior.ApiError;
        const string crisisId = "CR-2025-018";

        // Act
        var result = await _service.GenerateResourceRecommendationsAsync(
            crisisId, new GenerateResourceRecommendationRequestDto(), "Logistics Officer");

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.False(result.Data.IsLiveAiGenerated);
        Assert.NotEmpty(result.Data.ResourceRecommendations);

        // Grounded plan never exceeds available stock without shortage
        foreach (var item in result.Data.ResourceRecommendations)
        {
            if (item.RecommendedQuantity > item.AvailableQuantity)
            {
                Assert.True(item.IsShortage);
                Assert.True(item.ShortageQuantity > 0);
            }
            else
            {
                Assert.False(item.IsShortage);
                Assert.Equal(0, item.ShortageQuantity);
            }
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task GenerateResourceRecommendationsAsync_EmptyCrisisId_ReturnsFailure(string? invalidCrisisId)
    {
        // Act
        var result = await _service.GenerateResourceRecommendationsAsync(
            invalidCrisisId!, new GenerateResourceRecommendationRequestDto(), "Logistics Officer");

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Crisis ID is required", result.Message);
        Assert.Equal(0, _fakeGemini.CallCount);
    }
}
