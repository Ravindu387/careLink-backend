using System.Collections.Concurrent;
using CareLink.Application.Common;
using CareLink.Application.DTOs;
using CareLink.Application.DTOs.AuditLogs;
using CareLink.Application.DTOs.Users;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using CareLink.Core.Models;
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
        var isUrgent = dto.Urgent ?? (
            dto.Category.Equals("urgent", StringComparison.OrdinalIgnoreCase) ||
            dto.Category.Equals("critical", StringComparison.OrdinalIgnoreCase)
        );

        var item = new NotificationItem
        {
            Title = dto.Title.Trim(),
            Description = dto.Description.Trim(),
            Type = dto.Category.Trim(),
            Icon = string.IsNullOrWhiteSpace(dto.Icon) ? "resident" : dto.Icon.Trim(),
            Unread = true,
            Urgent = isUrgent,
            Reference = dto.Reference?.Trim() ?? string.Empty,
            Details = string.IsNullOrWhiteSpace(dto.Details) ? dto.Description.Trim() : dto.Details.Trim(),
            ActionUrl = dto.ActionUrl,
            ActionLabel = dto.ActionLabel,
            Role = string.IsNullOrWhiteSpace(dto.Role) ? "all" : dto.Role.Trim(),
            CreatedAt = DateTime.UtcNow,
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
        var all = await _repository.GetAllAsync(new NotificationFilterParams());
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
        var all = await _repository.GetAllAsync(new NotificationFilterParams());
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
        var item = await _repository.GetByIdAsync(id);
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
            Type = request.Type.Trim(),
            Urgent = request.Priority.Equals("Critical", StringComparison.OrdinalIgnoreCase),
            Unread = true,
            Reference = request.Location?.Trim() ?? string.Empty,
            ActionLabel = request.Action?.Trim(),
            ActionUrl = request.ActionUrl?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(item);
        await _auditLogService.CreateAuditLogAsync(new CreateAuditLogRequestDto
        {
            Action = "CreateNotification",
            Actor = actor,
            Record = created.Id.ToString(),
            Change = $"Created {created.Category} notification",
            Rationale = created.Title,
            Type = "Notification"
        });

        return ApiResponse<AdminDtos.NotificationDto>.SuccessResult(MapToAdminDto(created, 1), "Notification sent.");
    }

    public async Task<ApiResponse<bool>> MarkAsReadAsync(int id)
    {
        var success = await _repository.MarkAsReadAsync(id);
        if (!success)
        {
            return ApiResponse<bool>.FailureResult($"Notification '{id}' not found.");
        }

        return ApiResponse<bool>.SuccessResult(true, "Notification marked as read.");
    }

    public async Task<ApiResponse<bool>> MarkAllAsReadAsync()
    {
        await _repository.MarkAllReadAsync();
        return ApiResponse<bool>.SuccessResult(true, "All notifications marked as read.");
    }

    public async Task<ApiResponse<bool>> ArchiveNotificationAsync(int id)
    {
        var success = await _repository.DeleteAsync(id);
        if (!success)
        {
            return ApiResponse<bool>.FailureResult($"Notification '{id}' not found.");
        }

        return ApiResponse<bool>.SuccessResult(true, "Notification archived.");
    }

    public async Task<ApiResponse<int>> BatchActionAsync(
        AdminDtos.BatchNotificationActionDto request,
        string actor)
    {
        var targetIds = new HashSet<int>(request.NotificationIds);
        var affected = 0;

        foreach (var id in targetIds)
        {
            switch (request.Action.ToLowerInvariant())
            {
                case "markasread":
                case "read":
                    if (await _repository.MarkAsReadAsync(id)) affected++;
                    break;
                case "delete":
                case "archive":
                    if (await _repository.DeleteAsync(id)) affected++;
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

    #region Shelter & Funding Officer Notification Methods (Prabhani)

    public async Task<PagedResultDto<NotificationItemDto>> GetPagedNotificationsAsync(
        NotificationQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _repository.GetPagedAsync(parameters, cancellationToken);

        return new PagedResultDto<NotificationItemDto>
        {
            Items = items.Select(MapToItemDto),
            TotalCount = totalCount,
            Page = parameters.Page,
            PageSize = parameters.PageSize
        };
    }

    public async Task<IEnumerable<NotificationItemDto>> GetAllNotificationsAsync(
        NotificationQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var items = await _repository.GetAllAsync(parameters, cancellationToken);
        return items.Select(MapToItemDto);
    }

    public async Task<NotificationItemDto?> GetNotificationByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken);
        return item != null ? MapToItemDto(item) : null;
    }

    public async Task<NotificationSummaryDto> GetSummaryAsync(
        string? role = "ShelterAdmin",
        CancellationToken cancellationToken = default)
    {
        var all = await _repository.GetAllAsync(new NotificationQueryParameters
        {
            Role = role,
            Page = 1,
            PageSize = 1000
        }, cancellationToken);

        var list = all.ToList();

        var unread = list.Count(n => n.Unread);
        var critical = list.Count(n => n.Type.Equals("Critical", StringComparison.OrdinalIgnoreCase));
        var requests = list.Count(n => n.Type.Equals("Requests", StringComparison.OrdinalIgnoreCase));
        var inventory = list.Count(n => n.Type.Equals("Inventory", StringComparison.OrdinalIgnoreCase));
        var system = list.Count(n => n.Type.Equals("System", StringComparison.OrdinalIgnoreCase));
        var flags = list.Count(n => n.Type.Equals("Flags", StringComparison.OrdinalIgnoreCase));
        var verification = list.Count(n => n.Type.Equals("Verification", StringComparison.OrdinalIgnoreCase));
        var campaigns = list.Count(n => n.Type.Equals("Campaigns", StringComparison.OrdinalIgnoreCase));
        var allocation = list.Count(n => n.Type.Equals("Allocation", StringComparison.OrdinalIgnoreCase));
        var expenditure = list.Count(n => n.Type.Equals("Expenditure", StringComparison.OrdinalIgnoreCase));
        var reports = list.Count(n => n.Type.Equals("Reports", StringComparison.OrdinalIgnoreCase));

        return new NotificationSummaryDto
        {
            UnreadCount = unread,
            CriticalCount = critical,
            RequestCount = requests,
            InventoryCount = inventory,
            SystemCount = system,
            FlagsCount = flags,
            VerificationCount = verification,
            CampaignsCount = campaigns,
            AllocationCount = allocation,
            ExpenditureCount = expenditure,
            ReportsCount = reports,
            TotalCount = list.Count
        };
    }

    public async Task<int> GetUnreadCountAsync(
        string? role = "ShelterAdmin",
        CancellationToken cancellationToken = default)
    {
        return await _repository.GetUnreadCountAsync(role, cancellationToken);
    }

    public async Task<NotificationItemDto> CreateNotificationAsync(
        CreateNotificationDto dto,
        CancellationToken cancellationToken = default)
    {
        var entity = new NotificationItem
        {
            Title = dto.Title.Trim(),
            Description = dto.Description.Trim(),
            Type = string.IsNullOrWhiteSpace(dto.Type) ? "System" : dto.Type.Trim(),
            Icon = string.IsNullOrWhiteSpace(dto.Icon) ? "resident" : dto.Icon.Trim(),
            Details = dto.Details?.Trim() ?? string.Empty,
            Reference = string.IsNullOrWhiteSpace(dto.Reference) ? string.Empty : dto.Reference.Trim(),
            Role = string.IsNullOrWhiteSpace(dto.Role) ? "ShelterAdmin" : dto.Role.Trim(),
            Unread = true,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(entity, cancellationToken);
        return MapToItemDto(created);
    }

    public async Task<bool> MarkAsReadAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _repository.MarkAsReadAsync(id, cancellationToken);
    }

    public async Task<int> MarkAllAsReadAsync(
        string? role = "ShelterAdmin",
        CancellationToken cancellationToken = default)
    {
        return await _repository.MarkAllAsReadAsync(role, cancellationToken);
    }

    public async Task<bool> DeleteNotificationAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _repository.DeleteAsync(id, cancellationToken);
    }

    public async Task<NotificationPreferenceDto> GetPreferencesAsync(
        string role = "ShelterAdmin",
        CancellationToken cancellationToken = default)
    {
        var pref = await _repository.GetPreferencesAsync(role, cancellationToken);
        if (pref == null)
        {
            pref = new NotificationPreference
            {
                Role = role,
                InApp = true,
                Email = true,
                Sms = false,
                Critical = true,
                Requests = true,
                Inventory = true,
                System = false,
                Verification = true,
                Campaigns = true,
                Reports = true,
                UpdatedAt = DateTime.UtcNow
            };
            pref = await _repository.SavePreferencesAsync(pref, cancellationToken);
        }

        return new NotificationPreferenceDto
        {
            Role = pref.Role,
            InApp = pref.InApp,
            Email = pref.Email,
            Sms = pref.Sms,
            Critical = pref.Critical,
            Requests = pref.Requests,
            Inventory = pref.Inventory,
            System = pref.System,
            Verification = pref.Verification,
            Campaigns = pref.Campaigns,
            Reports = pref.Reports
        };
    }

    public async Task<NotificationPreferenceDto> SavePreferencesAsync(
        string role,
        UpdateNotificationPreferenceDto dto,
        CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetPreferencesAsync(role, cancellationToken)
            ?? new NotificationPreference { Role = role };

        existing.InApp = dto.InApp;
        existing.Email = dto.Email;
        existing.Sms = dto.Sms;
        existing.Critical = dto.Critical;
        existing.Requests = dto.Requests;
        existing.Inventory = dto.Inventory;
        existing.System = dto.System;
        existing.Verification = dto.Verification;
        existing.Campaigns = dto.Campaigns;
        existing.Reports = dto.Reports;
        existing.UpdatedAt = DateTime.UtcNow;

        var saved = await _repository.SavePreferencesAsync(existing, cancellationToken);

        return new NotificationPreferenceDto
        {
            Role = saved.Role,
            InApp = saved.InApp,
            Email = saved.Email,
            Sms = saved.Sms,
            Critical = saved.Critical,
            Requests = saved.Requests,
            Inventory = saved.Inventory,
            System = saved.System,
            Verification = saved.Verification,
            Campaigns = saved.Campaigns,
            Reports = saved.Reports
        };
    }

    #endregion

    #region Helper Mappers

    private static NotificationDto MapToSafnaDto(NotificationItem item)
    {
        return new NotificationDto
        {
            Id = item.Id.ToString(),
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

    private static NotificationItemDto MapToItemDto(NotificationItem entity)
    {
        return new NotificationItemDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            Type = entity.Type,
            Time = FormatRelativeTime(entity.CreatedAt),
            CreatedAtUtc = entity.CreatedAt,
            Icon = entity.Icon,
            Unread = entity.Unread,
            Details = entity.Details,
            Reference = entity.Reference,
            Role = entity.Role
        };
    }

    private static AdminDtos.NotificationDto MapToAdminDto(NotificationItem item, int fallbackId = 1)
    {
        return new AdminDtos.NotificationDto
        {
            Id = item.Id > 0 ? item.Id : fallbackId,
            Type = item.Category,
            Title = item.Title,
            Message = item.Description ?? string.Empty,
            Priority = item.Urgent ? "Critical" : "Normal",
            Location = item.Reference ?? string.Empty,
            Action = item.ActionLabel,
            ActionUrl = item.ActionUrl,
            Unread = item.Unread,
            CreatedAt = item.CreatedAt
        };
    }

    private static string FormatTime(NotificationItem item)
    {
        if (!string.IsNullOrWhiteSpace(item.Time))
        {
            return item.Time;
        }

        var elapsed = DateTime.UtcNow - item.CreatedAt;
        if (elapsed.TotalMinutes < 2) return "Just now";
        if (elapsed.TotalMinutes < 60) return $"{(int)elapsed.TotalMinutes} min ago";
        if (elapsed.TotalHours < 24) return $"{(int)elapsed.TotalHours} hr ago";
        if (elapsed.TotalDays < 7) return $"{(int)elapsed.TotalDays}d ago";
        return item.CreatedAt.ToString("MMM dd, yyyy");
    }

    private static string FormatRelativeTime(DateTime dateTime)
    {
        var span = DateTime.UtcNow - dateTime;
        if (span.TotalMinutes < 1) return "Just now";
        if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} min ago";
        if (span.TotalHours < 24) return $"{(int)span.TotalHours} hr ago";
        if (span.TotalDays < 7) return $"{(int)span.TotalDays} day ago";
        return dateTime.ToString("dd MMM");
    }

    #endregion
}
