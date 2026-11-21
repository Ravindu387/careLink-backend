using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;

namespace CareLink.Application.Services;

public class PriorityQueueService : IPriorityQueueService
{
    private readonly IPriorityQueueRepository _repository;
    private static int _requestCounter = 2841;
    private static readonly object _counterLock = new();

    public PriorityQueueService(IPriorityQueueRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<PriorityQueueItemDto>> GetAllAsync(
        PriorityQueueQueryParameters? query = null,
        CancellationToken cancellationToken = default)
    {
        var filterParams = query == null
            ? null
            : new PriorityQueueFilterParams
            {
                Search = query.Search,
                Tab = query.Tab,
                CrisisEvent = query.CrisisEvent,
                Priority = query.Priority,
                Category = query.Category,
                Status = query.Status,
                Organisation = query.Organisation,
            };

        var list = await _repository.GetAllAsync(filterParams, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        return list.Select(item => MapToDto(item, now)).ToList();
    }

    public async Task<PagedPriorityQueueDto> GetPagedAsync(
        PriorityQueueQueryParameters query,
        CancellationToken cancellationToken = default)
    {
        var all = await GetAllAsync(query, cancellationToken);
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 10 : query.PageSize;

        var totalCount = all.Count;
        var totalPages = Math.Max(1, (int)Math.Ceiling((double)totalCount / pageSize));
        var items = all
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedPriorityQueueDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages,
        };
    }

    public async Task<PriorityQueueItemDto?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken);
        return item == null ? null : MapToDto(item, DateTimeOffset.UtcNow);
    }

    public async Task<PriorityQueueItemDto> CreateAsync(
        CreatePriorityRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        string id;
        lock (_counterLock)
        {
            _requestCounter++;
            id = $"REQ-{_requestCounter}";
        }

        var level = dto.PriorityLevel ?? "Critical";
        var confidence = dto.Confidence ?? 94;
        var display = !string.IsNullOrWhiteSpace(dto.Priority)
            ? dto.Priority.Trim()
            : $"{level} · {confidence}%";

        var now = DateTimeOffset.UtcNow;
        var item = new PriorityQueueItem
        {
            Id = id,
            Category = dto.Category.Trim(),
            Location = dto.Location.Trim(),
            CrisisEvent = dto.CrisisEvent?.Trim() ?? "Monsoon Floods 2025",
            PriorityLevel = level,
            Confidence = confidence,
            PriorityDisplay = display,
            Verification = dto.Verification?.Trim() ?? "Pending verification",
            Assigned = dto.Assigned?.Trim() ?? "Unassigned",
            Status = "Submitted",
            Organisation = "National Disaster Authority",
            Description = dto.Description?.Trim(),
            AffectedPeopleCount = dto.AffectedPeopleCount ?? 1,
            HasVulnerableMembers = false,
            IsLifeThreatening = level.Equals("Critical", StringComparison.OrdinalIgnoreCase),
            IsOverdue = false,
            IsPotentialDuplicate = false,
            CreatedAt = now,
        };

        var created = await _repository.AddAsync(item, cancellationToken);
        return MapToDto(created, now);
    }

    public async Task<PriorityQueueItemDto?> AssignTeamAsync(
        string id,
        AssignTeamDto dto,
        CancellationToken cancellationToken = default)
    {
        var updated = await _repository.AssignTeamAsync(id, dto.AssignedTeam, cancellationToken);
        return updated == null ? null : MapToDto(updated, DateTimeOffset.UtcNow);
    }

    public async Task<PriorityQueueItemDto?> UpdateStatusAsync(
        string id,
        UpdatePriorityStatusDto dto,
        CancellationToken cancellationToken = default)
    {
        var updated = await _repository.UpdateStatusAsync(id, dto.Status, cancellationToken);
        return updated == null ? null : MapToDto(updated, DateTimeOffset.UtcNow);
    }

    public async Task<PriorityQueueItemDto?> UpdatePriorityAsync(
        string id,
        UpdatePriorityLevelDto dto,
        CancellationToken cancellationToken = default)
    {
        var confidence = dto.Confidence ?? 90;
        var updated = await _repository.UpdatePriorityAsync(id, dto.PriorityLevel, confidence, cancellationToken);
        return updated == null ? null : MapToDto(updated, DateTimeOffset.UtcNow);
    }

    public async Task<bool> DeleteAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        return await _repository.DeleteAsync(id, cancellationToken);
    }

    public async Task<PriorityQueueMetricsDto> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        var m = await _repository.GetMetricsAsync(cancellationToken);
        return new PriorityQueueMetricsDto
        {
            TotalCount = m.TotalCount,
            UnassignedCount = m.UnassignedCount,
            OverdueCount = m.OverdueCount,
            PotentialDuplicatesCount = m.PotentialDuplicatesCount,
            CriticalOrOverdueRequiringAttentionCount = m.CriticalOrOverdueRequiringAttentionCount,
            CriticalCount = m.CriticalCount,
            HighCount = m.HighCount,
            MediumCount = m.MediumCount,
            LowCount = m.LowCount,
            LastUpdated = m.LastUpdated,
        };
    }

    public async Task<IReadOnlyList<SlaPolicyDto>> GetSlaPoliciesAsync(CancellationToken cancellationToken = default)
    {
        var policies = await _repository.GetSlaPoliciesAsync(cancellationToken);
        return policies.Select(p => new SlaPolicyDto
        {
            Level = p.Level,
            Time = p.Time,
            TargetMinutes = p.TargetMinutes,
            Color = p.Color,
            Description = p.Description,
        }).ToList();
    }

    public async Task ResetDefaultDataAsync(CancellationToken cancellationToken = default)
    {
        await _repository.ResetToDefaultAsync(cancellationToken);
    }

    private static PriorityQueueItemDto MapToDto(PriorityQueueItem item, DateTimeOffset now)
    {
        var diff = now - item.CreatedAt;
        var reported = FormatReported(diff);
        var age = FormatAge(diff);

        var priority = !string.IsNullOrWhiteSpace(item.PriorityDisplay)
            ? item.PriorityDisplay
            : $"{item.PriorityLevel} · {item.Confidence}%";

        return new PriorityQueueItemDto
        {
            Id = item.Id,
            Category = item.Category,
            Location = item.Location,
            CrisisEvent = item.CrisisEvent,
            Reported = reported,
            Priority = priority,
            PriorityLevel = item.PriorityLevel,
            Confidence = item.Confidence,
            Verification = item.Verification,
            Assigned = item.Assigned,
            Age = age,
            Status = item.Status,
            Overdue = item.IsOverdue,
            Organisation = item.Organisation,
            Description = item.Description,
            AffectedPeopleCount = item.AffectedPeopleCount,
            HasVulnerableMembers = item.HasVulnerableMembers,
            IsLifeThreatening = item.IsLifeThreatening,
            CreatedAt = item.CreatedAt,
            AssignedAt = item.AssignedAt,
        };
    }

    private static string FormatReported(TimeSpan diff)
    {
        if (diff.TotalMinutes < 1) return "Just now";
        if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} min ago";
        if (diff.TotalHours < 2) return "1 hr ago";
        if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} hrs ago";
        return $"{(int)diff.TotalDays} d ago";
    }

    private static string FormatAge(TimeSpan diff)
    {
        if (diff.TotalMinutes < 1) return "Just now";
        if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} min";
        var hours = (int)diff.TotalHours;
        var minutes = diff.Minutes;
        return minutes > 0 ? $"{hours}h {minutes}m" : $"{hours}h";
    }
}
