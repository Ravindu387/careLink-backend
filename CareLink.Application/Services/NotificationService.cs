using System.Collections.Concurrent;
using CareLink.Application.Common;
using CareLink.Application.DTOs.AuditLogs;
using CareLink.Application.DTOs.Notifications;
using CareLink.Application.DTOs.Users;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;

namespace CareLink.Application.Services;

public class NotificationService : INotificationService
{
    private readonly ConcurrentDictionary<int, SystemNotification> _notifications = new();
    private readonly ConcurrentDictionary<string, NotificationPreferencesDto> _preferences = new(StringComparer.OrdinalIgnoreCase);
    private readonly IAuditLogService _auditLogService;
    private int _idSequence = 20;

    public NotificationService(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
        InitializeDefaultNotifications();
    }

    private void InitializeDefaultNotifications()
    {
        var now = DateTime.UtcNow;

        var seeds = new List<SystemNotification>
        {
            new()
            {
                Id = 1,
                Type = "Emergency",
                Title = "Flood warning escalated – Greater Riverton",
                Message = "Water levels have risen above the warning threshold. Review the affected locations and coordinate the emergency response team.",
                Priority = "Critical",
                Location = "Greater Riverton · North Ward",
                Action = "View incident",
                ActionUrl = "/admin/dashboard",
                Unread = true,
                CreatedAt = now.AddMinutes(-2)
            },
            new()
            {
                Id = 2,
                Type = "Assistance",
                Title = "New urgent assistance request",
                Message = "A family of five has requested temporary shelter and drinking water. The request is awaiting assignment.",
                Priority = "High",
                Location = "Central Clinic area",
                Action = "Review request",
                ActionUrl = "/admin/requests",
                Unread = true,
                CreatedAt = now.AddMinutes(-14)
            },
            new()
            {
                Id = 3,
                Type = "Volunteer",
                Title = "Volunteer team check-in received",
                Message = "Team Delta has checked in successfully. All 8 volunteers have been marked as available for the afternoon shift.",
                Priority = "Normal",
                Location = "Riverton Community Centre",
                Action = "View team",
                ActionUrl = "/admin/users",
                Unread = true,
                CreatedAt = now.AddMinutes(-32)
            },
            new()
            {
                Id = 4,
                Type = "Resource",
                Title = "Low stock alert: drinking water",
                Message = "The Central Warehouse has less than 20% of its target drinking-water stock remaining. Replenishment is recommended.",
                Priority = "High",
                Location = "Central Warehouse",
                Action = "View inventory",
                ActionUrl = "/admin/dashboard",
                Unread = true,
                CreatedAt = now.AddHours(-1)
            },
            new()
            {
                Id = 5,
                Type = "Emergency",
                Title = "Shelter capacity nearing limit",
                Message = "Riverton Primary School Shelter has reached 92% occupancy. Consider redirecting new arrivals to nearby shelters.",
                Priority = "High",
                Location = "Riverton Primary School",
                Action = "View shelter",
                ActionUrl = "/admin/dashboard",
                Unread = false,
                CreatedAt = now.AddHours(-2)
            },
            new()
            {
                Id = 6,
                Type = "System",
                Title = "Daily operational summary is ready",
                Message = "Your daily response summary includes 48 resolved requests, 12 active incidents and 6 pending verifications.",
                Priority = "Normal",
                Action = "View summary",
                ActionUrl = "/admin/reports",
                Unread = false,
                CreatedAt = now.Date.AddHours(8)
            },
            new()
            {
                Id = 7,
                Type = "Assistance",
                Title = "Assistance request successfully assigned",
                Message = "Request REQ-2048 has been assigned to Case Manager N. Perera.",
                Priority = "Normal",
                Location = "East Riverton",
                Action = "View request",
                ActionUrl = "/admin/requests",
                Unread = false,
                CreatedAt = now.AddDays(-1).AddHours(3)
            },
            new()
            {
                Id = 8,
                Type = "Volunteer",
                Title = "Volunteer verification completed",
                Message = "Three volunteer profiles have passed identity verification and are ready for deployment.",
                Priority = "Normal",
                Action = "View verification",
                ActionUrl = "/admin/verification",
                Unread = false,
                CreatedAt = now.AddDays(-1).AddHours(1)
            },
            new()
            {
                Id = 9,
                Type = "System",
                Title = "Security event: 5 repeated failed sign-ins detected",
                Message = "Suspicious login attempts detected from IP 185.42.18.9 targeting USR-10312. Automated rate limiting initiated.",
                Priority = "High",
                Location = "Security Gateway",
                Action = "View security events",
                ActionUrl = "/admin/security-events",
                Unread = true,
                CreatedAt = now.AddMinutes(-50)
            },
            new()
            {
                Id = 10,
                Type = "Resource",
                Title = "Emergency medical supply shipment arriving",
                Message = "Shipment MED-882 from National Red Cross has arrived at Central Depot and cleared intake inspection.",
                Priority = "Normal",
                Location = "Central Depot Kalutara",
                Action = "View inventory",
                ActionUrl = "/admin/dashboard",
                Unread = false,
                CreatedAt = now.AddDays(-2)
            }
        };

        foreach (var seed in seeds)
        {
            _notifications.TryAdd(seed.Id, seed);
        }
    }

    public Task<ApiResponse<PaginatedListDto<NotificationDto>>> GetNotificationsAsync(NotificationQueryDto query)
    {
        var items = _notifications.Values
            .Where(n => !n.IsArchived)
            .AsEnumerable();

        // 1. Text Search
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim().ToLowerInvariant();
            items = items.Where(n =>
                n.Title.ToLowerInvariant().Contains(s) ||
                n.Message.ToLowerInvariant().Contains(s) ||
                (n.Location != null && n.Location.ToLowerInvariant().Contains(s)));
        }

        // 2. Type Filter
        if (!string.IsNullOrWhiteSpace(query.Type) && !query.Type.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            if (query.Type.Equals("Unread", StringComparison.OrdinalIgnoreCase))
            {
                items = items.Where(n => n.Unread);
            }
            else
            {
                items = items.Where(n => n.Type.Equals(query.Type, StringComparison.OrdinalIgnoreCase));
            }
        }

        // 3. UnreadOnly flag
        if (query.UnreadOnly.HasValue && query.UnreadOnly.Value)
        {
            items = items.Where(n => n.Unread);
        }

        // 4. Priority Filter
        if (!string.IsNullOrWhiteSpace(query.Priority) && !query.Priority.Equals("All priorities", StringComparison.OrdinalIgnoreCase))
        {
            items = items.Where(n => n.Priority.Equals(query.Priority, StringComparison.OrdinalIgnoreCase));
        }

        var ordered = items.OrderByDescending(n => n.CreatedAt).ToList();
        var totalCount = ordered.Count;
        var pageSize = Math.Max(1, Math.Min(100, query.PageSize));
        var pageNumber = Math.Max(1, query.PageNumber);
        var pagedList = ordered
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(MapToDto)
            .ToList();

        var result = new PaginatedListDto<NotificationDto>
        {
            Items = pagedList,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        return Task.FromResult(ApiResponse<PaginatedListDto<NotificationDto>>.SuccessResult(result));
    }

    public Task<ApiResponse<NotificationStatsDto>> GetStatsAsync()
    {
        var active = _notifications.Values.Where(n => !n.IsArchived).ToList();

        var stats = new NotificationStatsDto
        {
            Total = active.Count,
            UnreadCount = active.Count(n => n.Unread),
            CriticalCount = active.Count(n => n.Priority.Equals("Critical", StringComparison.OrdinalIgnoreCase)),
            HighPriorityCount = active.Count(n => n.Priority.Equals("High", StringComparison.OrdinalIgnoreCase)),
            EmergencyCount = active.Count(n => n.Type.Equals("Emergency", StringComparison.OrdinalIgnoreCase)),
            AssistanceCount = active.Count(n => n.Type.Equals("Assistance", StringComparison.OrdinalIgnoreCase)),
            VolunteerCount = active.Count(n => n.Type.Equals("Volunteer", StringComparison.OrdinalIgnoreCase)),
            ResourceCount = active.Count(n => n.Type.Equals("Resource", StringComparison.OrdinalIgnoreCase)),
            SystemCount = active.Count(n => n.Type.Equals("System", StringComparison.OrdinalIgnoreCase))
        };

        return Task.FromResult(ApiResponse<NotificationStatsDto>.SuccessResult(stats));
    }

    public Task<ApiResponse<NotificationDto>> GetNotificationByIdAsync(int id)
    {
        if (_notifications.TryGetValue(id, out var entity) && !entity.IsArchived)
        {
            return Task.FromResult(ApiResponse<NotificationDto>.SuccessResult(MapToDto(entity)));
        }

        return Task.FromResult(ApiResponse<NotificationDto>.FailureResult("Notification not found."));
    }

    public async Task<ApiResponse<NotificationDto>> CreateNotificationAsync(CreateNotificationRequestDto request, string actor)
    {
        var id = Interlocked.Increment(ref _idSequence);
        var entity = new SystemNotification
        {
            Id = id,
            Type = request.Type,
            Title = request.Title,
            Message = request.Message,
            Priority = request.Priority,
            Location = request.Location,
            Action = request.Action,
            ActionUrl = request.ActionUrl,
            TargetUserRole = request.TargetUserRole,
            Unread = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = actor
        };

        _notifications.TryAdd(id, entity);

        // Record audit trail if critical alert
        if (request.Priority.Equals("Critical", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                await _auditLogService.CreateAuditLogAsync(new CreateAuditLogRequestDto
                {
                    Action = "Emergency Notification Broadcasted",
                    Actor = actor,
                    Record = $"NOTIF-{id}",
                    Change = $"Broadcasted Critical Alert: {request.Title}",
                    Rationale = "Emergency operational alert dissemination",
                    Type = "Notification",
                    Environment = "Production"
                });
            }
            catch
            {
                // Best effort
            }
        }

        return ApiResponse<NotificationDto>.SuccessResult(MapToDto(entity), "Notification created successfully.");
    }

    public Task<ApiResponse<bool>> MarkAsReadAsync(int id)
    {
        if (_notifications.TryGetValue(id, out var entity))
        {
            entity.Unread = false;
            entity.ReadAt = DateTime.UtcNow;
            return Task.FromResult(ApiResponse<bool>.SuccessResult(true, "Notification marked as read."));
        }

        return Task.FromResult(ApiResponse<bool>.FailureResult("Notification not found."));
    }

    public Task<ApiResponse<bool>> MarkAllAsReadAsync()
    {
        var now = DateTime.UtcNow;
        foreach (var item in _notifications.Values)
        {
            if (item.Unread)
            {
                item.Unread = false;
                item.ReadAt = now;
            }
        }

        return Task.FromResult(ApiResponse<bool>.SuccessResult(true, "All notifications marked as read."));
    }

    public Task<ApiResponse<bool>> ArchiveNotificationAsync(int id)
    {
        if (_notifications.TryGetValue(id, out var entity))
        {
            entity.IsArchived = true;
            return Task.FromResult(ApiResponse<bool>.SuccessResult(true, "Notification archived."));
        }

        return Task.FromResult(ApiResponse<bool>.FailureResult("Notification not found."));
    }

    public Task<ApiResponse<int>> BatchActionAsync(BatchNotificationActionDto request, string actor)
    {
        var count = 0;
        var now = DateTime.UtcNow;

        foreach (var id in request.NotificationIds)
        {
            if (_notifications.TryGetValue(id, out var entity))
            {
                switch (request.Action.ToLowerInvariant())
                {
                    case "mark_read":
                        entity.Unread = false;
                        entity.ReadAt = now;
                        count++;
                        break;
                    case "mark_unread":
                        entity.Unread = true;
                        entity.ReadAt = null;
                        count++;
                        break;
                    case "archive":
                    case "delete":
                        entity.IsArchived = true;
                        count++;
                        break;
                }
            }
        }

        return Task.FromResult(ApiResponse<int>.SuccessResult(count, $"{count} notifications updated."));
    }

    public Task<ApiResponse<NotificationPreferencesDto>> GetPreferencesAsync(string userId)
    {
        if (!_preferences.TryGetValue(userId, out var prefs))
        {
            prefs = new NotificationPreferencesDto();
            _preferences.TryAdd(userId, prefs);
        }

        return Task.FromResult(ApiResponse<NotificationPreferencesDto>.SuccessResult(prefs));
    }

    public Task<ApiResponse<NotificationPreferencesDto>> UpdatePreferencesAsync(string userId, NotificationPreferencesDto preferences)
    {
        _preferences.AddOrUpdate(userId, preferences, (_, _) => preferences);
        return Task.FromResult(ApiResponse<NotificationPreferencesDto>.SuccessResult(preferences, "Preferences saved successfully."));
    }

    private static NotificationDto MapToDto(SystemNotification entity)
    {
        return new NotificationDto
        {
            Id = entity.Id,
            Type = entity.Type,
            Title = entity.Title,
            Message = entity.Message,
            Priority = entity.Priority,
            Time = FormatRelativeTime(entity.CreatedAt),
            Date = FormatDateHeader(entity.CreatedAt),
            CreatedAt = entity.CreatedAt,
            Location = entity.Location,
            Unread = entity.Unread,
            Action = entity.Action,
            ActionUrl = entity.ActionUrl
        };
    }

    private static string FormatRelativeTime(DateTime createdAt)
    {
        var span = DateTime.UtcNow - createdAt;
        if (span.TotalMinutes < 1) return "Just now";
        if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} min ago";
        if (span.TotalHours < 24)
        {
            var h = (int)span.TotalHours;
            return $"{h} hour{(h > 1 ? "s" : "")} ago";
        }
        if (span.TotalDays < 2) return "Yesterday";
        if (span.TotalDays < 7) return $"{(int)span.TotalDays} days ago";
        return createdAt.ToString("MMM dd");
    }

    private static string FormatDateHeader(DateTime createdAt)
    {
        var span = DateTime.UtcNow.Date - createdAt.Date;
        if (span.Days == 0) return "Today";
        if (span.Days == 1) return "Yesterday";
        return createdAt.ToString("yyyy-MM-dd");
    }
}
