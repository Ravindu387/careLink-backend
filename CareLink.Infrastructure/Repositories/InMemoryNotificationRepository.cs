using System.Collections.Concurrent;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;

namespace CareLink.Infrastructure.Repositories;

public class InMemoryNotificationRepository : INotificationRepository
{
    private readonly ConcurrentDictionary<string, NotificationItem> _notifications = new();
    private NotificationPreferences _preferences = new();
    private readonly object _prefLock = new();

    public InMemoryNotificationRepository()
    {
        SeedDefaultData();
    }

    public Task<IReadOnlyList<NotificationItem>> GetAllAsync(
        NotificationFilterParams? filter = null,
        CancellationToken cancellationToken = default)
    {
        var query = FilterQuery(filter);

        // Sort: Unread first, then by CreatedAt descending
        var sorted = query
            .OrderByDescending(n => n.Unread)
            .ThenByDescending(n => n.CreatedAt);

        if (filter != null && filter.Page > 0 && filter.PageSize > 0)
        {
            var paged = sorted
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();
            return Task.FromResult<IReadOnlyList<NotificationItem>>(paged);
        }

        return Task.FromResult<IReadOnlyList<NotificationItem>>(sorted.ToList());
    }

    public Task<int> GetCountAsync(
        NotificationFilterParams? filter = null,
        CancellationToken cancellationToken = default)
    {
        var count = FilterQuery(filter).Count();
        return Task.FromResult(count);
    }

    public Task<NotificationItem?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            return Task.FromResult<NotificationItem?>(null);

        var trimmed = id.Trim();
        if (_notifications.TryGetValue(trimmed, out var found))
            return Task.FromResult<NotificationItem?>(found);

        var byRef = _notifications.Values.FirstOrDefault(n =>
            n.Reference.Equals(trimmed, StringComparison.OrdinalIgnoreCase) ||
            n.Id.Equals(trimmed, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(byRef);
    }

    public Task<NotificationItem> AddAsync(
        NotificationItem notification,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(notification.Id))
        {
            notification.Id = (_notifications.Count + 1).ToString();
        }

        _notifications[notification.Id] = notification;
        return Task.FromResult(notification);
    }

    public Task<bool> MarkReadAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id)) return Task.FromResult(false);

        var trimmed = id.Trim();
        if (_notifications.TryGetValue(trimmed, out var item))
        {
            item.Unread = false;
            item.ReadAt = DateTimeOffset.UtcNow;
            return Task.FromResult(true);
        }

        var match = _notifications.Values.FirstOrDefault(n =>
            n.Reference.Equals(trimmed, StringComparison.OrdinalIgnoreCase) ||
            n.Id.Equals(trimmed, StringComparison.OrdinalIgnoreCase));

        if (match != null)
        {
            match.Unread = false;
            match.ReadAt = DateTimeOffset.UtcNow;
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    public Task<int> MarkAllReadAsync(string? role = null, CancellationToken cancellationToken = default)
    {
        var items = _notifications.Values.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(role))
        {
            var r = role.Trim();
            items = items.Where(n =>
                string.Equals(n.TargetRole, r, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(n.TargetRole, "all", StringComparison.OrdinalIgnoreCase));
        }

        int count = 0;
        var now = DateTimeOffset.UtcNow;
        foreach (var item in items)
        {
            if (item.Unread)
            {
                item.Unread = false;
                item.ReadAt = now;
                count++;
            }
        }

        return Task.FromResult(count);
    }

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id)) return Task.FromResult(false);

        var trimmed = id.Trim();
        if (_notifications.TryRemove(trimmed, out _))
            return Task.FromResult(true);

        var match = _notifications.Values.FirstOrDefault(n =>
            n.Reference.Equals(trimmed, StringComparison.OrdinalIgnoreCase) ||
            n.Id.Equals(trimmed, StringComparison.OrdinalIgnoreCase));

        if (match != null)
        {
            return Task.FromResult(_notifications.TryRemove(match.Id, out _));
        }

        return Task.FromResult(false);
    }

    public Task<NotificationPreferences> GetPreferencesAsync(CancellationToken cancellationToken = default)
    {
        lock (_prefLock)
        {
            return Task.FromResult(new NotificationPreferences
            {
                Critical = _preferences.Critical,
                Ai = _preferences.Ai,
                Resources = _preferences.Resources,
                Shelter = _preferences.Shelter,
                Partner = _preferences.Partner,
                Reports = _preferences.Reports,
                Urgent = _preferences.Urgent,
                Referrals = _preferences.Referrals,
                Collaboration = _preferences.Collaboration,
                Reporting = _preferences.Reporting,
                Team = _preferences.Team,
                Email = _preferences.Email,
                Push = _preferences.Push,
                UpdatedAt = _preferences.UpdatedAt
            });
        }
    }

    public Task<NotificationPreferences> UpdatePreferencesAsync(
        NotificationPreferences preferences,
        CancellationToken cancellationToken = default)
    {
        lock (_prefLock)
        {
            _preferences = new NotificationPreferences
            {
                Critical = preferences.Critical,
                Ai = preferences.Ai,
                Resources = preferences.Resources,
                Shelter = preferences.Shelter,
                Partner = preferences.Partner,
                Reports = preferences.Reports,
                Urgent = preferences.Urgent,
                Referrals = preferences.Referrals,
                Collaboration = preferences.Collaboration,
                Reporting = preferences.Reporting,
                Team = preferences.Team,
                Email = preferences.Email,
                Push = preferences.Push,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            return Task.FromResult(_preferences);
        }
    }

    public Task<NotificationMetrics> GetMetricsAsync(string? role = null, CancellationToken cancellationToken = default)
    {
        var items = _notifications.Values.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(role))
        {
            var r = role.Trim();
            items = items.Where(n =>
                string.Equals(n.TargetRole, r, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(n.TargetRole, "all", StringComparison.OrdinalIgnoreCase));
        }

        var list = items.ToList();
        var metrics = new NotificationMetrics
        {
            All = list.Count,
            Unread = list.Count(n => n.Unread),
            Urgent = list.Count(n => n.Urgent),
            Critical = list.Count(n => n.Category.Equals("critical", StringComparison.OrdinalIgnoreCase)),
            Ai = list.Count(n => n.Category.Equals("ai", StringComparison.OrdinalIgnoreCase)),
            Resource = list.Count(n => n.Category.Equals("resource", StringComparison.OrdinalIgnoreCase)),
            Shelter = list.Count(n => n.Category.Equals("shelter", StringComparison.OrdinalIgnoreCase)),
            Partner = list.Count(n => n.Category.Equals("partner", StringComparison.OrdinalIgnoreCase)),
            Report = list.Count(n => n.Category.Equals("report", StringComparison.OrdinalIgnoreCase)),
            Collaboration = list.Count(n => n.Category.Equals("collaboration", StringComparison.OrdinalIgnoreCase)),
            Reporting = list.Count(n => n.Category.Equals("reporting", StringComparison.OrdinalIgnoreCase)),
            Team = list.Count(n => n.Category.Equals("team", StringComparison.OrdinalIgnoreCase)),
            Agreement = list.Count(n => n.Category.Equals("agreement", StringComparison.OrdinalIgnoreCase)),
            LastUpdated = DateTimeOffset.UtcNow
        };
        return Task.FromResult(metrics);
    }

    public Task ResetDefaultDataAsync(CancellationToken cancellationToken = default)
    {
        _notifications.Clear();
        lock (_prefLock)
        {
            _preferences = new NotificationPreferences();
        }
        SeedDefaultData();
        return Task.CompletedTask;
    }

    private IEnumerable<NotificationItem> FilterQuery(NotificationFilterParams? filter)
    {
        var query = _notifications.Values.AsEnumerable();

        if (filter != null)
        {
            if (!string.IsNullOrWhiteSpace(filter.Role))
            {
                var r = filter.Role.Trim();
                query = query.Where(n =>
                    string.Equals(n.TargetRole, r, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(n.TargetRole, "all", StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var s = filter.Search.Trim();
                query = query.Where(n =>
                    n.Title.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    n.Description.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    n.Reference.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    n.Details.Contains(s, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.Category) &&
                !filter.Category.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                var c = filter.Category.Trim();
                if (c.Equals("unread", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(n => n.Unread);
                }
                else if (c.Equals("urgent", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(n => n.Urgent || n.Category.Equals("urgent", StringComparison.OrdinalIgnoreCase) || n.Category.Equals("critical", StringComparison.OrdinalIgnoreCase));
                }
                else
                {
                    query = query.Where(n => n.Category.Equals(c, StringComparison.OrdinalIgnoreCase));
                }
            }

            if (filter.UnreadOnly.HasValue && filter.UnreadOnly.Value)
            {
                query = query.Where(n => n.Unread);
            }

            if (filter.UrgentOnly.HasValue && filter.UrgentOnly.Value)
            {
                query = query.Where(n => n.Urgent);
            }
        }

        return query;
    }

    private void SeedDefaultData()
    {
        var now = DateTimeOffset.UtcNow;

        var coordinatorSeeds = new List<NotificationItem>
        {
            new()
            {
                Id = "1",
                Title = "Critical request overdue",
                Description = "REQ-2821 exceeded critical response SLA.",
                Time = "3 min ago",
                Category = "critical",
                Unread = true,
                Urgent = true,
                Reference = "REQ-2821",
                Details = "This critical assistance request has exceeded the expected response time. Immediate coordinator review and escalation are required.",
                ActionUrl = "/emergency-coordinator/assistance-requests",
                ActionLabel = "Open Request",
                TargetRole = "coordinator",
                CreatedAt = now.AddMinutes(-3)
            },
            new()
            {
                Id = "2",
                Title = "AI recommendation awaiting review",
                Description = "REQ-2841 · Critical · 94% confidence.",
                Time = "12 min ago",
                Category = "ai",
                Unread = true,
                Urgent = true,
                Reference = "REQ-2841",
                Details = "Priority Assist AI recommends Critical priority based on verified flood risk and vulnerable household information. An authorised coordinator must review this recommendation.",
                ActionUrl = "/emergency-coordinator/ai-recommendations",
                ActionLabel = "Review AI Priority",
                TargetRole = "coordinator",
                CreatedAt = now.AddMinutes(-12)
            },
            new()
            {
                Id = "3",
                Title = "Critical medical stock alert",
                Description = "Trauma kits at 25% availability.",
                Time = "24 min ago",
                Category = "resource",
                Unread = true,
                Urgent = true,
                Reference = "RES-102",
                Details = "Available trauma kit inventory has reached a critical level. Resource managers should review stock and arrange emergency replenishment.",
                ActionUrl = "/emergency-coordinator/resource-overview",
                ActionLabel = "View Inventory",
                TargetRole = "coordinator",
                CreatedAt = now.AddMinutes(-24)
            },
            new()
            {
                Id = "4",
                Title = "Shelter reached full capacity",
                Description = "St. Mary Civic Centre cannot accept arrivals.",
                Time = "38 min ago",
                Category = "shelter",
                Unread = false,
                Urgent = false,
                Reference = "SH-003",
                Details = "St. Mary Civic Centre has reached its maximum accommodation capacity. New arrivals should be redirected to alternative available shelters.",
                ActionUrl = "/emergency-coordinator/shelter-overview",
                ActionLabel = "Redirect Shelters",
                TargetRole = "coordinator",
                CreatedAt = now.AddMinutes(-38)
            },
            new()
            {
                Id = "5",
                Title = "Partner collaboration accepted",
                Description = "Global Relief Network accepted food response role.",
                Time = "1 hr ago",
                Category = "partner",
                Unread = false,
                Urgent = false,
                Reference = "ORG-027",
                Details = "Global Relief Network has accepted the assigned food response responsibility. Coordination teams can now proceed with the shared operation.",
                ActionUrl = "/emergency-coordinator/partner-organisations",
                ActionLabel = "View Partner",
                TargetRole = "coordinator",
                CreatedAt = now.AddHours(-1)
            },
            new()
            {
                Id = "6",
                Title = "Situation report ready for approval",
                Description = "SITREP 025 draft is complete.",
                Time = "2 hr ago",
                Category = "report",
                Unread = false,
                Urgent = false,
                Reference = "SITREP-025",
                Details = "The latest situation report draft is ready for review. An authorised coordinator should verify the report before approval and distribution.",
                ActionUrl = "/emergency-coordinator/situation-reports",
                ActionLabel = "Review SITREP",
                TargetRole = "coordinator",
                CreatedAt = now.AddHours(-2)
            },
            new()
            {
                Id = "7",
                Title = "Resource dispatch completed",
                Description = "DEP-104 delivered to Eastbank.",
                Time = "3 hr ago",
                Category = "resource",
                Unread = true,
                Urgent = false,
                Reference = "DEP-104",
                Details = "Emergency resources have been successfully delivered to the Eastbank operational area.",
                ActionUrl = "/emergency-coordinator/resource-overview",
                ActionLabel = "Track Depot",
                TargetRole = "coordinator",
                CreatedAt = now.AddHours(-3)
            },
            new()
            {
                Id = "8",
                Title = "New assistance request received",
                Description = "REQ-2845 · North Ward · High priority.",
                Time = "4 hr ago",
                Category = "critical",
                Unread = false,
                Urgent = false,
                Reference = "REQ-2845",
                Details = "A new assistance request has been submitted from North Ward and requires verification by the emergency coordination team.",
                ActionUrl = "/emergency-coordinator/assistance-requests",
                ActionLabel = "View Request",
                TargetRole = "coordinator",
                CreatedAt = now.AddHours(-4)
            },
            new()
            {
                Id = "9",
                Title = "AI review completed",
                Description = "REQ-2836 recommendation approved.",
                Time = "5 hr ago",
                Category = "ai",
                Unread = false,
                Urgent = false,
                Reference = "REQ-2836",
                Details = "The AI priority recommendation has been reviewed and approved by an authorised coordinator.",
                ActionUrl = "/emergency-coordinator/ai-recommendations",
                ActionLabel = "Audit History",
                TargetRole = "coordinator",
                CreatedAt = now.AddHours(-5)
            },
            new()
            {
                Id = "10",
                Title = "Shelter capacity warning",
                Description = "Riverton High School at 92% capacity.",
                Time = "6 hr ago",
                Category = "shelter",
                Unread = true,
                Urgent = false,
                Reference = "SH-001",
                Details = "Riverton High School is approaching full capacity. Monitor incoming arrivals and prepare alternative shelter arrangements.",
                ActionUrl = "/emergency-coordinator/shelter-overview",
                ActionLabel = "Monitor Capacity",
                TargetRole = "coordinator",
                CreatedAt = now.AddHours(-6)
            },
            new()
            {
                Id = "11",
                Title = "Partner verification completed",
                Description = "National Disaster Authority verified.",
                Time = "7 hr ago",
                Category = "partner",
                Unread = false,
                Urgent = false,
                Reference = "ORG-001",
                Details = "The partner organisation verification process has been completed successfully.",
                ActionUrl = "/emergency-coordinator/partner-organisations",
                ActionLabel = "View Profile",
                TargetRole = "coordinator",
                CreatedAt = now.AddHours(-7)
            },
            new()
            {
                Id = "12",
                Title = "Situation report published",
                Description = "SITREP 024 distributed to partners.",
                Time = "8 hr ago",
                Category = "report",
                Unread = false,
                Urgent = false,
                Reference = "SITREP-024",
                Details = "The approved situation report has been published and distributed to registered partner organisations.",
                ActionUrl = "/emergency-coordinator/situation-reports",
                ActionLabel = "View Published SITREP",
                TargetRole = "coordinator",
                CreatedAt = now.AddHours(-8)
            }
        };

        var partnerSeeds = new List<NotificationItem>
        {
            new()
            {
                Id = "P-1",
                Title = "Critical referral awaiting response",
                Description = "REF-2841 · Emergency shelter · 6 people",
                Time = "4 min ago",
                Category = "Urgent",
                Unread = true,
                Urgent = true,
                Reference = "REF-2841",
                Details = "A critical emergency shelter referral has been received from the North Ward. Six affected people require urgent accommodation support. Please review and respond as soon as possible.",
                TargetRole = "partner",
                ActionUrl = "/partner-officer/partner/assistance-referrals",
                ActionLabel = "View Referral",
                CreatedAt = now.AddMinutes(-4)
            },
            new()
            {
                Id = "P-2",
                Title = "New collaboration request",
                Description = "Mobile water distribution · North Ward",
                Time = "12 min ago",
                Category = "Collaboration",
                Unread = true,
                Urgent = false,
                Reference = "COL-0742",
                Details = "National Disaster Authority has requested collaboration for a mobile water distribution activity in North Ward. Your organisation is requested to provide vehicles and field staff.",
                TargetRole = "partner",
                ActionUrl = "/partner-officer/partner/collaboration-requests",
                ActionLabel = "View Request",
                CreatedAt = now.AddMinutes(-12)
            },
            new()
            {
                Id = "P-3",
                Title = "Resource delivery exception",
                Description = "COM-1039 · Partial delivery recorded",
                Time = "28 min ago",
                Category = "Resource",
                Unread = true,
                Urgent = true,
                Reference = "COM-1039",
                Details = "A partial delivery has been recorded for commitment COM-1039. Some water purification tablets have not reached the destination. Please review the delivery status and coordinate with the logistics team.",
                TargetRole = "partner",
                ActionUrl = "/partner-officer/partner/resource-sharing",
                ActionLabel = "Review Delivery",
                CreatedAt = now.AddMinutes(-28)
            },
            new()
            {
                Id = "P-4",
                Title = "Partner report due today",
                Description = "Daily activity report due at 18:00",
                Time = "42 min ago",
                Category = "Reporting",
                Unread = true,
                Urgent = false,
                Reference = "RPT-1042",
                Details = "Your Daily Partner Activity Report for Monsoon Floods 2025 must be submitted today before 18:00. The report is currently 82% complete and three sections require review.",
                TargetRole = "partner",
                ActionUrl = "/partner-officer/partner/reports",
                ActionLabel = "Open Report",
                CreatedAt = now.AddMinutes(-42)
            },
            new()
            {
                Id = "P-5",
                Title = "Agreement review required",
                Description = "Safeguarding attestation due in 18 days",
                Time = "1 hr ago",
                Category = "Agreement",
                Unread = false,
                Urgent = false,
                Reference = "AGR-0038",
                Details = "Your organisation's safeguarding attestation requires review. Please verify the information and submit the updated agreement before the due date.",
                TargetRole = "partner",
                ActionUrl = "/partner-officer/partner/documents",
                ActionLabel = "Review Agreement",
                CreatedAt = now.AddHours(-1)
            },
            new()
            {
                Id = "P-6",
                Title = "Field team checked in",
                Description = "North Ward distribution team · 12 members",
                Time = "2 hr ago",
                Category = "Team",
                Unread = false,
                Urgent = false,
                Reference = "TEAM-0012",
                Details = "The North Ward distribution team has successfully checked in. Twelve team members are currently active in the field and ready to continue the scheduled distribution activity.",
                TargetRole = "partner",
                ActionUrl = "/partner-officer/partner/field-activities",
                ActionLabel = "Field Activities",
                CreatedAt = now.AddHours(-2)
            }
        };

        foreach (var item in coordinatorSeeds)
        {
            _notifications[item.Id] = item;
        }

        foreach (var item in partnerSeeds)
        {
            _notifications[item.Id] = item;
        }
    }
}
