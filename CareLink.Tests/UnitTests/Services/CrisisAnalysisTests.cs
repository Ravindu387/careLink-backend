using CareLink.Application.DTOs.AiRecommendations;
using CareLink.Application.DTOs.AuditLogs;
using CareLink.Application.Interfaces;
using CareLink.Application.Services;
using CareLink.Tests.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CareLink.Tests.UnitTests.Services;

public class CrisisAnalysisTests
{
    private readonly FakeGeminiService _fakeGemini;
    private readonly Mock<IAuditLogService> _auditLogMock;
    private readonly Mock<IAiRecommendationRepository> _repositoryMock;
    private readonly AiRecommendationService _service;

    public CrisisAnalysisTests()
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
    public async Task GenerateCrisisAnalysisAsync_ValidCrisisId_InvokesGeminiAndReturnsAnalysis()
    {
        // Arrange
        const string crisisId = "CR-2025-018";
        const string crisisName = "Monsoon Floods 2025";
        var request = new GenerateCrisisAnalysisRequestDto
        {
            CrisisId = crisisId,
            CrisisName = crisisName,
            FocusArea = "Riverside and Old Town",
            AdditionalContext = "Severe river cresting. 1,284 affected households."
        };

        _fakeGemini.CustomStructuredResult = new GeminiAnalysisResponse
        {
            Summary = "Live Gemini analysis: Rapid river cresting requires urgent evacuation in Riverside.",
            RecommendedPriority = "Critical",
            Confidence = 95,
            Explanation = "Water level measured at +1.8m above flood stage based on telemetry.",
            RecommendedAction = "Deploy high-water rescue trucks and open North Community Hall.",
            ImmediatePriorities = ["Reinforce levee", "Evacuate children and seniors", "Set up mobile clinic"],
            Risks = ["Bridge collapse on Old Town span"],
            Assumptions = ["Road to north remains dry"],
            MissingInformation = ["Exact status of Sector 4 power substation"]
        };

        // Act
        var result = await _service.GenerateCrisisAnalysisAsync(crisisId, request, "Coordinator Test");

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(crisisId, result.Data.CrisisId);
        Assert.Equal(crisisName, result.Data.CrisisName);
        Assert.Equal("CrisisAnalysis", result.Data.Category);
        Assert.Equal("Critical", result.Data.RecommendedPriority);
        Assert.Equal(95, result.Data.Confidence);
        Assert.True(result.Data.IsLiveAiGenerated);
        Assert.Contains("Live Gemini analysis", result.Data.Summary);
        Assert.NotEmpty(result.Data.ImmediatePriorities);
        Assert.NotEmpty(result.Data.Risks);
        Assert.NotEmpty(result.Data.Assumptions);
        Assert.NotEmpty(result.Data.MissingInformation);

        // Verify Gemini was invoked with crisis context
        Assert.Equal(1, _fakeGemini.CallCount);
        Assert.Contains(crisisId, _fakeGemini.LastPrompt);
        Assert.Contains(crisisName, _fakeGemini.LastPrompt);
        Assert.Contains("Riverside and Old Town", _fakeGemini.LastPrompt);

        // Verify audit log
        _auditLogMock.Verify(a => a.CreateAuditLogAsync(It.Is<CreateAuditLogRequestDto>(
            r => r.Action == "AI Crisis Analysis Generated"), default), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task GenerateCrisisAnalysisAsync_InvalidCrisisId_ReturnsFailure(string? invalidCrisisId)
    {
        // Arrange
        var request = new GenerateCrisisAnalysisRequestDto
        {
            CrisisId = invalidCrisisId ?? string.Empty
        };

        // Act
        var result = await _service.GenerateCrisisAnalysisAsync(invalidCrisisId!, request, "Coordinator Test");

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Crisis ID is required", result.Message);
        Assert.Equal(0, _fakeGemini.CallCount);
    }

    [Fact]
    public async Task GenerateCrisisAnalysisAsync_GeminiServiceFailure_GracefullyFallsBackToDeterministicBaseline()
    {
        // Arrange
        _fakeGemini.Behavior = FakeGeminiBehavior.ApiError;
        const string crisisId = "CR-2025-018";
        var request = new GenerateCrisisAnalysisRequestDto
        {
            CrisisId = crisisId,
            CrisisName = "Monsoon Floods 2025"
        };

        // Act
        var result = await _service.GenerateCrisisAnalysisAsync(crisisId, request, "Coordinator Test");

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.False(result.Data.IsLiveAiGenerated);
        Assert.Equal("Critical", result.Data.RecommendedPriority);
        Assert.NotEmpty(result.Data.Summary);
        Assert.NotEmpty(result.Data.ImmediatePriorities);
        Assert.NotEmpty(result.Data.Risks);
    }

    [Fact]
    public async Task GenerateCrisisAnalysisAsync_GeminiTimeout_GracefullyFallsBackWithoutCrashing()
    {
        // Arrange
        _fakeGemini.Behavior = FakeGeminiBehavior.Timeout;
        const string crisisId = "CR-2025-018";

        // Act
        var result = await _service.GenerateCrisisAnalysisAsync(crisisId, new GenerateCrisisAnalysisRequestDto(), "Coordinator Test");

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.False(result.Data.IsLiveAiGenerated);
        Assert.NotEmpty(result.Data.Explanation);
    }

    [Fact]
    public async Task GenerateCrisisAnalysisAsync_MalformedJsonFromAi_GracefullyFallsBack()
    {
        // Arrange
        _fakeGemini.Behavior = FakeGeminiBehavior.InvalidJson;
        const string crisisId = "CR-2025-018";

        // Act
        var result = await _service.GenerateCrisisAnalysisAsync(crisisId, new GenerateCrisisAnalysisRequestDto(), "Coordinator Test");

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.False(result.Data.IsLiveAiGenerated);
        Assert.NotEmpty(result.Data.RecommendedAction);
    }

    [Fact]
    public async Task GenerateCrisisAnalysisAsync_WhenCancelled_ThrowsOperationCanceledException()
    {
        // Arrange
        _fakeGemini.Behavior = FakeGeminiBehavior.Cancellation;
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
        {
            await _service.GenerateCrisisAnalysisAsync("CR-2025-018", new GenerateCrisisAnalysisRequestDto(), "Coordinator Test", cts.Token);
        });
    }

    [Fact]
    public async Task GenerateCrisisAnalysisAsync_WhenGeminiUnconfigured_UsesGroundedFallbackDirectly()
    {
        // Arrange
        _fakeGemini.Behavior = FakeGeminiBehavior.Unconfigured;
        const string crisisId = "CR-2025-018";

        // Act
        var result = await _service.GenerateCrisisAnalysisAsync(crisisId, new GenerateCrisisAnalysisRequestDto(), "Coordinator Test");

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.False(result.Data.IsLiveAiGenerated);
        Assert.Contains("Grounded", result.Data.ModelName);
        Assert.Equal(0, _fakeGemini.CallCount); // Gemini not called when unconfigured
    }
}
