using CareLink.Core.Entities;
using CareLink.Core.Interfaces;

namespace CareLink.Infrastructure.Repositories;

public class InMemoryEmergencyTopbarRepository : IEmergencyTopbarRepository
{
    private readonly object _lock = new();
    private EmergencyTopbarState _state;

    public InMemoryEmergencyTopbarRepository()
    {
        _state = CreateDefaultState();
    }

    public Task<EmergencyTopbarState> GetStateAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            return Task.FromResult(CloneState(_state));
        }
    }

    public Task<IReadOnlyList<EmergencyNotification>> GetNotificationsAsync(
        bool unreadOnly = false,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var list = unreadOnly
                ? _state.Notifications.Where(n => !n.IsRead).ToList()
                : _state.Notifications.ToList();

            return Task.FromResult<IReadOnlyList<EmergencyNotification>>(
                list.Select(CloneNotification).ToList()
            );
        }
    }

    public Task<bool> MarkNotificationReadAsync(string notificationId, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var notif = _state.Notifications.FirstOrDefault(n => n.Id.Equals(notificationId, StringComparison.OrdinalIgnoreCase));
            if (notif == null)
            {
                return Task.FromResult(false);
            }

            if (!notif.IsRead)
            {
                notif.IsRead = true;
                _state.UnreadNotificationCount = Math.Max(0, _state.Notifications.Count(n => !n.IsRead));
            }

            return Task.FromResult(true);
        }
    }

    public Task<int> MarkAllNotificationsReadAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            foreach (var notif in _state.Notifications)
            {
                notif.IsRead = true;
            }
            _state.UnreadNotificationCount = 0;
            return Task.FromResult(0);
        }
    }

    public Task<EmergencyNotification> AddNotificationAsync(
        EmergencyNotification notification,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _state.Notifications.Insert(0, notification);
            _state.UnreadNotificationCount = _state.Notifications.Count(n => !n.IsRead);
            return Task.FromResult(CloneNotification(notification));
        }
    }

    public Task<string> UpdateOperationalAreaAsync(string area, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _state.OperationalArea = area;
            return Task.FromResult(_state.OperationalArea);
        }
    }

    public Task<string> UpdateLanguageAsync(string language, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _state.SelectedLanguage = language.ToUpperInvariant();
            return Task.FromResult(_state.SelectedLanguage);
        }
    }

    public Task ResetStateAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _state = CreateDefaultState();
            return Task.CompletedTask;
        }
    }

    private static EmergencyTopbarState CreateDefaultState()
    {
        var now = DateTimeOffset.UtcNow;
        return new EmergencyTopbarState
        {
            OperationalArea = "Greater Riverton",
            SelectedLanguage = "EN",
            SupportedLanguages = new List<string> { "EN", "SI", "TA" },
            UnreadNotificationCount = 14,
            CoordinatorName = "Amina Mensah",
            CoordinatorRole = "Emergency Coordinator",
            CoordinatorInitials = "AM",
            DutyStatus = "On Duty",
            Helplines = new List<EmergencyHelpline>
            {
                new()
                {
                    Name = "Disaster Management Centre",
                    Number = "117",
                    Department = "National Emergency Command",
                    Description = "24/7 central emergency operations hotline for search, rescue, and hazard alerts.",
                    Is24x7 = true
                },
                new()
                {
                    Name = "Suwa Seriya Ambulance",
                    Number = "1990",
                    Department = "Medical Emergency Services",
                    Description = "Toll-free nationwide rapid medical transport and field paramedic dispatch.",
                    Is24x7 = true
                },
                new()
                {
                    Name = "Police Emergency Service",
                    Number = "119",
                    Department = "Sri Lanka Police Operations",
                    Description = "Civil protection, urgent law enforcement, and emergency escort assistance.",
                    Is24x7 = true
                },
                new()
                {
                    Name = "Tri-Forces Search & Rescue",
                    Number = "118",
                    Department = "Ministry of Defence",
                    Description = "Air Force helicopter rescue and military heavy engineering evacuation unit.",
                    Is24x7 = true
                },
                new()
                {
                    Name = "Coast Guard Maritime SAR",
                    Number = "1989",
                    Department = "Maritime Safety Command",
                    Description = "Coastal flood surge and watercraft distress response unit.",
                    Is24x7 = true
                }
            },
            Notifications = new List<EmergencyNotification>
            {
                new()
                {
                    Id = "NOTIF-001",
                    Title = "Kelani River Crest Warning - Sector 4",
                    Message = "River gauge reached 8.65m (Critical Flood Alert). Immediate low-lying evacuation ordered for 1,200 households.",
                    Category = "Incident",
                    Severity = "Critical",
                    Timestamp = now.AddMinutes(-4),
                    IsRead = false,
                    ActionUrl = "/emergency-coordinator/crisis-events",
                    ActionLabel = "View Incident"
                },
                new()
                {
                    Id = "NOTIF-002",
                    Title = "High-Priority Medical Air-Evacuation Request",
                    Message = "Urgent ventilator & oxygen assistance needed at Kolonnawa Field Clinic for 4 critical patients.",
                    Category = "Assistance",
                    Severity = "Critical",
                    Timestamp = now.AddMinutes(-12),
                    IsRead = false,
                    ActionUrl = "/emergency-coordinator/assistance-requests",
                    ActionLabel = "Review Request"
                },
                new()
                {
                    Id = "NOTIF-003",
                    Title = "AI Resource Rebalancing Recommendation Ready",
                    Message = "Recommendation #REC-8849 generated: Reposition 3 search boats from Colombo South to Wellampitiya.",
                    Category = "AI Alert",
                    Severity = "Warning",
                    Timestamp = now.AddMinutes(-28),
                    IsRead = false,
                    ActionUrl = "/emergency-coordinator/ai-recommendations",
                    ActionLabel = "Review AI Action"
                },
                new()
                {
                    Id = "NOTIF-004",
                    Title = "Severe Thunderstorm & Flash Flood Watch",
                    Message = "Meteorological Dept issued warning for heavy torrential rainfall (>150mm) across Western Province over the next 6 hours.",
                    Category = "Weather",
                    Severity = "Warning",
                    Timestamp = now.AddMinutes(-45),
                    IsRead = false,
                    ActionUrl = "/emergency-coordinator/live-map",
                    ActionLabel = "Inspect Radar"
                },
                new()
                {
                    Id = "NOTIF-005",
                    Title = "DEOC Auxiliary Power & Satellite Uplink Active",
                    Message = "Western Province Command Center switched to primary diesel generators. Data connectivity 100% operational.",
                    Category = "System",
                    Severity = "Info",
                    Timestamp = now.AddHours(-2),
                    IsRead = false,
                    ActionUrl = "/emergency-coordinator/audit-timeline",
                    ActionLabel = "View Audit Log"
                },
                new()
                {
                    Id = "NOTIF-006",
                    Title = "Overdue Request: 65 Infant Nutritional Kits",
                    Message = "Awaiting dispatch to Sedawatte community shelter past 2-hour SLA.",
                    Category = "Assistance",
                    Severity = "Warning",
                    Timestamp = now.AddHours(-3),
                    IsRead = false,
                    ActionUrl = "/emergency-coordinator/priority-queue",
                    ActionLabel = "Prioritize"
                },
                new()
                {
                    Id = "NOTIF-007",
                    Title = "Nuwara Eliya Landslide Stabilization",
                    Message = "Road clearance team cleared 40% of debris along Hatton-Ginigathhena main route. Secondary access open.",
                    Category = "Incident",
                    Severity = "Info",
                    Timestamp = now.AddHours(-4),
                    IsRead = true,
                    ActionUrl = "/emergency-coordinator/crisis-events",
                    ActionLabel = "View Status"
                }
            }
        };
    }

    private static EmergencyTopbarState CloneState(EmergencyTopbarState s)
    {
        return new EmergencyTopbarState
        {
            OperationalArea = s.OperationalArea,
            SelectedLanguage = s.SelectedLanguage,
            SupportedLanguages = new List<string>(s.SupportedLanguages),
            UnreadNotificationCount = s.Notifications.Count(n => !n.IsRead),
            CoordinatorName = s.CoordinatorName,
            CoordinatorRole = s.CoordinatorRole,
            CoordinatorInitials = s.CoordinatorInitials,
            DutyStatus = s.DutyStatus,
            Helplines = s.Helplines.Select(CloneHelpline).ToList(),
            Notifications = s.Notifications.Select(CloneNotification).ToList()
        };
    }

    private static EmergencyHelpline CloneHelpline(EmergencyHelpline h)
    {
        return new EmergencyHelpline
        {
            Name = h.Name,
            Number = h.Number,
            Department = h.Department,
            Description = h.Description,
            Is24x7 = h.Is24x7
        };
    }

    private static EmergencyNotification CloneNotification(EmergencyNotification n)
    {
        return new EmergencyNotification
        {
            Id = n.Id,
            Title = n.Title,
            Message = n.Message,
            Category = n.Category,
            Severity = n.Severity,
            Timestamp = n.Timestamp,
            IsRead = n.IsRead,
            ActionUrl = n.ActionUrl,
            ActionLabel = n.ActionLabel
        };
    }
}
