using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using CareLink.Core.Models;

namespace CareLink.Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;

    public NotificationService(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResultDto<NotificationItemDto>> GetPagedNotificationsAsync(
        NotificationQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _repository.GetPagedAsync(parameters, cancellationToken);

        return new PagedResultDto<NotificationItemDto>
        {
            Items = items.Select(MapToDto),
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
        return items.Select(MapToDto);
    }

    public async Task<NotificationItemDto?> GetNotificationByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken);
        return item != null ? MapToDto(item) : null;
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
            Details = dto.Details.Trim(),
            Reference = string.IsNullOrWhiteSpace(dto.Reference) ? string.Empty : dto.Reference.Trim(),
            Role = string.IsNullOrWhiteSpace(dto.Role) ? "ShelterAdmin" : dto.Role.Trim(),
            Unread = true,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(entity, cancellationToken);
        return MapToDto(created);
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

    private static NotificationItemDto MapToDto(NotificationItem entity)
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

    private static string FormatRelativeTime(DateTime dateTime)
    {
        var span = DateTime.UtcNow - dateTime;
        if (span.TotalMinutes < 1) return "Just now";
        if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} min ago";
        if (span.TotalHours < 24) return $"{(int)span.TotalHours} hr ago";
        if (span.TotalDays < 7) return $"{(int)span.TotalDays} day ago";
        return dateTime.ToString("dd MMM");
    }
}
