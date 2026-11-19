using System.Collections.Concurrent;
using System.Text;
using CareLink.Application.Common;
using CareLink.Application.DTOs.SecurityEvents;
using CareLink.Application.DTOs.Users;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;

namespace CareLink.Application.Services;

public class SecurityEventService : ISecurityEventService
{
    private readonly ConcurrentDictionary<string, SecurityEvent> _events = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, SecurityPolicyAction> _policyActions = new(StringComparer.OrdinalIgnoreCase);
    private int _idSequence = 8850;
    private int _policySequence = 100;
    private int _blockedAttemptsBase = 842;

    public SecurityEventService()
    {
        InitializeDefaultEvents();
    }

    private void InitializeDefaultEvents()
    {
        var now = DateTime.UtcNow;

        var seedList = new List<SecurityEvent>
        {
            new()
            {
                Id = "SEC-8842",
                Type = "Repeated failed sign-in",
                User = "USR-10312",
                Source = "185.42.18.9",
                Severity = "High",
                Status = "Investigating",
                CreatedAt = now.AddHours(-1).AddMinutes(-18),
                Description = "Five consecutive invalid password attempts from an unfamiliar external IP address.",
                Environment = "Production"
            },
            new()
            {
                Id = "SEC-8839",
                Type = "Privileged role assignment",
                User = "USR-10458",
                Source = "Admin Console",
                Severity = "Medium",
                Status = "Review Required",
                CreatedAt = now.AddHours(-2).AddMinutes(-42),
                Description = "Elevation of user role to Emergency Coordinator without secondary ticket linkage.",
                Environment = "Production"
            },
            new()
            {
                Id = "SEC-8831",
                Type = "New device sign-in",
                User = "USR-10482",
                Source = "41.74.22.18",
                Severity = "Low",
                Status = "Resolved",
                CreatedAt = now.AddHours(-3).AddMinutes(-14),
                ResolvedAt = now.AddHours(-2).AddMinutes(-50),
                ResolvedBy = "Security Automation",
                ResolutionNote = "Device verified via two-factor OTP authentication.",
                Description = "User logged in using Safari on iOS from an unrecognised user agent.",
                Environment = "Production"
            },
            new()
            {
                Id = "SEC-8824",
                Type = "Session token revoked",
                User = "USR-10291",
                Source = "System Policy",
                Severity = "Low",
                Status = "Resolved",
                CreatedAt = now.AddHours(-4).AddMinutes(-30),
                ResolvedAt = now.AddHours(-4).AddMinutes(-25),
                ResolvedBy = "System Service",
                ResolutionNote = "Token successfully revoked after scheduled credential rotation.",
                Description = "Idle administrative session terminated pursuant to maximum lease policy.",
                Environment = "Production"
            },
            new()
            {
                Id = "SEC-8819",
                Type = "Multiple login attempts",
                User = "USR-10521",
                Source = "192.168.1.24",
                Severity = "High",
                Status = "Review Required",
                CreatedAt = now.AddHours(-5).AddMinutes(-45),
                Description = "Rapid credential-stuffing sequence intercepted by rate limiting filter.",
                Environment = "Production"
            },
            new()
            {
                Id = "SEC-8814",
                Type = "Unusual account activity",
                User = "USR-10410",
                Source = "Web Portal",
                Severity = "Medium",
                Status = "Resolved",
                CreatedAt = now.AddHours(-21).AddMinutes(-20),
                ResolvedAt = now.AddHours(-20),
                ResolvedBy = "Alex Kimani",
                ResolutionNote = "Citizen confirmed relocation and location mismatch cleared.",
                Description = "Simultaneous login sessions detected from two geographically distant regions.",
                Environment = "Production"
            },
            new()
            {
                Id = "SEC-8809",
                Type = "Unauthorized API access attempt",
                User = "Anonymous",
                Source = "203.0.113.88",
                Severity = "Medium",
                Status = "Resolved",
                CreatedAt = now.AddDays(-2).AddHours(-4),
                ResolvedAt = now.AddDays(-2).AddHours(-3),
                ResolvedBy = "System Firewall",
                ResolutionNote = "IP temporarily blocked via edge rate limiting rule.",
                Description = "Attempted access to internal administrative endpoint without valid JWT bearer token.",
                Environment = "Production"
            },
            new()
            {
                Id = "SEC-8802",
                Type = "MFA challenge bypassed attempt",
                User = "USR-10115",
                Source = "198.51.100.14",
                Severity = "High",
                Status = "Resolved",
                CreatedAt = now.AddDays(-3).AddHours(-7),
                ResolvedAt = now.AddDays(-3).AddHours(-6),
                ResolvedBy = "Marta Silva",
                ResolutionNote = "Account placed in quarantine pending identity re-verification.",
                Description = "Anomalous response patterns detected during second-factor verification step.",
                Environment = "Production"
            }
        };

        foreach (var item in seedList)
        {
            _events[item.Id] = item;
        }
    }

    public Task<ApiResponse<PaginatedListDto<SecurityEventDto>>> GetEventsAsync(SecurityEventQueryDto query)
    {
        var items = _events.Values.AsEnumerable();

        // 1. Text Search
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLowerInvariant();
            items = items.Where(e =>
                e.Id.ToLowerInvariant().Contains(search) ||
                e.Type.ToLowerInvariant().Contains(search) ||
                e.User.ToLowerInvariant().Contains(search) ||
                e.Source.ToLowerInvariant().Contains(search) ||
                (e.Description != null && e.Description.ToLowerInvariant().Contains(search)));
        }

        // 2. Severity Filter
        if (!string.IsNullOrWhiteSpace(query.Severity) &&
            !query.Severity.Equals("All severities", StringComparison.OrdinalIgnoreCase))
        {
            items = items.Where(e => e.Severity.Equals(query.Severity, StringComparison.OrdinalIgnoreCase));
        }

        // 3. Type Filter
        if (!string.IsNullOrWhiteSpace(query.Type) &&
            !query.Type.Equals("All event types", StringComparison.OrdinalIgnoreCase))
        {
            items = items.Where(e => e.Type.Equals(query.Type, StringComparison.OrdinalIgnoreCase));
        }

        // 4. Status Filter
        if (!string.IsNullOrWhiteSpace(query.Status) &&
            !query.Status.Equals("All statuses", StringComparison.OrdinalIgnoreCase))
        {
            items = items.Where(e => e.Status.Equals(query.Status, StringComparison.OrdinalIgnoreCase));
        }

        // 5. Date Filter
        if (!string.IsNullOrWhiteSpace(query.DateFilter) &&
            !query.DateFilter.Equals("All time", StringComparison.OrdinalIgnoreCase))
        {
            var now = DateTime.UtcNow;
            if (query.DateFilter.Equals("Last 24 hours", StringComparison.OrdinalIgnoreCase))
            {
                items = items.Where(e => e.CreatedAt >= now.AddHours(-24));
            }
            else if (query.DateFilter.Equals("Last 7 days", StringComparison.OrdinalIgnoreCase))
            {
                items = items.Where(e => e.CreatedAt >= now.AddDays(-7));
            }
            else if (query.DateFilter.Equals("Last 30 days", StringComparison.OrdinalIgnoreCase))
            {
                items = items.Where(e => e.CreatedAt >= now.AddDays(-30));
            }
        }

        // Order by latest created
        var ordered = items.OrderByDescending(e => e.CreatedAt).ToList();

        var totalCount = ordered.Count;
        var page = query.PageNumber > 0 ? query.PageNumber : 1;
        var pageSize = query.PageSize > 0 ? query.PageSize : 10;

        var pagedItems = ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(MapToDto)
            .ToList();

        var result = new PaginatedListDto<SecurityEventDto>
        {
            Items = pagedItems,
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize
        };

        return Task.FromResult(ApiResponse<PaginatedListDto<SecurityEventDto>>.SuccessResult(result));
    }

    public Task<ApiResponse<SecurityEventDto>> GetEventByIdAsync(string id)
    {
        if (_events.TryGetValue(id, out var ev))
        {
            return Task.FromResult(ApiResponse<SecurityEventDto>.SuccessResult(MapToDto(ev)));
        }

        return Task.FromResult(ApiResponse<SecurityEventDto>.FailureResult($"Security event '{id}' not found."));
    }

    public Task<ApiResponse<SecurityEventStatsDto>> GetStatsAsync()
    {
        var all = _events.Values.ToList();
        var now = DateTime.UtcNow;

        var openEvents = all.Count(e => !e.Status.Equals("Resolved", StringComparison.OrdinalIgnoreCase));
        var highSeverityEvents = all.Count(e => e.Severity.Equals("High", StringComparison.OrdinalIgnoreCase) && !e.Status.Equals("Resolved", StringComparison.OrdinalIgnoreCase));
        var resolvedToday = all.Count(e => e.Status.Equals("Resolved", StringComparison.OrdinalIgnoreCase) &&
            (e.ResolvedAt.HasValue ? e.ResolvedAt.Value >= now.Date : e.CreatedAt >= now.Date));

        string threatLevel;
        string threatHeadline;

        if (highSeverityEvents >= 2)
        {
            threatLevel = "Critical";
            threatHeadline = $"{highSeverityEvents} high-severity authentication events require immediate containment.";
        }
        else if (highSeverityEvents == 1)
        {
            threatLevel = "Elevated";
            threatHeadline = "One high-severity authentication event requires investigation.";
        }
        else if (openEvents > 0)
        {
            threatLevel = "Normal";
            threatHeadline = $"{openEvents} security events undergoing standard administrative review.";
        }
        else
        {
            threatLevel = "Secure";
            threatHeadline = "All security events resolved. Zero active threats detected.";
        }

        var stats = new SecurityEventStatsDto
        {
            OpenEvents = openEvents,
            OpenEventsDetail = openEvents > 0 ? $"{openEvents} require action" : "All clear",
            HighSeverityEvents = highSeverityEvents,
            HighSeverityDetail = highSeverityEvents > 0 ? "Investigation active" : "None active",
            BlockedAttempts = _blockedAttemptsBase,
            BlockedAttemptsDetail = "Last 24 hours",
            ResolvedToday = resolvedToday,
            ResolvedTodayDetail = "Median 18 minutes",
            ThreatLevel = threatLevel,
            ThreatHeadline = threatHeadline,
            MonitoringStatus = "Monitoring active",
            TotalEvents = all.Count
        };

        return Task.FromResult(ApiResponse<SecurityEventStatsDto>.SuccessResult(stats));
    }

    public Task<ApiResponse<IReadOnlyList<string>>> GetEventTypesAsync()
    {
        var types = _events.Values
            .Select(e => e.Type)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(t => t)
            .ToList();

        return Task.FromResult(ApiResponse<IReadOnlyList<string>>.SuccessResult(types));
    }

    public Task<ApiResponse<SecurityEventDto>> CreateEventAsync(CreateSecurityEventRequestDto request, string? actor = null)
    {
        var nextId = $"SEC-{Interlocked.Increment(ref _idSequence)}";
        var newEvent = new SecurityEvent
        {
            Id = nextId,
            Type = request.Type.Trim(),
            User = request.User.Trim(),
            Source = request.Source.Trim(),
            Severity = string.IsNullOrWhiteSpace(request.Severity) ? "Low" : request.Severity.Trim(),
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Investigating" : request.Status.Trim(),
            Description = request.Description?.Trim(),
            CreatedAt = DateTime.UtcNow,
            Environment = string.IsNullOrWhiteSpace(request.Environment) ? "Production" : request.Environment.Trim()
        };

        if (newEvent.Status.Equals("Resolved", StringComparison.OrdinalIgnoreCase))
        {
            newEvent.ResolvedAt = DateTime.UtcNow;
            newEvent.ResolvedBy = actor ?? "System Admin";
        }

        _events[newEvent.Id] = newEvent;
        Interlocked.Increment(ref _blockedAttemptsBase);

        return Task.FromResult(ApiResponse<SecurityEventDto>.SuccessResult(
            MapToDto(newEvent), "Security event created successfully."));
    }

    public Task<ApiResponse<SecurityEventDto>> UpdateEventStatusAsync(string id, UpdateSecurityEventStatusRequestDto request, string? actor = null)
    {
        if (!_events.TryGetValue(id, out var ev))
        {
            return Task.FromResult(ApiResponse<SecurityEventDto>.FailureResult($"Security event '{id}' not found."));
        }

        ev.Status = request.Status.Trim();
        if (!string.IsNullOrWhiteSpace(request.ResolutionNote))
        {
            ev.ResolutionNote = request.ResolutionNote.Trim();
        }

        if (ev.Status.Equals("Resolved", StringComparison.OrdinalIgnoreCase))
        {
            ev.ResolvedAt = DateTime.UtcNow;
            ev.ResolvedBy = actor ?? "System Admin";
            if (string.IsNullOrWhiteSpace(ev.ResolutionNote))
            {
                ev.ResolutionNote = "Marked as resolved by administrator.";
            }
        }
        else
        {
            ev.ResolvedAt = null;
            ev.ResolvedBy = null;
        }

        return Task.FromResult(ApiResponse<SecurityEventDto>.SuccessResult(
            MapToDto(ev), $"Security event '{id}' marked as {ev.Status}."));
    }

    public Task<ApiResponse<SecurityPolicyActionDto>> RecordPolicyActionAsync(RecordSecurityPolicyActionRequestDto request, string? actor = null)
    {
        var nextId = $"POL-{Interlocked.Increment(ref _policySequence)}";
        var action = new SecurityPolicyAction
        {
            Id = nextId,
            ActionTitle = request.PolicyTitle.Trim(),
            Environment = request.Environment.Trim(),
            Scope = request.Scope.Trim(),
            ApprovalType = request.ApprovalType.Trim(),
            AdministrativeRationale = request.PolicyReason.Trim(),
            PerformedBy = actor ?? "System Administrator",
            RelatedEventId = request.RelatedEventId,
            CreatedAt = DateTime.UtcNow
        };

        _policyActions[action.Id] = action;

        var dto = new SecurityPolicyActionDto
        {
            Id = action.Id,
            PolicyTitle = action.ActionTitle,
            Environment = action.Environment,
            Scope = action.Scope,
            ApprovalType = action.ApprovalType,
            PolicyReason = action.AdministrativeRationale,
            PerformedBy = action.PerformedBy,
            RelatedEventId = action.RelatedEventId,
            CreatedAt = action.CreatedAt
        };

        return Task.FromResult(ApiResponse<SecurityPolicyActionDto>.SuccessResult(
            dto, "Security policy action recorded successfully and audited."));
    }

    public Task<byte[]> ExportEventsCsvAsync(SecurityEventQueryDto query)
    {
        var items = _events.Values.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLowerInvariant();
            items = items.Where(e =>
                e.Id.ToLowerInvariant().Contains(search) ||
                e.Type.ToLowerInvariant().Contains(search) ||
                e.User.ToLowerInvariant().Contains(search) ||
                e.Source.ToLowerInvariant().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(query.Severity) &&
            !query.Severity.Equals("All severities", StringComparison.OrdinalIgnoreCase))
        {
            items = items.Where(e => e.Severity.Equals(query.Severity, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.Type) &&
            !query.Type.Equals("All event types", StringComparison.OrdinalIgnoreCase))
        {
            items = items.Where(e => e.Type.Equals(query.Type, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.Status) &&
            !query.Status.Equals("All statuses", StringComparison.OrdinalIgnoreCase))
        {
            items = items.Where(e => e.Status.Equals(query.Status, StringComparison.OrdinalIgnoreCase));
        }

        var list = items.OrderByDescending(e => e.CreatedAt).ToList();

        var sb = new StringBuilder();
        sb.AppendLine("Event ID,Event Type,User,Source,Severity,Status,Timestamp,Resolved By,Resolution Note");

        foreach (var item in list)
        {
            var formattedTime = FormatTimestamp(item.CreatedAt);
            var row = string.Join(",",
                EscapeCsv(item.Id),
                EscapeCsv(item.Type),
                EscapeCsv(item.User),
                EscapeCsv(item.Source),
                EscapeCsv(item.Severity),
                EscapeCsv(item.Status),
                EscapeCsv(formattedTime),
                EscapeCsv(item.ResolvedBy ?? string.Empty),
                EscapeCsv(item.ResolutionNote ?? string.Empty)
            );
            sb.AppendLine(row);
        }

        return Task.FromResult(Encoding.UTF8.GetBytes(sb.ToString()));
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value)) return "\"\"";
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return $"\"{value}\"";
    }

    private static SecurityEventDto MapToDto(SecurityEvent ev)
    {
        return new SecurityEventDto
        {
            Id = ev.Id,
            Type = ev.Type,
            User = ev.User,
            Source = ev.Source,
            Severity = ev.Severity,
            Status = ev.Status,
            Timestamp = FormatTimestamp(ev.CreatedAt),
            CreatedAt = ev.CreatedAt,
            Description = ev.Description,
            ResolutionNote = ev.ResolutionNote,
            ResolvedBy = ev.ResolvedBy,
            ResolvedAt = ev.ResolvedAt,
            Environment = ev.Environment
        };
    }

    private static string FormatTimestamp(DateTime dt)
    {
        return dt.ToString("dd MMM · HH:mm");
    }
}
