using System.Collections.Concurrent;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;

namespace CareLink.Infrastructure.Repositories;

public class InMemoryPartnerDashboardRepository : IPartnerDashboardRepository
{
    private readonly object _lock = new();
    private PartnerDashboardData _data = new();

    public InMemoryPartnerDashboardRepository()
    {
        SeedDefaultData();
    }

    public Task<PartnerDashboardData> GetDashboardDataAsync(
        string? partnerId = null,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            return Task.FromResult(_data);
        }
    }

    public Task<PartnerActivityItem> LogActivityAsync(
        PartnerActivityItem activity,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _data.RecentActivities.Insert(0, activity);
            _data.LastUpdated = DateTimeOffset.UtcNow;
            return Task.FromResult(activity);
        }
    }

    public Task ResetToDefaultAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            SeedDefaultData();
            return Task.CompletedTask;
        }
    }

    private void SeedDefaultData()
    {
        var now = DateTimeOffset.UtcNow;

        _data = new PartnerDashboardData
        {
            ActiveCrisisName = "Monsoon Floods 2025",
            Summary = new PartnerDashboardSummary
            {
                ActiveCrisisEvents = 8,
                ActiveCrisisDetail = "3 require attention",
                AssistanceReferrals = 24,
                AssistanceReferralsDetail = "12 awaiting response",
                SharedResources = 16,
                SharedResourcesDetail = "Across 5 locations",
                ActiveTeamMembers = 32,
                ActiveTeamMembersDetail = "4 field teams deployed"
            },
            Organisation = new PartnerOrganisationProfileSummary
            {
                Name = "Global Relief Network",
                Type = "International NGO",
                Code = "ORG-027",
                VerificationStatus = "Verified",
                AssignedRegion = "Western",
                FieldLocationsCount = 5,
                ContactEmail = "contact@globalrelief.org",
                ContactPhone = "+94 11 234 5678"
            },
            RecentActivities = new List<PartnerActivityItem>
            {
                new()
                {
                    Id = "ACT-1001",
                    Title = "New assistance referral received",
                    Description = "Flood relief support requested in Gampaha District",
                    Time = "10 minutes ago",
                    IconType = "ClipboardList",
                    ColorClass = "text-blue-600 bg-blue-50",
                    Timestamp = now.AddMinutes(-10)
                },
                new()
                {
                    Id = "ACT-1002",
                    Title = "Resource sharing request updated",
                    Description = "Medical supplies request status changed to In Review",
                    Time = "35 minutes ago",
                    IconType = "Package",
                    ColorClass = "text-emerald-600 bg-emerald-50",
                    Timestamp = now.AddMinutes(-35)
                },
                new()
                {
                    Id = "ACT-1003",
                    Title = "Collaboration request received",
                    Description = "Regional response coordination invitation",
                    Time = "1 hour ago",
                    IconType = "Handshake",
                    ColorClass = "text-violet-600 bg-violet-50",
                    Timestamp = now.AddHours(-1)
                },
                new()
                {
                    Id = "ACT-1004",
                    Title = "Field activity logged",
                    Description = "Water purification unit deployed at Kelaniya High School",
                    Time = "2 hours ago",
                    IconType = "Activity",
                    ColorClass = "text-amber-600 bg-amber-50",
                    Timestamp = now.AddHours(-2)
                }
            },
            LastUpdated = now
        };
    }
}
