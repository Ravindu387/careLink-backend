using System.Collections.Concurrent;
using CareLink.Application.Common;
using CareLink.Application.DTOs;
using CareLink.Application.DTOs.AuditLogs;
using CareLink.Application.DTOs.Users;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using Microsoft.Extensions.Logging;
using AdminDtos = CareLink.Application.DTOs.Notifications;

namespace CareLink.Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<NotificationService> _logger;
    private readonly ConcurrentDictionary<string, AdminDtos.NotificationPreferencesDto> _adminPreferences = new(StringComparer.OrdinalIgnoreCase);

    public NotificationService(
        INotificationRepository repository,
        IAuditLogService auditLogService,
        ILogger<NotificationService> logger)
    {
        _repository = repository;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    #region Operational & Coordinator Methods (Safna)

    public async Task<PagedNotificationsDto> GetPagedAsync(
        NotificationQueryParameters query,
        CancellationToken cancellationToken = default)
    {
        var filter = new NotificationFilterParams
        {
            Search = query.Search,
            Category = query.Category,
            UnreadOnly = query.UnreadOnly,
            UrgentOnly = query.UrgentOnly,
            Role = query.Role,
            Page = Math.Max(1, query.Page),
            PageSize = Math.Clamp(query.PageSize, 1, 100)
        };

        var items = await _repository.GetAllAsync(filter, cancellationToken);
        var totalCount = await _repository.GetCountAsync(filter, cancellationToken);
        var metrics = await _repository.GetMetricsAsync(query.Role, cancellationToken);

        var totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize);
        if (totalPages < 1) totalPages = 1;

        return new PagedNotificationsDto
        {
            Items = items.Select(MapToSafnaDto).ToList(),
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalPages = totalPages,
            UnreadCount = metrics.Unread
        };
    }

    public async Task<IReadOnlyList<NotificationDto>> GetAllAsync(
        NotificationQueryParameters query,
        CancellationToken cancellationToken = default)
    {
        var filter = new NotificationFilterParams
        {
            Search = query.Search,
            Category = query.Category,
            UnreadOnly = query.UnreadOnly,
            UrgentOnly = query.UrgentOnly,
            Role = query.Role,
            Page = 1,
            PageSize = 500
        };

        var items = await _repository.GetAllAsync(filter, cancellationToken);
        return items.Select(MapToSafnaDto).ToList();
    }

    public async Task<NotificationDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken);
        return item == null ? null : MapToSafnaDto(item);
    }

    public async Task<NotificationDto> CreateAsync(CreateNotificationDto dto, CancellationToken cancellationToken = default)
    {
        var id = Guid.NewGuid().ToString("N")[..8];
        var isUrgent = dto.Urgent ?? (
            dto.Category.Equals("urgent", StringComparison.OrdinalIgnoreCase) ||
            dto.Category.Equals("critical", StringComparison.OrdinalIgnoreCase)
        );

        var item = new NotificationItem
        {
            Id = id,
            Title = dto.Title.Trim(),
            Description = dto.Description.Trim(),
            Category = dto.Category.Trim(),
            Unread = true,
            Urgent = isUrgent,
            Reference = string.IsNullOrWhiteSpace(dto.Reference) ? $"NOTIF-{id[..4].ToUpperInvariant()}" : dto.Reference.Trim(),
            Details = string.IsNullOrWhiteSpace(dto.Details) ? dto.Description.Trim() : dto.Details.Trim(),
            ActionUrl = dto.ActionUrl,
            ActionLabel = dto.ActionLabel,
            TargetRole = string.IsNullOrWhiteSpace(dto.TargetRole) ? "all" : dto.TargetRole.Trim().ToLowerInvariant(),
            CreatedAt = DateTimeOffset.UtcNow,
            Time = "Just now"
        };

        var created = await _repository.AddAsync(item, cancellationToken);
        return MapToSafnaDto(created);
    }

    public async Task<bool> MarkReadAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _repository.MarkReadAsync(id, cancellationToken);
    }

    public async Task<int> MarkAllReadAsync(string? role = null, CancellationToken cancellationToken = default)
    {
        return await _repository.MarkAllReadAsync(role, cancellationToken);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _repository.DeleteAsync(id, cancellationToken);
    }

    public async Task<CareLink.Application.DTOs.NotificationPreferencesDto> GetPreferencesAsync(
        CancellationToken cancellationToken = default)
    {
        var p = await _repository.GetPreferencesAsync(cancellationToken);
        return new CareLink.Application.DTOs.NotificationPreferencesDto
        {
            Critical = p.Critical,
            Ai = p.Ai,
            Resources = p.Resources,
            Shelter = p.Shelter,
            Partner = p.Partner,
            Reports = p.Reports,
            Urgent = p.Urgent,
            Referrals = p.Referrals,
            Collaboration = p.Collaboration,
            Reporting = p.Reporting,
            Team = p.Team,
            Email = p.Email,
            Push = p.Push,
            UpdatedAt = p.UpdatedAt
        };
    }

    public async Task<CareLink.Application.DTOs.NotificationPreferencesDto> UpdatePreferencesAsync(
        UpdateNotificationPreferencesDto dto,
        CancellationToken cancellationToken = default)
    {
        var current = await _repository.GetPreferencesAsync(cancellationToken);
        var p = new NotificationPreferences
        {
            Critical = dto.Critical ?? current.Critical,
            Ai = dto.Ai ?? current.Ai,
            Resources = dto.Resources ?? current.Resources,
            Shelter = dto.Shelter ?? current.Shelter,
            Partner = dto.Partner ?? current.Partner,
            Reports = dto.Reports ?? current.Reports,
            Urgent = dto.Urgent ?? current.Urgent,
            Referrals = dto.Referrals ?? current.Referrals,
            Collaboration = dto.Collaboration ?? current.Collaboration,
            Reporting = dto.Reporting ?? current.Reporting,
            Team = dto.Team ?? current.Team,
            Email = dto.Email ?? current.Email,
            Push = dto.Push ?? current.Push,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var updated = await _repository.UpdatePreferencesAsync(p, cancellationToken);
        return new CareLink.Application.DTOs.NotificationPreferencesDto
        {
            Critical = updated.Critical,
            Ai = updated.Ai,
            Resources = updated.Resources,
            Shelter = updated.Shelter,
            Partner = updated.Partner,
            Reports = updated.Reports,
            Urgent = updated.Urgent,
            Referrals = updated.Referrals,
            Collaboration = updated.Collaboration,
            Reporting = updated.Reporting,
            Team = updated.Team,
            Email = updated.Email,
            Push = updated.Push,
            UpdatedAt = updated.UpdatedAt
        };
    }

    public async Task<NotificationMetricsDto> GetMetricsAsync(string? role = null, CancellationToken cancellationToken = default)
    {
        var m = await _repository.GetMetricsAsync(role, cancellationToken);
        return new NotificationMetricsDto
        {
            All = m.All,
            Unread = m.Unread,
            Urgent = m.Urgent,
            Critical = m.Critical,
            Ai = m.Ai,
            Resource = m.Resource,
            Shelter = m.Shelter,
            Partner = m.Partner,
            Report = m.Report,
            Collaboration = m.Collaboration,
            Reporting = m.Reporting,
            Team = m.Team,
            Agreement = m.Agreement,
            LastUpdated = m.LastUpdated
        };
    }

    public async Task ResetDefaultDataAsync(CancellationToken cancellationToken = default)
    {
        await _repository.ResetDefaultDataAsync(cancellationToken);
    }

    #endregion

    #region Administrative Methods (dev)

    public async Task<ApiResponse<PaginatedListDto<AdminDtos.NotificationDto>>> GetNotificationsAsync(
        AdminDtos.NotificationQueryDto query)
    {
        var all = await _repository.GetAllAsync();
        var q = all.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(n =>
                n.Title.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                (n.Description != null && n.Description.Contains(s, StringComparison.OrdinalIgnoreCase)) ||
                (n.Reference != null && n.Reference.Contains(s, StringComparison.OrdinalIgnoreCase)));
        }

        if (!string.IsNullOrWhiteSpace(query.Type))
        {
            q = q.Where(n => n.Category.Equals(query.Type, StringComparison.OrdinalIgnoreCase));
        }

        if (query.UnreadOnly == true)
        {
            q = q.Where(n => n.Unread);
        }

        var list = q.Select((item, idx) => MapToAdminDto(item, idx + 1)).ToList();
        var total = list.Count;
        var paged = list
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        var result = new PaginatedListDto<AdminDtos.NotificationDto>
        {
            Items = paged,
            TotalCount = total,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };

        return ApiResponse<PaginatedListDto<AdminDtos.NotificationDto>>.SuccessResult(result);
    }

    public async Task<ApiResponse<AdminDtos.NotificationStatsDto>> GetStatsAsync()
    {
        var all = await _repository.GetAllAsync();
        var stats = new AdminDtos.NotificationStatsDto
        {
            Total = all.Count,
            UnreadCount = all.Count(n => n.Unread),
            CriticalCount = all.Count(n => n.Urgent),
            HighPriorityCount = all.Count(n => n.Category.Equals("Shelter", StringComparison.OrdinalIgnoreCase) || n.Category.Equals("Resource", StringComparison.OrdinalIgnoreCase)),
            EmergencyCount = all.Count(n => n.Category.Equals("Emergency", StringComparison.OrdinalIgnoreCase) || n.Urgent),
            AssistanceCount = all.Count(n => n.Category.Equals("Referrals", StringComparison.OrdinalIgnoreCase) || n.Category.Equals("Referral", StringComparison.OrdinalIgnoreCase)),
            VolunteerCount = all.Count(n => n.Category.Equals("Team", StringComparison.OrdinalIgnoreCase)),
            ResourceCount = all.Count(n => n.Category.Equals("Resources", StringComparison.OrdinalIgnoreCase) || n.Category.Equals("Resource", StringComparison.OrdinalIgnoreCase)),
            SystemCount = all.Count(n => n.Category.Equals("System", StringComparison.OrdinalIgnoreCase))
        };

        return ApiResponse<AdminDtos.NotificationStatsDto>.SuccessResult(stats);
    }

    public async Task<ApiResponse<AdminDtos.NotificationDto>> GetNotificationByIdAsync(int id)
    {
        var all = await _repository.GetAllAsync();
        var item = all.FirstOrDefault(n => MatchesIntId(n.Id, id));
        if (item == null)
        {
            return ApiResponse<AdminDtos.NotificationDto>.FailureResult($"Notification '{id}' not found.");
        }

        return ApiResponse<AdminDtos.NotificationDto>.SuccessResult(MapToAdminDto(item, id));
    }

    public async Task<ApiResponse<AdminDtos.NotificationDto>> CreateNotificationAsync(
        AdminDtos.CreateNotificationRequestDto request,
        string actor)
    {
        var item = new NotificationItem
        {
            Title = request.Title.Trim(),
            Description = request.Message.Trim(),
            Category = request.Type.Trim(),
            Urgent = request.Priority.Equals("Critical", StringComparison.OrdinalIgnoreCase),
            Unread = true,
            Reference = request.Location?.Trim(),
            ActionLabel = request.Action?.Trim(),
            ActionUrl = request.ActionUrl?.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        var created = await _repository.AddAsync(item);
        await _auditLogService.CreateAuditLogAsync(new CreateAuditLogRequestDto
        {
            Action = "CreateNotification",
            Actor = actor,
            Record = created.Id,
            Change = $"Created {created.Category} notification",
            Rationale = created.Title,
            Type = "Notification"
        });

        return ApiResponse<AdminDtos.NotificationDto>.SuccessResult(MapToAdminDto(created, 1), "Notification sent.");
    }

    public async Task<ApiResponse<bool>> MarkAsReadAsync(int id)
    {
        var all = await _repository.GetAllAsync();
        var item = all.FirstOrDefault(n => MatchesIntId(n.Id, id));
        if (item == null)
        {
            return ApiResponse<bool>.FailureResult($"Notification '{id}' not found.");
        }

        await _repository.MarkReadAsync(item.Id);
        return ApiResponse<bool>.SuccessResult(true, "Notification marked as read.");
    }

    public async Task<ApiResponse<bool>> MarkAllAsReadAsync()
    {
        await _repository.MarkAllReadAsync();
        return ApiResponse<bool>.SuccessResult(true, "All notifications marked as read.");
    }

    public async Task<ApiResponse<bool>> ArchiveNotificationAsync(int id)
    {
        var all = await _repository.GetAllAsync();
        var item = all.FirstOrDefault(n => MatchesIntId(n.Id, id));
        if (item == null)
        {
            return ApiResponse<bool>.FailureResult($"Notification '{id}' not found.");
        }

        await _repository.DeleteAsync(item.Id);
        return ApiResponse<bool>.SuccessResult(true, "Notification archived.");
    }

    public async Task<ApiResponse<int>> BatchActionAsync(
        AdminDtos.BatchNotificationActionDto request,
        string actor)
    {
        var all = await _repository.GetAllAsync();
        var targetIds = new HashSet<int>(request.NotificationIds);
        var matchingItems = all.Where(n => targetIds.Any(tid => MatchesIntId(n.Id, tid))).ToList();

        var affected = 0;
        foreach (var item in matchingItems)
        {
            switch (request.Action.ToLowerInvariant())
            {
                case "markasread":
                case "read":
                    await _repository.MarkReadAsync(item.Id);
                    affected++;
                    break;
                case "delete":
                case "archive":
                    await _repository.DeleteAsync(item.Id);
                    affected++;
                    break;
            }
        }

        await _auditLogService.CreateAuditLogAsync(new CreateAuditLogRequestDto
        {
            Action = "BatchNotificationAction",
            Actor = actor,
            Record = string.Join(",", request.NotificationIds),
            Change = $"Batch {request.Action} performed on {affected} notifications",
            Rationale = "Batch operation requested by administrator",
            Type = "System"
        });

        return ApiResponse<int>.SuccessResult(affected, $"{affected} notifications updated.");
    }

    public Task<ApiResponse<AdminDtos.NotificationPreferencesDto>> GetAdminPreferencesAsync(string userId)
    {
        var p = _adminPreferences.GetOrAdd(userId, _ => new AdminDtos.NotificationPreferencesDto());
        return Task.FromResult(ApiResponse<AdminDtos.NotificationPreferencesDto>.SuccessResult(p));
    }

    public Task<ApiResponse<AdminDtos.NotificationPreferencesDto>> UpdateAdminPreferencesAsync(
        string userId,
        AdminDtos.NotificationPreferencesDto preferences)
    {
        _adminPreferences[userId] = preferences;
        return Task.FromResult(ApiResponse<AdminDtos.NotificationPreferencesDto>.SuccessResult(preferences, "Notification preferences updated."));
    }

    #endregion

    #region Helper Mappers

    private static bool MatchesIntId(string entityId, int intId)
    {
        if (int.TryParse(entityId, out var id) && id == intId) return true;
        var digitsOnly = System.Text.RegularExpressions.Regex.Replace(entityId, @"\D", "");
        return int.TryParse(digitsOnly, out var num) && num == intId;
    }

    private static NotificationDto MapToSafnaDto(NotificationItem item)
    {
        return new NotificationDto
        {
            Id = item.Id,
            Title = item.Title,
            Description = item.Description,
            Time = FormatTime(item),
            Category = item.Category,
            Unread = item.Unread,
            Urgent = item.Urgent,
            Reference = item.Reference,
            Details = item.Details,
            ActionUrl = item.ActionUrl,
            ActionLabel = item.ActionLabel,
            TargetRole = item.TargetRole,
            CreatedAt = item.CreatedAt
        };
    }

    private static AdminDtos.NotificationDto MapToAdminDto(NotificationItem item, int fallbackId = 1)
    {
        var digitsOnly = System.Text.RegularExpressions.Regex.Replace(item.Id, @"\D", "");
        var idNum = int.TryParse(digitsOnly, out var parsed) ? parsed : fallbackId;

        return new AdminDtos.NotificationDto
        {
            Id = idNum,
            Type = item.Category,
            Title = item.Title,
            Message = item.Description ?? string.Empty,
            Priority = item.Urgent ? "Critical" : "Normal",
            Location = item.Reference ?? string.Empty,
            Action = item.ActionLabel,
            ActionUrl = item.ActionUrl,
            Unread = item.Unread,
            CreatedAt = item.CreatedAt.UtcDateTime
        };
    }

    private static string FormatTime(NotificationItem item)
    {
        if (!string.IsNullOrWhiteSpace(item.Time))
        {
            return item.Time;
        }

        var elapsed = DateTimeOffset.UtcNow - item.CreatedAt;
        if (elapsed.TotalMinutes < 2) return "Just now";
        if (elapsed.TotalMinutes < 60) return $"{(int)elapsed.TotalMinutes} min ago";
        if (elapsed.TotalHours < 24) return $"{(int)elapsed.TotalHours} hr ago";
        if (elapsed.TotalDays < 7) return $"{(int)elapsed.TotalDays}d ago";
        return item.CreatedAt.ToString("MMM dd, yyyy");
    }

    #endregion
}
