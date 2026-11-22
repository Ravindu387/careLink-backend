using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;

namespace CareLink.Application.Services;

public class PartnerDashboardService : IPartnerDashboardService
{
    private readonly IPartnerDashboardRepository _repository;

    public PartnerDashboardService(IPartnerDashboardRepository repository)
    {
        _repository = repository;
    }

    public async Task<PartnerDashboardResponseDto> GetDashboardAsync(
        string? partnerId = null,
        CancellationToken cancellationToken = default)
    {
        var data = await _repository.GetDashboardDataAsync(partnerId, cancellationToken);

        var cards = new List<SummaryCardDto>
        {
            new()
            {
                Label = "Active Crisis Events",
                Value = data.Summary.ActiveCrisisEvents.ToString("D2"),
                Detail = data.Summary.ActiveCrisisDetail,
                Icon = "AlertTriangle",
                IconClass = "bg-amber-50 text-amber-600"
            },
            new()
            {
                Label = "Assistance Referrals",
                Value = data.Summary.AssistanceReferrals.ToString("D2"),
                Detail = data.Summary.AssistanceReferralsDetail,
                Icon = "ClipboardList",
                IconClass = "bg-blue-50 text-blue-600"
            },
            new()
            {
                Label = "Shared Resources",
                Value = data.Summary.SharedResources.ToString("D2"),
                Detail = data.Summary.SharedResourcesDetail,
                Icon = "Package",
                IconClass = "bg-emerald-50 text-emerald-600"
            },
            new()
            {
                Label = "Active Team Members",
                Value = data.Summary.ActiveTeamMembers.ToString("D2"),
                Detail = data.Summary.ActiveTeamMembersDetail,
                Icon = "Users",
                IconClass = "bg-violet-50 text-violet-600"
            }
        };

        var activities = data.RecentActivities
            .OrderByDescending(a => a.Timestamp)
            .Select(a => new PartnerActivityDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                Time = FormatRelativeTime(a.Timestamp),
                Icon = a.IconType,
                Color = a.ColorClass,
                Timestamp = a.Timestamp
            })
            .ToList();

        var org = new PartnerDashboardOrgDto
        {
            Name = data.Organisation.Name,
            Type = data.Organisation.Type,
            Code = data.Organisation.Code,
            VerificationStatus = data.Organisation.VerificationStatus,
            AssignedRegion = data.Organisation.AssignedRegion,
            FieldLocationsCount = data.Organisation.FieldLocationsCount,
            ContactEmail = data.Organisation.ContactEmail,
            ContactPhone = data.Organisation.ContactPhone
        };

        return new PartnerDashboardResponseDto
        {
            ActiveCrisisName = data.ActiveCrisisName,
            SummaryCards = cards,
            Activities = activities,
            Organisation = org,
            LastUpdated = data.LastUpdated
        };
    }

    public async Task<PartnerActivityDto> LogActivityAsync(
        CreatePartnerActivityDto dto,
        CancellationToken cancellationToken = default)
    {
        var item = new PartnerActivityItem
        {
            Id = $"ACT-{Random.Shared.Next(1000, 9999)}",
            Title = dto.Title.Trim(),
            Description = dto.Description.Trim(),
            Time = "Just now",
            IconType = string.IsNullOrWhiteSpace(dto.Icon) ? "ClipboardList" : dto.Icon.Trim(),
            ColorClass = string.IsNullOrWhiteSpace(dto.Color) ? "text-blue-600 bg-blue-50" : dto.Color.Trim(),
            Timestamp = DateTimeOffset.UtcNow
        };

        var created = await _repository.LogActivityAsync(item, cancellationToken);
        return new PartnerActivityDto
        {
            Id = created.Id,
            Title = created.Title,
            Description = created.Description,
            Time = "Just now",
            Icon = created.IconType,
            Color = created.ColorClass,
            Timestamp = created.Timestamp
        };
    }

    public async Task ResetToDefaultAsync(CancellationToken cancellationToken = default)
    {
        await _repository.ResetToDefaultAsync(cancellationToken);
    }

    private static string FormatRelativeTime(DateTimeOffset timestamp)
    {
        var diff = DateTimeOffset.UtcNow - timestamp;
        if (diff.TotalMinutes < 1) return "Just now";
        if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} minutes ago";
        if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} hour{((int)diff.TotalHours > 1 ? "s" : "")} ago";
        return $"{(int)diff.TotalDays} day{((int)diff.TotalDays > 1 ? "s" : "")} ago";
    }
}
