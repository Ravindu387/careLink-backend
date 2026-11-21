using System.Globalization;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using CareLink.Core.Models;

namespace CareLink.Application.Services;

public class HealthSafetyService : IHealthSafetyService
{
    private readonly IHealthSafetyRepository _repository;

    private static readonly List<string> IncidentTypes = new()
    {
        "Medical assistance",
        "Sanitation inspection",
        "Fire safety",
        "Accessibility concern",
        "Evacuation",
        "Water safety",
        "Food safety",
        "Other"
    };

    private static readonly List<string> StaffMembers = new()
    {
        "Dr. M. Rao",
        "Nurse M. Rao",
        "Facilities Team",
        "Maintenance",
        "Security Team",
        "Shelter Manager"
    };

    private static readonly List<string> Severities = new()
    {
        "Critical",
        "High",
        "Medium",
        "Low"
    };

    private static readonly List<string> Statuses = new()
    {
        "Action Required",
        "Under Investigation",
        "Resolved"
    };

    public HealthSafetyService(IHealthSafetyRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<HealthSafetyIncidentDto>> GetIncidentsAsync(
        HealthSafetyIncidentQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var items = await _repository.GetAllAsync(parameters, cancellationToken);
        return items.Select(MapToDto);
    }

    public async Task<HealthSafetyIncidentDto?> GetIncidentByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken);
        return item != null ? MapToDto(item) : null;
    }

    public async Task<HealthSafetySummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var statusCounts = await _repository.GetStatusCountsAsync(cancellationToken);
        var checks = await _repository.GetSafetyChecksAsync(cancellationToken);

        statusCounts.TryGetValue("Action Required", out var actionRequired);
        statusCounts.TryGetValue("Under Investigation", out var underInvestigation);
        statusCounts.TryGetValue("Resolved", out var resolved);

        var total = statusCounts.Values.Sum();

        return new HealthSafetySummaryDto
        {
            ActionRequired = actionRequired,
            UnderInvestigation = underInvestigation,
            Resolved = resolved,
            Total = total,
            SafetyChecks = checks.Select(c => new SafetyCheckDto
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                Color = c.Color,
                IconKey = c.IconKey
            })
        };
    }

    public Task<HealthSafetyMetaDto> GetMetaAsync(CancellationToken cancellationToken = default)
    {
        var meta = new HealthSafetyMetaDto
        {
            IncidentTypes = IncidentTypes,
            StaffMembers = StaffMembers,
            Severities = Severities,
            Statuses = Statuses
        };

        return Task.FromResult(meta);
    }

    public async Task<IEnumerable<SafetyCheckDto>> GetSafetyChecksAsync(CancellationToken cancellationToken = default)
    {
        var checks = await _repository.GetSafetyChecksAsync(cancellationToken);
        return checks.Select(c => new SafetyCheckDto
        {
            Id = c.Id,
            Title = c.Title,
            Description = c.Description,
            Color = c.Color,
            IconKey = c.IconKey
        });
    }

    public async Task<HealthSafetyIncidentDto> CreateIncidentAsync(
        CreateHealthSafetyIncidentDto dto,
        CancellationToken cancellationToken = default)
    {
        var all = await _repository.GetAllAsync(new HealthSafetyIncidentQueryParameters { PageSize = 1000 }, cancellationToken);
        var maxId = 190;
        foreach (var item in all)
        {
            if (item.Id.StartsWith("HI-") && int.TryParse(item.Id.Substring(3), out var n))
            {
                if (n > maxId) maxId = n;
            }
        }

        var entity = new HealthSafetyIncident
        {
            Id = $"HI-{maxId + 1}",
            Type = string.IsNullOrWhiteSpace(dto.Type) ? "Medical assistance" : dto.Type.Trim(),
            Location = dto.Location.Trim(),
            ReportedBy = dto.ReportedBy.Trim(),
            Severity = string.IsNullOrWhiteSpace(dto.Severity) ? "Medium" : dto.Severity.Trim(),
            AssignedStaff = string.IsNullOrWhiteSpace(dto.AssignedStaff) ? "Nurse M. Rao" : dto.AssignedStaff.Trim(),
            Status = "Action Required",
            Description = dto.Description.Trim(),
            IncidentDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(entity, cancellationToken);
        return MapToDto(created);
    }

    public async Task<HealthSafetyIncidentDto?> UpdateIncidentAsync(
        string id,
        UpdateHealthSafetyIncidentDto dto,
        CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing == null) return null;

        if (!string.IsNullOrWhiteSpace(dto.AssignedStaff))
            existing.AssignedStaff = dto.AssignedStaff.Trim();

        if (!string.IsNullOrWhiteSpace(dto.Severity))
            existing.Severity = dto.Severity.Trim();

        if (!string.IsNullOrWhiteSpace(dto.Status))
            existing.Status = dto.Status.Trim();

        if (!string.IsNullOrWhiteSpace(dto.Description))
            existing.Description = dto.Description.Trim();

        if (!string.IsNullOrWhiteSpace(dto.Location))
            existing.Location = dto.Location.Trim();

        existing.UpdatedAt = DateTime.UtcNow;

        var success = await _repository.UpdateAsync(existing, cancellationToken);
        if (!success) return null;

        var updated = await _repository.GetByIdAsync(id, cancellationToken);
        return updated != null ? MapToDto(updated) : null;
    }

    public async Task<bool> DeleteIncidentAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _repository.DeleteAsync(id, cancellationToken);
    }

    private static HealthSafetyIncidentDto MapToDto(HealthSafetyIncident entity)
    {
        return new HealthSafetyIncidentDto
        {
            Id = entity.Id,
            Type = entity.Type,
            Location = entity.Location,
            ReportedBy = entity.ReportedBy,
            Date = entity.IncidentDate.ToString("dd MMM", CultureInfo.InvariantCulture),
            Time = entity.IncidentDate.ToString("HH:mm", CultureInfo.InvariantCulture),
            Severity = entity.Severity,
            AssignedStaff = entity.AssignedStaff,
            Status = entity.Status,
            Description = entity.Description,
            IncidentDateUtc = entity.IncidentDate
        };
    }
}
