using CareLink.Core.Entities;
using CareLink.Infrastructure.Repositories;
using CareLink.Tests.Common;
using CareLink.Tests.Fixtures;

namespace CareLink.Tests.IntegrationTests;

public class DatabasePersistenceTests
{
    [Fact]
    public async Task Repository_CreateAndRetrieveRecommendation_PersistsToDatabase()
    {
        // Arrange: isolated SQLite In-Memory DB
        var (context, connection) = TestDbContextFactory.CreateSqliteInMemory();
        using (connection)
        using (context)
        {
            var repo = new AiRecommendationRepository(context);
            var entity = AiTestDataBuilder.CreateRecommendationEntity("AI-DB-001", "CR-2025-018", "CrisisAnalysis");

            // Act
            await repo.AddAsync(entity);
            var retrieved = await repo.GetByIdAsync("AI-DB-001");

            // Assert
            Assert.NotNull(retrieved);
            Assert.Equal("AI-DB-001", retrieved.Id);
            Assert.Equal("CR-2025-018", retrieved.CrisisId);
            Assert.Equal("CrisisAnalysis", retrieved.Category);
            Assert.Equal("Awaiting Review", retrieved.ReviewState);
        }
    }

    [Fact]
    public async Task Repository_ApprovalWorkflow_PersistsReviewStateAndReviewerInfo()
    {
        var (context, connection) = TestDbContextFactory.CreateSqliteInMemory();
        using (connection)
        using (context)
        {
            var repo = new AiRecommendationRepository(context);
            var entity = AiTestDataBuilder.CreateRecommendationEntity("AI-DB-002", "CR-2025-018");
            await repo.AddAsync(entity);

            // Act: approve the recommendation
            entity.ReviewState = "Approved";
            entity.DecidedPriority = "Critical";
            entity.DecisionRationale = "Ground field teams validated extreme flood risk.";
            entity.ReviewedBy = "Incident Commander Perera";
            entity.ReviewedAt = DateTime.UtcNow;

            await repo.UpdateAsync(entity);

            // Assert from fresh query
            var fresh = await repo.GetByIdAsync("AI-DB-002");
            Assert.NotNull(fresh);
            Assert.Equal("Approved", fresh.ReviewState);
            Assert.Equal("Critical", fresh.DecidedPriority);
            Assert.Equal("Incident Commander Perera", fresh.ReviewedBy);
            Assert.NotNull(fresh.ReviewedAt);
            Assert.Equal("Ground field teams validated extreme flood risk.", fresh.DecisionRationale);
        }
    }

    [Fact]
    public async Task Repository_RejectionWorkflow_PersistsRejectionReasonAndTimestamp()
    {
        var (context, connection) = TestDbContextFactory.CreateSqliteInMemory();
        using (connection)
        using (context)
        {
            var repo = new AiRecommendationRepository(context);
            var entity = AiTestDataBuilder.CreateRecommendationEntity("AI-DB-003", "CR-2025-018");
            await repo.AddAsync(entity);

            // Act: reject the recommendation
            entity.ReviewState = "Rejected";
            entity.RejectionReason = "Duplicate assistance case already dispatched under REQ-1049.";
            entity.ReviewedBy = "Officer Fernando";
            entity.ReviewedAt = DateTime.UtcNow;

            await repo.UpdateAsync(entity);

            // Assert
            var fresh = await repo.GetByIdAsync("AI-DB-003");
            Assert.NotNull(fresh);
            Assert.Equal("Rejected", fresh.ReviewState);
            Assert.Equal("Duplicate assistance case already dispatched under REQ-1049.", fresh.RejectionReason);
            Assert.Equal("Officer Fernando", fresh.ReviewedBy);
            Assert.NotNull(fresh.ReviewedAt);
        }
    }

    [Fact]
    public async Task Repository_GetAllAsync_FiltersByStatusAndCrisisId()
    {
        var (context, connection) = TestDbContextFactory.CreateSqliteInMemory();
        using (connection)
        using (context)
        {
            var repo = new AiRecommendationRepository(context);

            await repo.AddAsync(AiTestDataBuilder.CreateRecommendationEntity("AI-FILT-1", "CR-001", "CrisisAnalysis", "Critical", "Awaiting Review"));
            await repo.AddAsync(AiTestDataBuilder.CreateRecommendationEntity("AI-FILT-2", "CR-001", "ResourceAllocation", "High", "Approved"));
            await repo.AddAsync(AiTestDataBuilder.CreateRecommendationEntity("AI-FILT-3", "CR-002", "ShelterMatching", "Medium", "Awaiting Review"));

            // Filter by crisis CR-001
            var cr1Items = await repo.GetAllAsync(crisisId: "CR-001");
            Assert.Equal(2, cr1Items.Count);

            // Filter by status Approved
            var approvedItems = await repo.GetAllAsync(status: "Approved");
            Assert.Single(approvedItems);
            Assert.Equal("AI-FILT-2", approvedItems[0].Id);

            // Filter by category CrisisAnalysis
            var analysisItems = await repo.GetAllAsync(category: "CrisisAnalysis");
            Assert.Single(analysisItems);
            Assert.Equal("AI-FILT-1", analysisItems[0].Id);
        }
    }

    [Fact]
    public async Task Repository_DeleteAsync_RemovesEntityFromDatabase()
    {
        var (context, connection) = TestDbContextFactory.CreateSqliteInMemory();
        using (connection)
        using (context)
        {
            var repo = new AiRecommendationRepository(context);
            var entity = AiTestDataBuilder.CreateRecommendationEntity("AI-DEL-001", "CR-001");
            await repo.AddAsync(entity);

            var deleted = await repo.DeleteAsync("AI-DEL-001");
            Assert.True(deleted);

            var retrieved = await repo.GetByIdAsync("AI-DEL-001");
            Assert.Null(retrieved);
        }
    }
}
