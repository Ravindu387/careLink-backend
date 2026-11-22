using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;

namespace CareLink.Application.Services;

public class PartnerReportService : IPartnerReportService
{
    private readonly IPartnerReportRepository _repository;

    public PartnerReportService(IPartnerReportRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<PartnerReportDto>> GetAllAsync(
        PartnerReportQueryParameters query,
        CancellationToken cancellationToken = default)
    {
        var filter = new PartnerReportFilterParams
        {
            Search = query.Search,
            Tab = query.Tab,
            Type = query.Type,
            Status = query.Status
        };

        var items = await _repository.GetAllAsync(filter, cancellationToken);
        return items.Select(MapToDto).ToList();
    }

    public async Task<PartnerReportDto?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken);
        return item == null ? null : MapToDto(item);
    }

    public async Task<PartnerReportDto> CreateAsync(
        CreatePartnerReportDto dto,
        CancellationToken cancellationToken = default)
    {
        var id = $"RPT-{Random.Shared.Next(1000, 9999)}";
        var now = DateTimeOffset.UtcNow;

        var entity = new PartnerReport
        {
            Id = id,
            Title = dto.Title.Trim(),
            Type = dto.Type.Trim(),
            Owner = dto.Owner.Trim(),
            Period = dto.Period.Trim(),
            Submitted = "Not submitted",
            Status = string.IsNullOrWhiteSpace(dto.Status) ? "Draft" : dto.Status.Trim(),
            Progress = dto.Progress ?? 0,
            Description = dto.Description?.Trim(),
            OrganizationId = "ORG-027",
            CreatedAt = now,
            UpdatedAt = now
        };

        var added = await _repository.AddAsync(entity, cancellationToken);
        return MapToDto(added);
    }

    public async Task<PartnerReportDto?> UpdateAsync(
        string id,
        UpdatePartnerReportDto dto,
        CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing == null) return null;

        if (!string.IsNullOrWhiteSpace(dto.Title)) existing.Title = dto.Title.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Type)) existing.Type = dto.Type.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Owner)) existing.Owner = dto.Owner.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Period)) existing.Period = dto.Period.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Status)) existing.Status = dto.Status.Trim();
        if (dto.Progress.HasValue) existing.Progress = Math.Clamp(dto.Progress.Value, 0, 100);
        if (dto.Description != null) existing.Description = dto.Description.Trim();

        existing.UpdatedAt = DateTimeOffset.UtcNow;
        var updated = await _repository.UpdateAsync(existing, cancellationToken);
        return updated == null ? null : MapToDto(updated);
    }

    public async Task<PartnerReportDto?> UpdateStatusAsync(
        string id,
        string status,
        CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing == null) return null;

        existing.Status = status.Trim();
        if (status.Equals("Submitted", StringComparison.OrdinalIgnoreCase))
        {
            existing.Submitted = DateTimeOffset.UtcNow.ToString("dd MMM - HH:mm");
            existing.Progress = 100;
        }
        else if (status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
        {
            existing.Progress = 100;
        }

        existing.UpdatedAt = DateTimeOffset.UtcNow;
        var updated = await _repository.UpdateAsync(existing, cancellationToken);
        return updated == null ? null : MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _repository.DeleteAsync(id, cancellationToken);
    }

    public async Task<PartnerReportsAnalyticsDto> GetAnalyticsAsync(CancellationToken cancellationToken = default)
    {
        var a = await _repository.GetAnalyticsAsync(cancellationToken);
        return new PartnerReportsAnalyticsDto
        {
            ReportsSubmitted = a.ReportsSubmitted,
            OnTimeRate = a.OnTimeRate,
            OnTimeTrend = a.OnTimeTrend,
            PeopleReached = a.PeopleReached,
            PeopleReachedSubtitle = a.PeopleReachedSubtitle,
            NextDeadline = a.NextDeadline,
            NextDeadlineSubtitle = a.NextDeadlineSubtitle,
            Reminder = new DailyReportReminderDto
            {
                Title = a.Reminder.Title,
                Deadline = a.Reminder.Deadline,
                Subtitle = a.Reminder.Subtitle,
                Progress = a.Reminder.Progress
            },
            WeeklyReach = a.WeeklyReach.Select(w => new WeeklyReachDto
            {
                Day = w.Day,
                Planned = w.Planned,
                Reached = w.Reached
            }).ToList(),
            PerformanceTargets = a.PerformanceTargets.Select(p => new PerformanceTargetDto
            {
                Title = p.Title,
                Target = p.Target,
                Value = p.Value
            }).ToList(),
            Schedules = a.Schedules.Select(s => new ReportingScheduleDto
            {
                Name = s.Name,
                Deadline = s.Deadline,
                Status = s.Status
            }).ToList(),
            TotalReports = a.TotalReports,
            NeedsActionCount = a.NeedsActionCount,
            ActiveCount = a.ActiveCount,
            CompletedCount = a.CompletedCount,
            ArchivedCount = a.ArchivedCount,
            LastUpdated = a.LastUpdated
        };
    }

    public async Task ResetToDefaultAsync(CancellationToken cancellationToken = default)
    {
        await _repository.ResetToDefaultAsync(cancellationToken);
    }

    private static PartnerReportDto MapToDto(PartnerReport r)
    {
        return new PartnerReportDto
        {
            Id = r.Id,
            Title = r.Title,
            Type = r.Type,
            Owner = r.Owner,
            Period = r.Period,
            Submitted = r.Submitted,
            Status = r.Status,
            Progress = r.Progress,
            Description = r.Description,
            OrganizationId = r.OrganizationId,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        };
    }
}
