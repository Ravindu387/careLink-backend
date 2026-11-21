using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;

namespace CareLink.Application.Services;

public class EmergencyTopbarService : IEmergencyTopbarService
{
    private readonly IEmergencyTopbarRepository _topbarRepository;
    private readonly ICrisisEventRepository _crisisEventRepository;
    private readonly IAssistanceRequestRepository _assistanceRequestRepository;
    private readonly IEmergencySidebarRepository _sidebarRepository;

    public EmergencyTopbarService(
        IEmergencyTopbarRepository topbarRepository,
        ICrisisEventRepository crisisEventRepository,
        IAssistanceRequestRepository assistanceRequestRepository,
        IEmergencySidebarRepository sidebarRepository)
    {
        _topbarRepository = topbarRepository;
        _crisisEventRepository = crisisEventRepository;
        _assistanceRequestRepository = assistanceRequestRepository;
        _sidebarRepository = sidebarRepository;
    }

    public async Task<EmergencyTopbarStateDto> GetTopbarStateAsync(CancellationToken cancellationToken = default)
    {
        var rawState = await _topbarRepository.GetStateAsync(cancellationToken);

        // Optionally sync coordinator info from sidebar state
        try
        {
            var sidebarState = await _sidebarRepository.GetStateAsync(cancellationToken);
            rawState.CoordinatorName = sidebarState.Coordinator.Name;
            rawState.CoordinatorRole = sidebarState.Coordinator.Role;
            rawState.CoordinatorInitials = sidebarState.Coordinator.Initials;
            rawState.DutyStatus = sidebarState.Coordinator.DutyStatus;
            if (!string.IsNullOrWhiteSpace(sidebarState.Coordinator.OperationalArea))
            {
                rawState.OperationalArea = sidebarState.Coordinator.OperationalArea;
            }
        }
        catch
        {
            // use topbar defaults
        }

        var notificationDtos = rawState.Notifications
            .Select(MapToNotificationDto)
            .ToList();

        var unreadCount = notificationDtos.Count(n => !n.IsRead);

        return new EmergencyTopbarStateDto
        {
            OperationalArea = rawState.OperationalArea,
            SelectedLanguage = rawState.SelectedLanguage,
            SupportedLanguages = rawState.SupportedLanguages,
            UnreadNotificationCount = unreadCount,
            Notifications = notificationDtos,
            Helplines = rawState.Helplines.Select(h => new EmergencyHelplineDto
            {
                Name = h.Name,
                Number = h.Number,
                Description = h.Description,
                Department = h.Department,
                Is24x7 = h.Is24x7
            }).ToList(),
            CoordinatorName = rawState.CoordinatorName,
            CoordinatorRole = rawState.CoordinatorRole,
            CoordinatorInitials = rawState.CoordinatorInitials,
            DutyStatus = rawState.DutyStatus
        };
    }

    public async Task<IReadOnlyList<EmergencyNotificationDto>> GetNotificationsAsync(
        bool unreadOnly = false,
        CancellationToken cancellationToken = default)
    {
        var list = await _topbarRepository.GetNotificationsAsync(unreadOnly, cancellationToken);
        return list.Select(MapToNotificationDto).ToList();
    }

    public async Task<bool> MarkNotificationReadAsync(string notificationId, CancellationToken cancellationToken = default)
    {
        return await _topbarRepository.MarkNotificationReadAsync(notificationId, cancellationToken);
    }

    public async Task<int> MarkAllNotificationsReadAsync(CancellationToken cancellationToken = default)
    {
        return await _topbarRepository.MarkAllNotificationsReadAsync(cancellationToken);
    }

    public async Task<EmergencyNotificationDto> AddNotificationAsync(
        CreateNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        var notif = new EmergencyNotification
        {
            Id = $"NOTIF-{DateTime.UtcNow.Ticks % 100000:00000}",
            Title = request.Title,
            Message = request.Message,
            Category = request.Category,
            Severity = request.Severity,
            Timestamp = DateTimeOffset.UtcNow,
            IsRead = false,
            ActionUrl = request.ActionUrl,
            ActionLabel = request.ActionLabel
        };

        var created = await _topbarRepository.AddNotificationAsync(notif, cancellationToken);
        return MapToNotificationDto(created);
    }

    public async Task<string> UpdateOperationalAreaAsync(string area, CancellationToken cancellationToken = default)
    {
        return await _topbarRepository.UpdateOperationalAreaAsync(area, cancellationToken);
    }

    public async Task<string> UpdateLanguageAsync(string language, CancellationToken cancellationToken = default)
    {
        return await _topbarRepository.UpdateLanguageAsync(language, cancellationToken);
    }

    public async Task<IReadOnlyList<GlobalSearchResultDto>> SearchGlobalAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < 2)
        {
            return Array.Empty<GlobalSearchResultDto>();
        }

        var q = query.Trim().ToLowerInvariant();
        var results = new List<GlobalSearchResultDto>();

        // 1. Search Crisis Events
        try
        {
            var events = await _crisisEventRepository.GetAllAsync(null, cancellationToken);
            var matchedEvents = events
                .Where(e => e.Name.ToLowerInvariant().Contains(q) ||
                            e.Type.ToLowerInvariant().Contains(q) ||
                            e.Area.ToLowerInvariant().Contains(q) ||
                            e.Status.ToLowerInvariant().Contains(q))
                .Take(3);

            foreach (var e in matchedEvents)
            {
                results.Add(new GlobalSearchResultDto
                {
                    Category = "Crisis Incident",
                    Id = e.Id,
                    Title = e.Name,
                    Subtitle = $"{e.Area} · {e.Status}",
                    Badge = e.Severity,
                    Url = "/emergency-coordinator/crisis-events"
                });
            }
        }
        catch
        {
            // skip if error
        }

        // 2. Search Assistance Requests
        try
        {
            var requests = await _assistanceRequestRepository.GetAllAsync(null, cancellationToken);
            var matchedRequests = requests
                .Where(r => r.Category.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                            r.PriorityLevel.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                            r.Location.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                            (r.Description != null && r.Description.Contains(q, StringComparison.OrdinalIgnoreCase)))
                .Take(3);

            foreach (var r in matchedRequests)
            {
                var desc = r.Description ?? "No additional description";
                results.Add(new GlobalSearchResultDto
                {
                    Category = "Assistance Request",
                    Id = r.Id,
                    Title = $"{r.Category} Support - {r.Location}",
                    Subtitle = desc.Length > 45 ? desc[..42] + "…" : desc,
                    Badge = r.PriorityLevel,
                    Url = "/emergency-coordinator/assistance-requests"
                });
            }
        }
        catch
        {
            // skip
        }

        // 3. Search Key Operational Locations
        var operationalLocations = new[]
        {
            ("Greater Riverton Operational Sector", "DEOC Headquarters · Primary Evacuation Hub", "Active Sector", "/emergency-coordinator/live-map"),
            ("Western Province Basin", "Kelani River Basin Monitoring Zone", "Flood Warning", "/emergency-coordinator/live-map"),
            ("Nuwara Eliya Slope Zone", "Central Highlands Geological Sector", "Landslide Watch", "/emergency-coordinator/live-map"),
            ("Southern Coastline Sector", "Galle & Matara Maritime Buffer", "Coast Guard SAR", "/emergency-coordinator/live-map"),
        };

        foreach (var loc in operationalLocations)
        {
            if (loc.Item1.ToLowerInvariant().Contains(q) || loc.Item2.ToLowerInvariant().Contains(q))
            {
                results.Add(new GlobalSearchResultDto
                {
                    Category = "Operational Location",
                    Id = loc.Item1,
                    Title = loc.Item1,
                    Subtitle = loc.Item2,
                    Badge = loc.Item3,
                    Url = loc.Item4
                });
            }
        }

        // 4. Search Coordinators & Response Teams
        var teams = new[]
        {
            ("Amina Mensah", "Lead Emergency Coordinator · Incident Command", "DEOC Lead", "/emergency-coordinator/dashboard"),
            ("Capt. Alex Rivera", "Disaster Rescue & Evacuation Leader", "SAR Commander", "/emergency-coordinator/partners"),
            ("Dr. Priya Sharma", "Medical Triage & Field Health Coordinator", "Health Lead", "/emergency-coordinator/partners"),
            ("Rapid Response Fleet Delta", "Heavy Amphibious Relief Transport", "Active Fleet", "/emergency-coordinator/resource-overview")
        };

        foreach (var t in teams)
        {
            if (t.Item1.ToLowerInvariant().Contains(q) || t.Item2.ToLowerInvariant().Contains(q))
            {
                results.Add(new GlobalSearchResultDto
                {
                    Category = "Coordinator / Team",
                    Id = t.Item1,
                    Title = t.Item1,
                    Subtitle = t.Item2,
                    Badge = t.Item3,
                    Url = t.Item4
                });
            }
        }

        return results.Take(8).ToList();
    }

    public async Task<EmergencyTopbarStateDto> ResetTopbarStateAsync(CancellationToken cancellationToken = default)
    {
        await _topbarRepository.ResetStateAsync(cancellationToken);
        return await GetTopbarStateAsync(cancellationToken);
    }

    private static EmergencyNotificationDto MapToNotificationDto(EmergencyNotification n)
    {
        return new EmergencyNotificationDto
        {
            Id = n.Id,
            Title = n.Title,
            Message = n.Message,
            Category = n.Category,
            Severity = n.Severity,
            Timestamp = n.Timestamp,
            TimeAgo = FormatTimeAgo(n.Timestamp),
            IsRead = n.IsRead,
            ActionUrl = n.ActionUrl,
            ActionLabel = n.ActionLabel
        };
    }

    private static string FormatTimeAgo(DateTimeOffset time)
    {
        var span = DateTimeOffset.UtcNow - time;
        if (span.TotalMinutes < 1) return "Just now";
        if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes}m ago";
        if (span.TotalHours < 24) return $"{(int)span.TotalHours}h ago";
        return $"{(int)span.TotalDays}d ago";
    }
}
