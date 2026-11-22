using CareLink.Application.DTOs.AiRecommendations;
using CareLink.Application.DTOs.AuditLogs;
using CareLink.Application.Interfaces;
using CareLink.Application.Services;
using CareLink.Tests.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CareLink.Tests.UnitTests.Services;

public class ShelterRecommendationTests
{
    private readonly FakeGeminiService _fakeGemini;
    private readonly Mock<IAuditLogService> _auditLogMock;
    private readonly Mock<IAiRecommendationRepository> _repositoryMock;
    private readonly AiRecommendationService _service;

    public ShelterRecommendationTests()
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
    public async Task GenerateShelterRecommendationsAsync_ValidCrisis_ValidatesCapacityAndOccupancy()
    {
        // Arrange
        const string crisisId = "CR-2025-018";
        var request = new GenerateShelterRecommendationRequestDto
        {
            CrisisId = crisisId,
            EvacueesCount = 120
        };

        // Act
        var result = await _service.GenerateShelterRecommendationsAsync(crisisId, request, "Shelter Officer");

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("ShelterMatching", result.Data.Category);
        Assert.NotEmpty(result.Data.ShelterRecommendations);

        // Every recommended shelter must strictly satisfy relational capacity rules
        foreach (var shelter in result.Data.ShelterRecommendations)
        {
            Assert.True(shelter.Capacity > 0, $"Shelter '{shelter.ShelterName}' capacity must be positive");
            Assert.True(shelter.CurrentOccupancy >= 0, $"Shelter '{shelter.ShelterName}' occupancy cannot be negative");
            Assert.True(shelter.Capacity >= shelter.CurrentOccupancy,
                $"Shelter '{shelter.ShelterName}' capacity ({shelter.Capacity}) cannot be less than occupancy ({shelter.CurrentOccupancy})");
            
            var expectedRemaining = shelter.Capacity - shelter.CurrentOccupancy;
            Assert.Equal(expectedRemaining, shelter.RemainingCapacity);
            
            // Recommends intake only if there is available remaining capacity
            if (shelter.RecommendedEvacueeCount > 0)
            {
                Assert.True(shelter.RemainingCapacity >= shelter.RecommendedEvacueeCount,
                    $"Recommended evacuee allocation ({shelter.RecommendedEvacueeCount}) cannot exceed remaining shelter capacity ({shelter.RemainingCapacity})");
            }
        }

        // Verify audit trail
        _auditLogMock.Verify(a => a.CreateAuditLogAsync(It.Is<CreateAuditLogRequestDto>(
            r => r.Action == "AI Shelter Recommendations Generated"), default), Times.Once);
    }

    [Fact]
    public async Task GenerateShelterRecommendationsAsync_DoesNotInventShelterCapacity()
    {
        // Act
        var result = await _service.GenerateShelterRecommendationsAsync("CR-2025-018", new GenerateShelterRecommendationRequestDto
        {
            EvacueesCount = 450
        }, "Shelter Officer");

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);

        var totalAllocated = result.Data.ShelterRecommendations.Sum(s => s.RecommendedEvacueeCount);
        var totalAvailableRemaining = result.Data.ShelterRecommendations.Sum(s => s.RemainingCapacity);

        // Total allocated evacuees must never exceed verified total remaining capacity
        Assert.True(totalAllocated <= totalAvailableRemaining,
            $"Total allocated evacuees ({totalAllocated}) must never exceed total remaining shelter capacity ({totalAvailableRemaining})");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task GenerateShelterRecommendationsAsync_EmptyCrisisId_ReturnsFailure(string? invalidCrisisId)
    {
        // Act
        var result = await _service.GenerateShelterRecommendationsAsync(
            invalidCrisisId!, new GenerateShelterRecommendationRequestDto(), "Shelter Officer");

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Crisis ID is required", result.Message);
    }
}
