using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;

namespace CareLink.Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;

    public NotificationService(INotificationRepository repository)
    {
        _repository = repository;
    }

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
            Items = items.Select(MapToDto).ToList(),
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
        return items.Select(MapToDto).ToList();
    }

    public async Task<NotificationDto?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken);
        return item == null ? null : MapToDto(item);
    }

    public async Task<NotificationDto> CreateAsync(
        CreateNotificationDto dto,
        CancellationToken cancellationToken = default)
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
        return MapToDto(created);
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

    public async Task<NotificationPreferencesDto> GetPreferencesAsync(CancellationToken cancellationToken = default)
    {
        var p = await _repository.GetPreferencesAsync(cancellationToken);
        return new NotificationPreferencesDto
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

    public async Task<NotificationPreferencesDto> UpdatePreferencesAsync(
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
        return new NotificationPreferencesDto
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

    private static NotificationDto MapToDto(NotificationItem item)
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
}
