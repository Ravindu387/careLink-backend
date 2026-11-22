using System.Collections.Concurrent;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using CareLink.Core.Models;

namespace CareLink.Infrastructure.Repositories;

public class InMemoryNotificationRepository : INotificationRepository
{
    private readonly ConcurrentDictionary<int, NotificationItem> _notifications = new();
    private readonly ConcurrentDictionary<string, NotificationPreference> _rolePreferences = new();
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
        int id,
        CancellationToken cancellationToken = default)
    {
        _notifications.TryGetValue(id, out var found);
        return Task.FromResult(found);
    }

    public Task<NotificationItem?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            return Task.FromResult<NotificationItem?>(null);

        var trimmed = id.Trim();
        if (int.TryParse(trimmed, out var intId) && _notifications.TryGetValue(intId, out var found))
        {
            return Task.FromResult<NotificationItem?>(found);
        }

        var match = _notifications.Values.FirstOrDefault(n =>
            n.Reference.Equals(trimmed, StringComparison.OrdinalIgnoreCase) ||
            n.Id.ToString() == trimmed);

        return Task.FromResult(match);
    }

    public Task<NotificationItem> AddAsync(
        NotificationItem notification,
        CancellationToken cancellationToken = default)
    {
        if (notification.Id <= 0)
        {
            int nextId = _notifications.Count > 0 ? _notifications.Keys.Max() + 1 : 1;
            notification.Id = nextId;
        }

        _notifications[notification.Id] = notification;
        return Task.FromResult(notification);
    }

    public Task<bool> UpdateAsync(
        NotificationItem item,
        CancellationToken cancellationToken = default)
    {
        if (!_notifications.ContainsKey(item.Id))
        {
            return Task.FromResult(false);
        }

        _notifications[item.Id] = item;
        return Task.FromResult(true);
    }

    public Task<bool> MarkAsReadAsync(int id, CancellationToken cancellationToken = default)
    {
        if (_notifications.TryGetValue(id, out var item))
        {
            item.Unread = false;
            item.ReadAt = DateTimeOffset.UtcNow;
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<bool> MarkReadAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id)) return Task.FromResult(false);

        var trimmed = id.Trim();
        if (int.TryParse(trimmed, out var intId))
        {
            return MarkAsReadAsync(intId, cancellationToken);
        }

        var match = _notifications.Values.FirstOrDefault(n =>
            n.Reference.Equals(trimmed, StringComparison.OrdinalIgnoreCase) ||
            n.Id.ToString() == trimmed);

        if (match != null)
        {
            match.Unread = false;
            match.ReadAt = DateTimeOffset.UtcNow;
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    public Task<int> MarkAllAsReadAsync(string? role = null, CancellationToken cancellationToken = default)
    {
        var items = _notifications.Values.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(role))
        {
            var r = role.Trim();
            items = items.Where(n =>
                string.Equals(n.Role, r, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(n.Role, "all", StringComparison.OrdinalIgnoreCase));
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

    public Task<int> MarkAllReadAsync(string? role = null, CancellationToken cancellationToken = default)
    {
        return MarkAllAsReadAsync(role, cancellationToken);
    }

    public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_notifications.TryRemove(id, out _));
    }

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id)) return Task.FromResult(false);

        var trimmed = id.Trim();
        if (int.TryParse(trimmed, out var intId))
        {
            return DeleteAsync(intId, cancellationToken);
        }

        var match = _notifications.Values.FirstOrDefault(n =>
            n.Reference.Equals(trimmed, StringComparison.OrdinalIgnoreCase) ||
            n.Id.ToString() == trimmed);

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
                string.Equals(n.Role, r, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(n.Role, "all", StringComparison.OrdinalIgnoreCase));
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
        _rolePreferences.Clear();
        SeedDefaultData();
        return Task.CompletedTask;
    }

    public Task<(IEnumerable<NotificationItem> Items, int TotalCount)> GetPagedAsync(
        NotificationQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = FilterByParameters(parameters);
        var totalCount = query.Count();
        var page = parameters.Page > 0 ? parameters.Page : 1;
        var pageSize = parameters.PageSize > 0 ? parameters.PageSize : 10;

        var items = query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult(((IEnumerable<NotificationItem>)items, totalCount));
    }

    public Task<IEnumerable<NotificationItem>> GetAllAsync(
        NotificationQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = FilterByParameters(parameters);
        var items = query.OrderByDescending(n => n.CreatedAt).ToList();
        return Task.FromResult<IEnumerable<NotificationItem>>(items);
    }

    public Task<int> GetUnreadCountAsync(string? role = null, CancellationToken cancellationToken = default)
    {
        var query = _notifications.Values.Where(n => n.Unread);
        if (!string.IsNullOrWhiteSpace(role) && !role.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(n => n.Role.Equals(role, StringComparison.OrdinalIgnoreCase) || n.Role.Equals("All", StringComparison.OrdinalIgnoreCase));
        }
        return Task.FromResult(query.Count());
    }

    public Task<Dictionary<string, int>> GetTypeCountsAsync(string? role = null, CancellationToken cancellationToken = default)
    {
        var query = _notifications.Values.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(role) && !role.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(n => n.Role.Equals(role, StringComparison.OrdinalIgnoreCase) || n.Role.Equals("All", StringComparison.OrdinalIgnoreCase));
        }

        var counts = query
            .GroupBy(n => n.Type)
            .ToDictionary(g => g.Key, g => g.Count());

        return Task.FromResult(counts);
    }

    public Task<NotificationPreference?> GetPreferencesAsync(string role, CancellationToken cancellationToken = default)
    {
        _rolePreferences.TryGetValue(role, out var pref);
        return Task.FromResult(pref);
    }

    public Task<NotificationPreference> SavePreferencesAsync(NotificationPreference preference, CancellationToken cancellationToken = default)
    {
        _rolePreferences[preference.Role] = preference;
        return Task.FromResult(preference);
    }

    private IEnumerable<NotificationItem> FilterByParameters(NotificationQueryParameters parameters)
    {
        var query = _notifications.Values.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(parameters.Role) && !parameters.Role.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(n => n.Role.Equals(parameters.Role, StringComparison.OrdinalIgnoreCase) || n.Role.Equals("All", StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(parameters.Type) && !parameters.Type.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(n => n.Type.Equals(parameters.Type, StringComparison.OrdinalIgnoreCase));
        }

        if (parameters.UnreadOnly.HasValue && parameters.UnreadOnly.Value)
        {
            query = query.Where(n => n.Unread);
        }

        if (parameters.UrgentOnly.HasValue && parameters.UrgentOnly.Value)
        {
            query = query.Where(n => n.Urgent);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search.Trim().ToLowerInvariant();
            query = query.Where(n =>
                n.Title.ToLowerInvariant().Contains(search) ||
                n.Description.ToLowerInvariant().Contains(search) ||
                n.Reference.ToLowerInvariant().Contains(search));
        }

        return query;
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
                    string.Equals(n.Role, r, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(n.Role, "all", StringComparison.OrdinalIgnoreCase));
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
                Id = 1,
                Title = "Critical request overdue",
                Description = "REQ-2821 exceeded critical response SLA.",
                Time = "3 min ago",
                Category = "critical",
                Unread = true,
                Urgent = true,
                Reference = "REQ-2821",
                Details = "This critical assistance request has exceeded the expected response time. Immediate coordinator review and escalation are required.",
                Role = "coordinator",
                ActionUrl = "/emergency-coordinator/assistance-requests",
                ActionLabel = "Open Request",
                CreatedAt = DateTime.UtcNow.AddMinutes(-3)
            },
            new()
            {
                Id = 2,
                Title = "Shelter reaching limit",
                Description = "North Community Hall reached 92% capacity.",
                Time = "8 min ago",
                Category = "shelter",
                Unread = true,
                Urgent = true,
                Reference = "SH-102",
                Details = "North Community Hall has exceeded the 90% threshold. Consider diverting incoming evacuees to Eastbank Civic Centre or West District Arena.",
                Role = "coordinator",
                ActionUrl = "/emergency-coordinator/shelters",
                ActionLabel = "View Shelter",
                CreatedAt = DateTime.UtcNow.AddMinutes(-8)
            },
            new()
            {
                Id = 3,
                Title = "Critical supply alert",
                Description = "Water purification tablets at Central Depot dropped below safety threshold.",
                Time = "14 min ago",
                Category = "resource",
                Unread = true,
                Urgent = true,
                Reference = "RES-004",
                Details = "Stock of water purification tablets has dropped below the minimum reserve level. Replenishment shipment requested from Provincial Hub.",
                Role = "coordinator",
                ActionUrl = "/emergency-coordinator/resources",
                ActionLabel = "Check Inventory",
                CreatedAt = DateTime.UtcNow.AddMinutes(-14)
            },
            new()
            {
                Id = 4,
                Title = "Partner report overdue",
                Description = "Red Cross Daily SitRep pending submission.",
                Time = "22 min ago",
                Category = "partner",
                Unread = true,
                Urgent = false,
                Reference = "PTR-RC-01",
                Details = "Daily situation report from partner Sri Lanka Red Cross Society is overdue by 2 hours. Notification sent to liaison officer.",
                Role = "coordinator",
                ActionUrl = "/emergency-coordinator/partners",
                ActionLabel = "View Partner",
                CreatedAt = DateTime.UtcNow.AddMinutes(-22)
            },
            new()
            {
                Id = 5,
                Title = "AI bottleneck detected",
                Description = "High request-to-resource ratio detected in Sector 4.",
                Time = "35 min ago",
                Category = "ai",
                Unread = true,
                Urgent = false,
                Reference = "AI-REC-01",
                Details = "Automated load-balancing analysis suggests reallocating 500 family food packs from North Hub to Central Depot to meet demand.",
                Role = "coordinator",
                ActionUrl = "/emergency-coordinator/recommendations",
                ActionLabel = "View Recommendation",
                CreatedAt = DateTime.UtcNow.AddMinutes(-35)
            },
            new()
            {
                Id = 6,
                Title = "System sync complete",
                Description = "Disaster management authority registry synchronized successfully.",
                Time = "1 hr ago",
                Category = "system",
                Unread = false,
                Urgent = false,
                Reference = "SYS-SYNC-401",
                Details = "Full synchronization with the National Disaster Relief Service database completed with 0 errors.",
                Role = "coordinator",
                ActionUrl = "/emergency-coordinator/settings",
                ActionLabel = "System Health",
                CreatedAt = DateTime.UtcNow.AddHours(-1)
            }
        };

        var partnerSeeds = new List<NotificationItem>
        {
            new()
            {
                Id = 101,
                Title = "Critical referral awaiting response",
                Description = "REF-2841 · Emergency shelter · 6 people",
                Time = "4 min ago",
                Category = "Urgent",
                Unread = true,
                Urgent = true,
                Reference = "REF-2841",
                Details = "A critical emergency shelter referral has been received from the North Ward. Six affected people require urgent accommodation support. Please review and respond as soon as possible.",
                Role = "partner",
                ActionUrl = "/partner-officer/partner/assistance-referrals",
                ActionLabel = "View Referral",
                CreatedAt = DateTime.UtcNow.AddMinutes(-4)
            },
            new()
            {
                Id = 102,
                Title = "New collaboration request",
                Description = "Mobile water distribution · North Ward",
                Time = "12 min ago",
                Category = "Collaboration",
                Unread = true,
                Urgent = false,
                Reference = "COL-0742",
                Details = "National Disaster Authority has requested collaboration for a mobile water distribution activity in North Ward. Your organisation is requested to provide vehicles and field staff.",
                Role = "partner",
                ActionUrl = "/partner-officer/partner/collaboration-requests",
                ActionLabel = "View Request",
                CreatedAt = DateTime.UtcNow.AddMinutes(-12)
            },
            new()
            {
                Id = 103,
                Title = "Resource delivery exception",
                Description = "COM-1039 · Partial delivery recorded",
                Time = "28 min ago",
                Category = "Resource",
                Unread = true,
                Urgent = true,
                Reference = "COM-1039",
                Details = "A partial delivery has been recorded for commitment COM-1039. Some water purification tablets have not reached the destination. Please review the delivery status and coordinate with the logistics team.",
                Role = "partner",
                ActionUrl = "/partner-officer/partner/resource-sharing",
                ActionLabel = "Review Delivery",
                CreatedAt = DateTime.UtcNow.AddMinutes(-28)
            },
            new()
            {
                Id = 104,
                Title = "Partner report due today",
                Description = "Daily activity report due at 18:00",
                Time = "42 min ago",
                Category = "Reporting",
                Unread = true,
                Urgent = false,
                Reference = "RPT-1042",
                Details = "Your Daily Partner Activity Report for Monsoon Floods 2025 must be submitted today before 18:00. The report is currently 82% complete and three sections require review.",
                Role = "partner",
                ActionUrl = "/partner-officer/partner/reports",
                ActionLabel = "Open Report",
                CreatedAt = DateTime.UtcNow.AddMinutes(-42)
            },
            new()
            {
                Id = 105,
                Title = "Agreement review required",
                Description = "Safeguarding attestation due in 18 days",
                Time = "1 hr ago",
                Category = "Agreement",
                Unread = false,
                Urgent = false,
                Reference = "AGR-0038",
                Details = "Your organisation's safeguarding attestation requires review. Please verify the information and submit the updated agreement before the due date.",
                Role = "partner",
                ActionUrl = "/partner-officer/partner/documents",
                ActionLabel = "Review Agreement",
                CreatedAt = DateTime.UtcNow.AddHours(-1)
            },
            new()
            {
                Id = 106,
                Title = "Field team checked in",
                Description = "North Ward distribution team · 12 members",
                Time = "2 hr ago",
                Category = "Team",
                Unread = false,
                Urgent = false,
                Reference = "TEAM-0012",
                Details = "The North Ward distribution team has successfully checked in. Twelve team members are currently active in the field and ready to continue the scheduled distribution activity.",
                Role = "partner",
                ActionUrl = "/partner-officer/partner/field-activities",
                ActionLabel = "Field Activities",
                CreatedAt = DateTime.UtcNow.AddHours(-2)
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
