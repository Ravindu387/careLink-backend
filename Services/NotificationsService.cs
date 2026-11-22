using CareLink.Backend.Models;

namespace CareLink.Backend.Services;

public class NotificationsService : INotificationsService
{
    private readonly List<NotificationItem> _notifications = [];
    private readonly object _lock = new();
    private NotificationPreferencesDto _preferences = new();

    public NotificationsService()
    {
        InitializeSeedData();
    }

    private void InitializeSeedData()
    {
        var items = new List<NotificationItem>
        {
            new()
            {
                Id = 1,
                Title = "Critical case overdue",
                Description = "REQ-2821 exceeded critical review SLA.",
                Time = "3 min ago",
                Type = "Critical",
                Read = false,
                CaseId = "REQ-2821",
                CreatedAt = DateTime.UtcNow.AddMinutes(-3)
            },
            new()
            {
                Id = 2,
                Title = "New case assigned to you",
                Description = "REQ-2841 · Emergency shelter.",
                Time = "12 min ago",
                Type = "Assignment",
                Read = false,
                CaseId = "REQ-2841",
                CreatedAt = DateTime.UtcNow.AddMinutes(-12)
            },
            new()
            {
                Id = 3,
                Title = "Potential duplicate detected",
                Description = "REQ-2836 matches REQ-2811 at 88%.",
                Time = "28 min ago",
                Type = "Duplicate",
                Read = false,
                CaseId = "REQ-2836",
                CreatedAt = DateTime.UtcNow.AddMinutes(-28)
            },
            new()
            {
                Id = 4,
                Title = "Citizen supplied requested information",
                Description = "REQ-2814 · Prescription uploaded.",
                Time = "42 min ago",
                Type = "Evidence",
                Read = false,
                CaseId = "REQ-2814",
                CreatedAt = DateTime.UtcNow.AddMinutes(-42)
            },
            new()
            {
                Id = 5,
                Title = "Escalation acknowledged",
                Description = "Emergency Coordinator accepted REQ-2821.",
                Time = "1 hr ago",
                Type = "Escalation",
                Read = true,
                CaseId = "REQ-2821",
                CreatedAt = DateTime.UtcNow.AddHours(-1)
            },
            new()
            {
                Id = 6,
                Title = "AI recommendation ready",
                Description = "REQ-2841 · 94% confidence.",
                Time = "2 hr ago",
                Type = "AI",
                Read = true,
                CaseId = "REQ-2841",
                CreatedAt = DateTime.UtcNow.AddHours(-2)
            },
            new()
            {
                Id = 7,
                Title = "New case assigned to your team",
                Description = "REQ-2839 · Medical supplies.",
                Time = "3 hr ago",
                Type = "Assignment",
                Read = true,
                CaseId = "REQ-2839",
                CreatedAt = DateTime.UtcNow.AddHours(-3)
            },
            new()
            {
                Id = 8,
                Title = "Evidence verification completed",
                Description = "REQ-2828 · Documents successfully verified.",
                Time = "4 hr ago",
                Type = "Evidence",
                Read = true,
                CaseId = "REQ-2828",
                CreatedAt = DateTime.UtcNow.AddHours(-4)
            },
            new()
            {
                Id = 9,
                Title = "Critical priority updated",
                Description = "REQ-2789 · Priority changed to Critical.",
                Time = "5 hr ago",
                Type = "Critical",
                Read = true,
                CaseId = "REQ-2789",
                CreatedAt = DateTime.UtcNow.AddHours(-5)
            },
            new()
            {
                Id = 10,
                Title = "AI recommendation ready",
                Description = "REQ-2761 · Resource matching completed.",
                Time = "6 hr ago",
                Type = "AI",
                Read = true,
                CaseId = "REQ-2761",
                CreatedAt = DateTime.UtcNow.AddHours(-6)
            },
            new()
            {
                Id = 11,
                Title = "Potential duplicate detected",
                Description = "REQ-2701 matches REQ-2689 at 81%.",
                Time = "Yesterday",
                Type = "Duplicate",
                Read = true,
                CaseId = "REQ-2701",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new()
            {
                Id = 12,
                Title = "Case escalation received",
                Description = "REQ-2650 requires senior review.",
                Time = "Yesterday",
                Type = "Escalation",
                Read = true,
                CaseId = "REQ-2650",
                CreatedAt = DateTime.UtcNow.AddDays(-1).AddHours(-2)
            }
        };

        _notifications.AddRange(items);
    }

    public Task<NotificationSummaryDto> GetSummaryAsync()
    {
        lock (_lock)
        {
            var summary = new NotificationSummaryDto
            {
                TotalCount = _notifications.Count,
                UnreadCount = _notifications.Count(n => !n.Read),
                CriticalCount = _notifications.Count(n => n.Type.Equals("Critical", StringComparison.OrdinalIgnoreCase)),
                AssignmentCount = _notifications.Count(n => n.Type.Equals("Assignment", StringComparison.OrdinalIgnoreCase)),
                EvidenceCount = _notifications.Count(n => n.Type.Equals("Evidence", StringComparison.OrdinalIgnoreCase)),
                DuplicateCount = _notifications.Count(n => n.Type.Equals("Duplicate", StringComparison.OrdinalIgnoreCase)),
                EscalationCount = _notifications.Count(n => n.Type.Equals("Escalation", StringComparison.OrdinalIgnoreCase)),
                AiCount = _notifications.Count(n => n.Type.Equals("AI", StringComparison.OrdinalIgnoreCase)),
                DeliveryServices =
                [
                    new() { Name = "In-app notifications", Status = "Operational", Message = "In-app operational" },
                    new() { Name = "Email notifications", Status = "Operational", Message = "Email operational" },
                    new() { Name = "SMS notifications", Status = "Degraded", Message = "SMS provider degraded" }
                ]
            };

            return Task.FromResult(summary);
        }
    }

    public Task<PaginatedResult<NotificationItem>> GetNotificationsAsync(NotificationQueryDto query)
    {
        lock (_lock)
        {
            var queryable = _notifications.AsEnumerable();

            // Tab filtering
            if (!string.IsNullOrWhiteSpace(query.Tab) && !query.Tab.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                if (query.Tab.Equals("Unread", StringComparison.OrdinalIgnoreCase))
                {
                    queryable = queryable.Where(n => !n.Read);
                }
                else if (query.Tab.Equals("Critical", StringComparison.OrdinalIgnoreCase))
                {
                    queryable = queryable.Where(n => n.Type.Equals("Critical", StringComparison.OrdinalIgnoreCase));
                }
                else if (query.Tab.Equals("Assignments", StringComparison.OrdinalIgnoreCase))
                {
                    queryable = queryable.Where(n => n.Type.Equals("Assignment", StringComparison.OrdinalIgnoreCase));
                }
                else if (query.Tab.Equals("Evidence", StringComparison.OrdinalIgnoreCase))
                {
                    queryable = queryable.Where(n => n.Type.Equals("Evidence", StringComparison.OrdinalIgnoreCase));
                }
            }

            // Unread Only filter
            if (query.UnreadOnly.HasValue && query.UnreadOnly.Value)
            {
                queryable = queryable.Where(n => !n.Read);
            }

            // Type filter
            if (!string.IsNullOrWhiteSpace(query.Type) && !query.Type.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                queryable = queryable.Where(n => n.Type.Equals(query.Type, StringComparison.OrdinalIgnoreCase));
            }

            // Search filter
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var s = query.Search.Trim().ToLower();
                queryable = queryable.Where(n =>
                    n.Title.ToLower().Contains(s) ||
                    n.Description.ToLower().Contains(s) ||
                    n.CaseId.ToLower().Contains(s));
            }

            var list = queryable.OrderByDescending(n => n.CreatedAt).ToList();
            var totalCount = list.Count;
            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize < 1 ? 6 : query.PageSize;
            var totalPages = Math.Max(1, (int)Math.Ceiling((double)totalCount / pageSize));

            var items = list.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return Task.FromResult(new PaginatedResult<NotificationItem>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            });
        }
    }

    public Task<NotificationItem?> GetByIdAsync(int id)
    {
        lock (_lock)
        {
            var item = _notifications.FirstOrDefault(n => n.Id == id);
            return Task.FromResult(item);
        }
    }

    public Task<bool> MarkAsReadAsync(int id)
    {
        lock (_lock)
        {
            var item = _notifications.FirstOrDefault(n => n.Id == id);
            if (item != null)
            {
                item.Read = true;
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
    }

    public Task<int> MarkAllAsReadAsync()
    {
        lock (_lock)
        {
            var unread = _notifications.Where(n => !n.Read).ToList();
            foreach (var item in unread)
            {
                item.Read = true;
            }
            return Task.FromResult(unread.Count);
        }
    }

    public Task<bool> DeleteNotificationAsync(int id)
    {
        lock (_lock)
        {
            var item = _notifications.FirstOrDefault(n => n.Id == id);
            if (item != null)
            {
                _notifications.Remove(item);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
    }

    public Task<NotificationItem> CreateNotificationAsync(CreateNotificationRequest request)
    {
        lock (_lock)
        {
            var nextId = _notifications.Count > 0 ? _notifications.Max(n => n.Id) + 1 : 1;
            var newItem = new NotificationItem
            {
                Id = nextId,
                Title = request.Title,
                Description = request.Description,
                Type = string.IsNullOrWhiteSpace(request.Type) ? "Assignment" : request.Type,
                Read = false,
                CaseId = request.CaseId,
                Time = "Just now",
                CreatedAt = DateTime.UtcNow
            };

            _notifications.Insert(0, newItem);
            return Task.FromResult(newItem);
        }
    }

    public Task<NotificationPreferencesDto> GetPreferencesAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(_preferences);
        }
    }

    public Task<NotificationPreferencesDto> UpdatePreferencesAsync(NotificationPreferencesDto preferences)
    {
        lock (_lock)
        {
            _preferences = preferences;
            return Task.FromResult(_preferences);
        }
    }
}

