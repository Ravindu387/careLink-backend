using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;

namespace CareLink.Application.Services;

public class CrisisEventService : ICrisisEventService
{
    private readonly ICrisisEventRepository _repository;
    private static int _crisisCounter = 20;
    private static readonly object _counterLock = new();

    public CrisisEventService(ICrisisEventRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<CrisisEventResponseDto>> GetAllAsync(
        CrisisEventQueryParameters query,
        CancellationToken cancellationToken = default)
    {
        var filter = new CrisisEventFilterParams
        {
            Search = query.Search,
            Type = query.Type,
            Severity = query.Severity,
            Status = query.Status,
            DateRange = query.DateRange,
            ShowArchived = query.ShowArchived
        };

        var list = await _repository.GetAllAsync(filter, cancellationToken);
        return list.Select(MapToDto).ToList();
    }

    public async Task<CrisisEventResponseDto?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken);
        return item == null ? null : MapToDto(item);
    }

    public async Task<CrisisEventResponseDto> CreateAsync(
        CreateCrisisEventDto dto,
        CancellationToken cancellationToken = default)
    {
        string id;
        lock (_counterLock)
        {
            _crisisCounter++;
            id = $"CR-2025-{_crisisCounter:D3}";
        }

        DateTimeOffset startDate;
        if (!string.IsNullOrWhiteSpace(dto.StartDate) && DateTimeOffset.TryParse(dto.StartDate, out var parsed))
        {
            startDate = parsed;
        }
        else
        {
            startDate = DateTimeOffset.UtcNow;
        }

        var severity = string.IsNullOrWhiteSpace(dto.Severity) ? "Moderate · Level 2" : dto.Severity.Trim();
        var severityLevel = ExtractSeverityLevel(severity);

        var entity = new CrisisEvent
        {
            Id = id,
            Name = dto.Name.Trim(),
            Type = dto.Type.Trim(),
            Severity = severity,
            SeverityLevel = severityLevel,
            Area = dto.Area.Trim(),
            StartDate = startDate,
            Status = string.IsNullOrWhiteSpace(dto.Status) ? "Active" : dto.Status.Trim(),
            RequestCount = 0,
            Coordinator = dto.Coordinator.Trim(),
            Description = dto.Description,
            AffectedPeopleCount = dto.AffectedPeopleCount ?? 5000,
            DistrictsCovered = dto.DistrictsCovered ?? 1,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var created = await _repository.AddAsync(entity, cancellationToken);
        return MapToDto(created);
    }

    public async Task<CrisisEventResponseDto?> UpdateAsync(
        string id,
        UpdateCrisisEventDto dto,
        CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing == null) return null;

        if (!string.IsNullOrWhiteSpace(dto.Name)) existing.Name = dto.Name.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Type)) existing.Type = dto.Type.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Severity))
        {
            existing.Severity = dto.Severity.Trim();
            existing.SeverityLevel = ExtractSeverityLevel(existing.Severity);
        }
        if (!string.IsNullOrWhiteSpace(dto.Area)) existing.Area = dto.Area.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Coordinator)) existing.Coordinator = dto.Coordinator.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Status)) existing.Status = dto.Status.Trim();
        if (dto.Description != null) existing.Description = dto.Description;
        if (dto.AffectedPeopleCount.HasValue) existing.AffectedPeopleCount = dto.AffectedPeopleCount.Value;
        if (dto.DistrictsCovered.HasValue) existing.DistrictsCovered = dto.DistrictsCovered.Value;
        if (!string.IsNullOrWhiteSpace(dto.StartDate) && DateTimeOffset.TryParse(dto.StartDate, out var parsed))
        {
            existing.StartDate = parsed;
        }

        existing.UpdatedAt = DateTimeOffset.UtcNow;

        var updated = await _repository.UpdateAsync(existing, cancellationToken);
        return updated == null ? null : MapToDto(updated);
    }

    public async Task<CrisisEventResponseDto?> UpdateStatusAsync(
        string id,
        UpdateCrisisStatusDto dto,
        CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing == null) return null;

        existing.Status = dto.Status.Trim();
        existing.UpdatedAt = DateTimeOffset.UtcNow;

        var updated = await _repository.UpdateAsync(existing, cancellationToken);
        return updated == null ? null : MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _repository.DeleteAsync(id, cancellationToken);
    }

    public async Task<CrisisEventMetricsDto> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        var m = await _repository.GetMetricsAsync(cancellationToken);
        return new CrisisEventMetricsDto
        {
            ActiveEvents = m.ActiveEvents,
            MajorSeverityEvents = m.MajorSeverityEvents,
            TotalAffectedPeople = m.TotalAffectedPeople,
            FormattedAffectedPeople = m.FormattedAffectedPeople,
            AssignedCoordinators = m.AssignedCoordinators,
            TotalEvents = m.TotalEvents,
            MonitoringEvents = m.MonitoringEvents,
            ResolvedEvents = m.ResolvedEvents,
            DistrictsCount = m.DistrictsCount
        };
    }

    public async Task ResetDataAsync(CancellationToken cancellationToken = default)
    {
        await _repository.ResetToDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SharedCrisisEventDto>> GetSharedEventsAsync(
        SharedCrisisQueryParameters query,
        CancellationToken cancellationToken = default)
    {
        var all = await _repository.GetAllAsync(null, cancellationToken);
        var q = all.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(e =>
                e.Id.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                e.Name.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                e.Area.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                e.Sectors.Contains(s, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.Tab) && !query.Tab.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            q = query.Tab switch
            {
                "Active" => q.Where(e => e.Status.Equals("Active", StringComparison.OrdinalIgnoreCase)),
                "Needs action" => q.Where(e => e.Status.Equals("Monitoring", StringComparison.OrdinalIgnoreCase)),
                "Completed" => q.Where(e => e.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase) || e.Status.Equals("Resolved", StringComparison.OrdinalIgnoreCase)),
                "Archived" => q.Where(e => e.Status.Equals("Archived", StringComparison.OrdinalIgnoreCase)),
                _ => q
            };
        }

        if (!string.IsNullOrWhiteSpace(query.Type) && !query.Type.Equals("All event types", StringComparison.OrdinalIgnoreCase))
        {
            q = q.Where(e => e.Type.Equals(query.Type, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.Severity) && !query.Severity.Equals("All severity levels", StringComparison.OrdinalIgnoreCase))
        {
            q = q.Where(e => NormalizeSeverity(e.Severity).Equals(query.Severity, StringComparison.OrdinalIgnoreCase) ||
                             e.Severity.Contains(query.Severity, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.Area) && !query.Area.Equals("All operational areas", StringComparison.OrdinalIgnoreCase))
        {
            q = q.Where(e => e.Area.Equals(query.Area, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.Status) && !query.Status.Equals("All statuses", StringComparison.OrdinalIgnoreCase))
        {
            q = q.Where(e => e.Status.Equals(query.Status, StringComparison.OrdinalIgnoreCase) ||
                             (query.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase) && e.Status.Equals("Resolved", StringComparison.OrdinalIgnoreCase)));
        }

        return q.OrderByDescending(e => e.StartDate)
                .Select(MapToSharedDto)
                .ToList();
    }

    public async Task<SharedCrisisMetricsDto> GetSharedMetricsAsync(CancellationToken cancellationToken = default)
    {
        var all = await _repository.GetAllAsync(null, cancellationToken);
        var active = all.Count(e => e.Status.Equals("Active", StringComparison.OrdinalIgnoreCase));
        var monitoring = all.Count(e => e.Status.Equals("Monitoring", StringComparison.OrdinalIgnoreCase));
        var completed = all.Count(e => e.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase) || e.Status.Equals("Resolved", StringComparison.OrdinalIgnoreCase));
        var archived = all.Count(e => e.Status.Equals("Archived", StringComparison.OrdinalIgnoreCase));
        var major = all.Count(e => e.SeverityLevel >= 4 || e.Severity.Contains("Level 4", StringComparison.OrdinalIgnoreCase));

        return new SharedCrisisMetricsDto
        {
            SharedEvents = active + monitoring > 0 ? active + monitoring : all.Count,
            SharedEventsSubtitle = $"{active} currently active",
            MajorResponse = major > 0 ? major : 1,
            MajorResponseSubtitle = "Level 4 operation",
            DistrictsCovered = 6,
            DistrictsCoveredSubtitle = "4 active now",
            CoordinationSectors = 4,
            CoordinationSectorsSubtitle = "All leads assigned",
            TotalCount = all.Count,
            NeedsActionCount = monitoring,
            ActiveCount = active,
            CompletedCount = completed,
            ArchivedCount = archived,
            LastUpdated = DateTimeOffset.UtcNow
        };
    }

    public async Task<CrisisAccessRequestResponseDto> CreateAccessRequestAsync(
        CreateCrisisAccessRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        var entity = new CrisisAccessRequest
        {
            Id = $"REQ-CR-{Random.Shared.Next(1000, 9999)}",
            CrisisId = dto.CrisisId ?? string.Empty,
            EventName = dto.EventName.Trim(),
            PartnerName = string.IsNullOrWhiteSpace(dto.PartnerName) ? "Global Relief Network" : dto.PartnerName.Trim(),
            Reason = dto.Reason.Trim(),
            Status = "Pending",
            SubmittedAt = DateTimeOffset.UtcNow
        };

        var created = await _repository.AddAccessRequestAsync(entity, cancellationToken);
        return new CrisisAccessRequestResponseDto
        {
            Id = created.Id,
            CrisisId = created.CrisisId,
            EventName = created.EventName,
            PartnerName = created.PartnerName,
            Reason = created.Reason,
            Status = created.Status,
            SubmittedAt = created.SubmittedAt
        };
    }

    public async Task<IReadOnlyList<CrisisAccessRequestResponseDto>> GetAccessRequestsAsync(CancellationToken cancellationToken = default)
    {
        var list = await _repository.GetAccessRequestsAsync(cancellationToken);
        return list.Select(r => new CrisisAccessRequestResponseDto
        {
            Id = r.Id,
            CrisisId = r.CrisisId,
            EventName = r.EventName,
            PartnerName = r.PartnerName,
            Reason = r.Reason,
            Status = r.Status,
            SubmittedAt = r.SubmittedAt
        }).ToList();
    }

    private static SharedCrisisEventDto MapToSharedDto(CrisisEvent entity)
    {
        return new SharedCrisisEventDto
        {
            Id = entity.Id,
            Event = entity.Name,
            Type = entity.Type,
            Severity = NormalizeSeverity(entity.Severity),
            Area = entity.Area,
            Date = entity.StartDate.ToString("dd MMM yyyy"),
            Sectors = string.IsNullOrWhiteSpace(entity.Sectors) ? "Food · WASH · Shelter" : entity.Sectors,
            Status = entity.Status.Equals("Resolved", StringComparison.OrdinalIgnoreCase) ? "Completed" : entity.Status,
            Description = entity.Description,
            Coordinator = entity.Coordinator,
            DistrictsCovered = entity.DistrictsCovered,
            AffectedPeopleCount = entity.AffectedPeopleCount
        };
    }

    private static string NormalizeSeverity(string severity)
    {
        if (severity.Contains("Level 4", StringComparison.OrdinalIgnoreCase)) return "Level 4 - Major";
        if (severity.Contains("Level 3", StringComparison.OrdinalIgnoreCase)) return "Level 3 - Severe";
        if (severity.Contains("Level 2", StringComparison.OrdinalIgnoreCase)) return "Level 2 - Moderate";
        if (severity.Contains("Level 1", StringComparison.OrdinalIgnoreCase)) return "Level 1 - Low";
        return severity;
    }

    private static CrisisEventResponseDto MapToDto(CrisisEvent entity)
    {
        return new CrisisEventResponseDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Type = entity.Type,
            Severity = entity.Severity,
            Area = entity.Area,
            StartDate = entity.StartDate.ToString("dd MMM yyyy"),
            Status = entity.Status,
            Requests = entity.RequestCount > 0 ? entity.RequestCount.ToString("N0") : "0",
            Coordinator = entity.Coordinator,
            Description = entity.Description,
            AffectedPeopleCount = entity.AffectedPeopleCount,
            DistrictsCovered = entity.DistrictsCovered,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    private static int ExtractSeverityLevel(string severity)
    {
        if (severity.Contains("Level 4", StringComparison.OrdinalIgnoreCase)) return 4;
        if (severity.Contains("Level 3", StringComparison.OrdinalIgnoreCase)) return 3;
        if (severity.Contains("Level 2", StringComparison.OrdinalIgnoreCase)) return 2;
        if (severity.Contains("Level 1", StringComparison.OrdinalIgnoreCase)) return 1;
        if (severity.StartsWith("Major", StringComparison.OrdinalIgnoreCase)) return 4;
        if (severity.StartsWith("Severe", StringComparison.OrdinalIgnoreCase)) return 3;
        if (severity.StartsWith("Moderate", StringComparison.OrdinalIgnoreCase)) return 2;
        return 1;
    }
}
