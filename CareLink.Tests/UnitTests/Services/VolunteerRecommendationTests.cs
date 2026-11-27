using CareLink.Application.DTOs.AiRecommendations;
using CareLink.Application.DTOs.AuditLogs;
using CareLink.Application.Interfaces;
using CareLink.Application.Services;
using CareLink.Tests.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CareLink.Tests.UnitTests.Services;

public class VolunteerRecommendationTests
{
    private readonly FakeGeminiService _fakeGemini;
    private readonly Mock<IAuditLogService> _auditLogMock;
    private readonly Mock<IAiRecommendationRepository> _repositoryMock;
    private readonly AiRecommendationService _service;

    public VolunteerRecommendationTests()
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
    public async Task GenerateVolunteerRecommendationsAsync_ValidCrisis_AlignsSkillsWithPriorityTasks()
    {
        // Arrange
        const string crisisId = "CR-2025-018";
        var request = new GenerateVolunteerRecommendationRequestDto
        {
            CrisisId = crisisId,
            SkillFilter = "Swiftwater flood perimeter and sandbag logistics"
        };

        // Act
        var result = await _service.GenerateVolunteerRecommendationsAsync(crisisId, request, "Volunteer Coordinator");

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("VolunteerAssignment", result.Data.Category);
        Assert.NotEmpty(result.Data.VolunteerRecommendations);

        // Every recommended cohort must have a designated skill group, non-zero headcount, and priority
        foreach (var volunteerGroup in result.Data.VolunteerRecommendations)
        {
            Assert.False(string.IsNullOrWhiteSpace(volunteerGroup.SkillGroup), "Skill group must be specified");
            Assert.True(volunteerGroup.RecommendedCount > 0, "Recommended volunteer count must be positive");
            Assert.False(string.IsNullOrWhiteSpace(volunteerGroup.SuggestedTask), "Task must be defined");
            Assert.False(string.IsNullOrWhiteSpace(volunteerGroup.Rationale), "Skill matching rationale must be provided");
            Assert.Contains(volunteerGroup.Priority, new[] { "Critical", "High", "Medium", "Low" });
        }

        // Verify audit log
        _auditLogMock.Verify(a => a.CreateAuditLogAsync(It.Is<CreateAuditLogRequestDto>(
            r => r.Action == "AI Volunteer Recommendations Generated"), default), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task GenerateVolunteerRecommendationsAsync_EmptyCrisisId_ReturnsFailure(string? invalidCrisisId)
    {
        // Act
        var result = await _service.GenerateVolunteerRecommendationsAsync(
            invalidCrisisId!, new GenerateVolunteerRecommendationRequestDto(), "Volunteer Coordinator");

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Crisis ID is required", result.Message);
    }
}
