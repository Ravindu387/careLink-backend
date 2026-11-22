using CareLink.Application.DTOs.AiRecommendations;
using CareLink.Application.DTOs.AuditLogs;
using CareLink.Application.Interfaces;
using CareLink.Application.Services;
using CareLink.Tests.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CareLink.Tests.UnitTests.Services;

public class ApproveRejectWorkflowTests
{
    private readonly FakeGeminiService _fakeGemini;
    private readonly Mock<IAuditLogService> _auditLogMock;
    private readonly Mock<IAiRecommendationRepository> _repositoryMock;
    private readonly AiRecommendationService _service;

    public ApproveRejectWorkflowTests()
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
    public async Task ApproveRecommendationAsync_PendingItem_UpdatesStatusAndPersistsReviewer()
    {
        // Arrange
        const string recId = "AI-9041";
        const string reviewer = "Commander Alex";
        var request = new ApproveRecommendationRequestDto
        {
            DecidedPriority = "Critical",
            Notes = "Field team confirmed severe rising waters."
        };

        // Act
        var response = await _service.ApproveRecommendationAsync(recId, request, reviewer);

        // Assert API response
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal("Approved", response.Data.ReviewState);
        Assert.Equal("Critical", response.Data.DecidedPriority);
        Assert.Equal(reviewer, response.Data.ReviewedBy);
        Assert.NotNull(response.Data.ReviewedAt);
        Assert.Equal(request.Notes, response.Data.DecisionRationale);

        // VERIFY PERSISTENCE BY INDEPENDENT RETRIEVAL
        var retrieved = await _service.GetRecommendationByIdAsync(recId);
        Assert.True(retrieved.Success);
        Assert.NotNull(retrieved.Data);
        Assert.Equal("Approved", retrieved.Data.ReviewState);
        Assert.Equal(reviewer, retrieved.Data.ReviewedBy);
        Assert.NotNull(retrieved.Data.ReviewedAt);
        Assert.Equal(request.Notes, retrieved.Data.DecisionRationale);

        // Verify audit trail
        _auditLogMock.Verify(a => a.CreateAuditLogAsync(It.Is<CreateAuditLogRequestDto>(
            r => r.Action == "AI Recommendation Approved" && r.Actor == reviewer), default), Times.Once);
    }

    [Fact]
    public async Task RejectRecommendationAsync_WithMandatoryReason_UpdatesStatusAndPersistsRejection()
    {
        // Arrange
        const string recId = "AI-9038";
        const string reviewer = "Dr. Silva";
        var request = new RejectRecommendationRequestDto
        {
            RejectionReason = "Local hospital team already delivered emergency insulin rations.",
            Notes = "Verified by mobile clinic driver at 13:15."
        };

        // Act
        var response = await _service.RejectRecommendationAsync(recId, request, reviewer);

        // Assert response
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal("Rejected", response.Data.ReviewState);
        Assert.Equal(request.RejectionReason, response.Data.RejectionReason);
        Assert.Equal(reviewer, response.Data.ReviewedBy);
        Assert.NotNull(response.Data.ReviewedAt);

        // VERIFY PERSISTENCE BY INDEPENDENT RETRIEVAL
        var retrieved = await _service.GetRecommendationByIdAsync(recId);
        Assert.True(retrieved.Success);
        Assert.NotNull(retrieved.Data);
        Assert.Equal("Rejected", retrieved.Data.ReviewState);
        Assert.Equal(request.RejectionReason, retrieved.Data.RejectionReason);
        Assert.Equal(reviewer, retrieved.Data.ReviewedBy);

        // Verify audit trail
        _auditLogMock.Verify(a => a.CreateAuditLogAsync(It.Is<CreateAuditLogRequestDto>(
            r => r.Action == "AI Recommendation Rejected" && r.Actor == reviewer), default), Times.Once);
    }

    [Fact]
    public async Task RejectRecommendationAsync_MissingRejectionReason_ReturnsFailure()
    {
        // Arrange
        const string recId = "AI-9032";
        var request = new RejectRecommendationRequestDto
        {
            RejectionReason = "   " // empty/whitespace
        };

        // Act
        var response = await _service.RejectRecommendationAsync(recId, request, "Officer");

        // Assert
        Assert.False(response.Success);
        Assert.Contains("rejection reason is required", response.Message, StringComparison.OrdinalIgnoreCase);

        // Verify recommendation was NOT rejected
        var retrieved = await _service.GetRecommendationByIdAsync(recId);
        Assert.Equal("Awaiting Review", retrieved.Data?.ReviewState);
    }

    [Fact]
    public async Task PreventInvalidStatusTransition_CannotApproveRejectedRecommendationWithoutReset()
    {
        // Arrange: first reject AI-9024
        const string recId = "AI-9024";
        await _service.RejectRecommendationAsync(recId, new RejectRecommendationRequestDto
        {
            RejectionReason = "Not needed"
        }, "Reviewer");

        // Act: try to approve directly
        var approveAttempt = await _service.ApproveRecommendationAsync(recId, new ApproveRecommendationRequestDto(), "Reviewer 2");

        // Assert
        Assert.False(approveAttempt.Success);
        Assert.Contains("currently rejected", approveAttempt.Message);

        // Verify still rejected
        var check = await _service.GetRecommendationByIdAsync(recId);
        Assert.Equal("Rejected", check.Data?.ReviewState);
    }

    [Fact]
    public async Task PreventInvalidStatusTransition_CannotRejectApprovedRecommendationWithoutReset()
    {
        // Arrange: first approve AI-9019
        const string recId = "AI-9019";
        await _service.ApproveRecommendationAsync(recId, new ApproveRecommendationRequestDto(), "Reviewer");

        // Act: try to reject directly
        var rejectAttempt = await _service.RejectRecommendationAsync(recId, new RejectRecommendationRequestDto
        {
            RejectionReason = "Change of mind"
        }, "Reviewer 2");

        // Assert
        Assert.False(rejectAttempt.Success);
        Assert.Contains("already approved", rejectAttempt.Message);

        // Verify still approved
        var check = await _service.GetRecommendationByIdAsync(recId);
        Assert.Equal("Approved", check.Data?.ReviewState);
    }

    [Fact]
    public async Task ResetRecommendationAsync_ResetsApprovedItemBackToAwaitingReview()
    {
        // Arrange: approve item
        const string recId = "AI-9015";
        await _service.ApproveRecommendationAsync(recId, new ApproveRecommendationRequestDto(), "Officer");

        // Act: reset
        var resetResult = await _service.ResetRecommendationAsync(recId);

        // Assert
        Assert.True(resetResult.Success);
        Assert.Equal("Awaiting Review", resetResult.Data?.ReviewState);
        Assert.Null(resetResult.Data?.ReviewedBy);
        Assert.Null(resetResult.Data?.ReviewedAt);

        // Verify persistence
        var retrieved = await _service.GetRecommendationByIdAsync(recId);
        Assert.Equal("Awaiting Review", retrieved.Data?.ReviewState);
    }

    [Fact]
    public async Task ApproveRecommendationAsync_NonexistentId_ReturnsFailure()
    {
        // Act
        var result = await _service.ApproveRecommendationAsync("AI-DOES-NOT-EXIST", new ApproveRecommendationRequestDto(), "Reviewer");

        // Assert
        Assert.False(result.Success);
        Assert.Contains("not found", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RejectRecommendationAsync_NonexistentId_ReturnsFailure()
    {
        // Act
        var result = await _service.RejectRecommendationAsync("AI-DOES-NOT-EXIST", new RejectRecommendationRequestDto
        {
            RejectionReason = "Valid reason"
        }, "Reviewer");

        // Assert
        Assert.False(result.Success);
        Assert.Contains("not found", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DeferRecommendationAsync_UpdatesStatusToDeferred()
    {
        // Arrange
        const string recId = "AI-9012";

        // Act
        var result = await _service.DeferRecommendationAsync(recId, new DeferRecommendationRequestDto
        {
            Reason = "Awaiting satellite GIS flood update."
        }, "Coordinator");

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Deferred", result.Data?.ReviewState);

        var retrieved = await _service.GetRecommendationByIdAsync(recId);
        Assert.Equal("Deferred", retrieved.Data?.ReviewState);
    }
}
