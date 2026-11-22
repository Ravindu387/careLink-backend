using CareLink.Core.Entities;
using CareLink.Core.Interfaces;

namespace CareLink.Infrastructure.Repositories;

public class InMemoryEmergencySidebarRepository : IEmergencySidebarRepository
{
    private readonly object _lock = new();
    private EmergencySidebarState _state;

    public InMemoryEmergencySidebarRepository()
    {
        _state = CreateDefaultState();
    }

    public Task<EmergencySidebarState> GetStateAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            return Task.FromResult(CloneState(_state));
        }
    }

    public Task<CoordinatorProfile> UpdateCoordinatorDutyStatusAsync(
        string status,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _state.Coordinator.DutyStatus = status;
            return Task.FromResult(CloneCoordinator(_state.Coordinator));
        }
    }

    public Task<ActiveCrisisSummary> SetActiveCrisisAsync(
        ActiveCrisisSummary summary,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _state.ActiveCrisis = CloneCrisisSummary(summary);
            _state.Coordinator.OperationalArea = summary.OperationalArea;
            return Task.FromResult(CloneCrisisSummary(_state.ActiveCrisis));
        }
    }

    public Task<int> DecrementNotificationCountAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (_state.Badges.Notifications > 0)
            {
                _state.Badges.Notifications--;
            }
            return Task.FromResult(_state.Badges.Notifications);
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

    private static EmergencySidebarState CreateDefaultState()
    {
        return new EmergencySidebarState
        {
            Coordinator = new CoordinatorProfile
            {
                Id = "COORD-001",
                Name = "Amina Mensah",
                Role = "Emergency Coordinator",
                Initials = "AM",
                Email = "amina.mensah@reliefos.org",
                DutyStatus = "On Duty",
                OperationalArea = "Greater Riverton",
                BadgeNumber = "EC-8842",
                Phone = "+94 77 123 4567",
                ShiftStartedAt = DateTime.UtcNow.AddHours(-4).AddMinutes(-15)
            },
            ActiveCrisis = new ActiveCrisisSummary
            {
                Id = "CR-2025-001",
                Name = "Western Province Monsoon Flash Floods 2025",
                ShortTitle = "Monsoon Floods 2025",
                Type = "Flood",
                Severity = "Severe · Level 4",
                Level = 4,
                OperationalArea = "Greater Riverton",
                Status = "Active",
                AffectedPeople = 112000,
                FormattedAffectedPeople = "112K",
                StartedDate = "2025-10-12"
            },
            Badges = new SidebarBadgeCounts
            {
                CrisisEvents = 3,
                AssistanceRequests = 86,
                PriorityQueue = 12,
                AiRecommendations = 8,
                Notifications = 14,
                AuditTimeline = 0
            },
            System = new SystemReadiness
            {
                Status = "Operational · Level 4",
                IsActiveResponse = true,
                LastSyncedAt = DateTime.UtcNow,
                ActiveOperationsCount = 3,
                Heartbeat = "Healthy"
            }
        };
    }

    private static EmergencySidebarState CloneState(EmergencySidebarState src)
    {
        return new EmergencySidebarState
        {
            Coordinator = CloneCoordinator(src.Coordinator),
            ActiveCrisis = CloneCrisisSummary(src.ActiveCrisis),
            Badges = new SidebarBadgeCounts
            {
                CrisisEvents = src.Badges.CrisisEvents,
                AssistanceRequests = src.Badges.AssistanceRequests,
                PriorityQueue = src.Badges.PriorityQueue,
                AiRecommendations = src.Badges.AiRecommendations,
                Notifications = src.Badges.Notifications,
                AuditTimeline = src.Badges.AuditTimeline
            },
            System = new SystemReadiness
            {
                Status = src.System.Status,
                IsActiveResponse = src.System.IsActiveResponse,
                LastSyncedAt = src.System.LastSyncedAt,
                ActiveOperationsCount = src.System.ActiveOperationsCount,
                Heartbeat = src.System.Heartbeat
            }
        };
    }

    private static CoordinatorProfile CloneCoordinator(CoordinatorProfile c)
    {
        return new CoordinatorProfile
        {
            Id = c.Id,
            Name = c.Name,
            Role = c.Role,
            Initials = c.Initials,
            Email = c.Email,
            DutyStatus = c.DutyStatus,
            OperationalArea = c.OperationalArea,
            BadgeNumber = c.BadgeNumber,
            Phone = c.Phone,
            ShiftStartedAt = c.ShiftStartedAt
        };
    }

    private static ActiveCrisisSummary CloneCrisisSummary(ActiveCrisisSummary a)
    {
        return new ActiveCrisisSummary
        {
            Id = a.Id,
            Name = a.Name,
            ShortTitle = a.ShortTitle,
            Type = a.Type,
            Severity = a.Severity,
            Level = a.Level,
            OperationalArea = a.OperationalArea,
            Status = a.Status,
            AffectedPeople = a.AffectedPeople,
            FormattedAffectedPeople = a.FormattedAffectedPeople,
            StartedDate = a.StartedDate
        };
    }
}
