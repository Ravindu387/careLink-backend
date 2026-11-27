using System.Security.Claims;
using CareLink.API.Controllers;
using CareLink.Application.Common;
using CareLink.Application.DTOs.AiRecommendations;
using CareLink.Application.Interfaces;
using CareLink.Application.Services;
using CareLink.Core.Constants;
using CareLink.Tests.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CareLink.Tests.IntegrationTests;

public class AiRecommendationsControllerTests
{
    private readonly FakeGeminiService _fakeGemini;
    private readonly Mock<IAuditLogService> _auditLogMock;
    private readonly AiRecommendationService _service;
    private readonly AiRecommendationsController _controller;

    public AiRecommendationsControllerTests()
    {
        _fakeGemini = new FakeGeminiService();
        _auditLogMock = new Mock<IAuditLogService>();
        _service = new AiRecommendationService(
            _fakeGemini,
            _auditLogMock.Object,
            NullLogger<AiRecommendationService>.Instance);

        _controller = new AiRecommendationsController(_service);

        // Setup controller context with Admin identity
        var user = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, "admin-user-01"),
            new Claim(ClaimTypes.Name, "Admin Reviewer"),
            new Claim(ClaimTypes.Role, UserRoles.Admin)
        ], "TestAuth"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task GetMetrics_ReturnsOkWithMetricsData()
    {
        // Act
        var actionResult = await _controller.GetMetrics();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var apiResponse = Assert.IsType<ApiResponse<AiRecommendationMetricsDto>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal(8, apiResponse.Data.TotalCases);
        Assert.Equal(8, apiResponse.Data.AwaitingReviewCount);
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsOkWithDetail()
    {
        // Act
        var actionResult = await _controller.GetById("AI-9041");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var apiResponse = Assert.IsType<ApiResponse<AiRecommendationDetailDto>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.Equal("AI-9041", apiResponse.Data?.Id);
        Assert.Equal("Critical", apiResponse.Data?.RecommendedPriority);
    }

    [Fact]
    public async Task GetById_NonexistentId_ReturnsNotFound()
    {
        // Act
        var actionResult = await _controller.GetById("AI-NONEXISTENT");

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult);
        var apiResponse = Assert.IsType<ApiResponse<AiRecommendationDetailDto>>(notFoundResult.Value);
        Assert.False(apiResponse.Success);
        Assert.Contains("not found", apiResponse.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ApproveRecommendation_ValidRequest_ReturnsOkWithApprovedState()
    {
        // Arrange
        var request = new ApproveRecommendationRequestDto
        {
            DecidedPriority = "Critical",
            Notes = "Approved by commander."
        };

        // Act
        var actionResult = await _controller.ApproveRecommendation("AI-9038", request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var apiResponse = Assert.IsType<ApiResponse<AiRecommendationDetailDto>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.Equal("Approved", apiResponse.Data?.ReviewState);
        Assert.Equal("Admin Reviewer", apiResponse.Data?.ReviewedBy);
    }

    [Fact]
    public async Task RejectRecommendation_MissingReason_ReturnsBadRequest()
    {
        // Arrange
        var request = new RejectRecommendationRequestDto
        {
            RejectionReason = "" // empty reason
        };

        // Act
        var actionResult = await _controller.RejectRecommendation("AI-9032", request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult);
        var apiResponse = Assert.IsType<ApiResponse<AiRecommendationDetailDto>>(badRequestResult.Value);
        Assert.False(apiResponse.Success);
        Assert.Contains("rejection reason is required", apiResponse.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RejectRecommendation_ValidReason_ReturnsOkWithRejectedState()
    {
        // Arrange
        var request = new RejectRecommendationRequestDto
        {
            RejectionReason = "Already addressed by local team."
        };

        // Act
        var actionResult = await _controller.RejectRecommendation("AI-9032", request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var apiResponse = Assert.IsType<ApiResponse<AiRecommendationDetailDto>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.Equal("Rejected", apiResponse.Data?.ReviewState);
    }

    [Fact]
    public async Task GenerateCrisisAnalysis_ValidCrisisId_ReturnsOk()
    {
        // Arrange
        var request = new GenerateCrisisAnalysisRequestDto
        {
            CrisisId = "CR-2025-018",
            CrisisName = "Monsoon Floods 2025"
        };

        // Act
        var actionResult = await _controller.GenerateCrisisAnalysis("CR-2025-018", request, default);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var apiResponse = Assert.IsType<ApiResponse<AiRecommendationDetailDto>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.Equal("CrisisAnalysis", apiResponse.Data?.Category);
    }

    [Fact]
    public async Task GenerateCrisisAnalysis_EmptyCrisisId_ReturnsBadRequest()
    {
        // Act
        var actionResult = await _controller.GenerateCrisisAnalysis("", new GenerateCrisisAnalysisRequestDto(), default);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult);
        var apiResponse = Assert.IsType<ApiResponse<AiRecommendationDetailDto>>(badRequestResult.Value);
        Assert.False(apiResponse.Success);
        Assert.Contains("Crisis ID is required", apiResponse.Message);
    }
}
