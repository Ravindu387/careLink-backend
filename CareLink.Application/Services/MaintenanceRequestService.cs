using System.Globalization;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using CareLink.Core.Models;

namespace CareLink.Application.Services;

public class MaintenanceRequestService : IMaintenanceRequestService
{
    private readonly IMaintenanceRequestRepository _repository;

    private static readonly List<string> Categories = new()
    {
        "Plumbing",
        "Electrical",
        "Structural",
        "HVAC",
        "Cleaning",
        "Equipment",
        "Other"
    };

    private static readonly List<string> Technicians = new()
    {
        "N. Kumara",
        "D. Silva",
        "M. Perera",
        "Facilities Team",
        "Maintenance Team",
        "Unassigned"
    };

    private static readonly List<string> Shelters = new()
    {
        "St. Mary Civic",
        "Riverton Central",
        "North Community",
        "Lakeside Sports",
        "Hillview College"
    };

    private static readonly List<string> Priorities = new()
    {
        "Critical",
        "High",
        "Medium",
        "Low"
    };

    private static readonly List<string> Statuses = new()
    {
        "In Progress",
        "Assigned",
        "Completed",
        "Pending",
        "On Hold"
    };

    public MaintenanceRequestService(IMaintenanceRequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResultDto<MaintenanceRequestDto>> GetPagedRequestsAsync(
        MaintenanceRequestQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _repository.GetPagedAsync(parameters, cancellationToken);

        return new PagedResultDto<MaintenanceRequestDto>
        {
            Items = items.Select(MapToDto),
            TotalCount = totalCount,
            Page = parameters.Page,
            PageSize = parameters.PageSize
        };
    }

    public async Task<IEnumerable<MaintenanceRequestDto>> GetAllRequestsAsync(CancellationToken cancellationToken = default)
    {
        var items = await _repository.GetAllAsync(cancellationToken);
        return items.Select(MapToDto);
    }

    public async Task<MaintenanceRequestDto?> GetRequestByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken);
        return item != null ? MapToDto(item) : null;
    }

    public async Task<MaintenanceRequestSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var all = await _repository.GetAllAsync(cancellationToken);
        var allList = all.ToList();

        var critical = allList.Count(r =>
            r.Priority.Equals("Critical", StringComparison.OrdinalIgnoreCase) &&
            !r.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase));

        var inProgress = allList.Count(r => r.Status.Equals("In Progress", StringComparison.OrdinalIgnoreCase));
        var completed = allList.Count(r => r.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase));
        var pending = allList.Count(r => r.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase));
        var onHold = allList.Count(r => r.Status.Equals("On Hold", StringComparison.OrdinalIgnoreCase));

        return new MaintenanceRequestSummaryDto
        {
            CriticalCount = critical,
            InProgressCount = inProgress,
            CompletedCount = completed,
            PendingCount = pending,
            OnHoldCount = onHold,
            TotalCount = allList.Count
        };
    }

    public Task<MaintenanceRequestMetaDto> GetMetaAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new MaintenanceRequestMetaDto
        {
            Categories = Categories,
            Technicians = Technicians,
            Shelters = Shelters,
            Priorities = Priorities,
            Statuses = Statuses
        });
    }

    public async Task<MaintenanceRequestDto> CreateRequestAsync(
        CreateMaintenanceRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        var all = await _repository.GetAllAsync(cancellationToken);
        var maxId = 320;
        foreach (var item in all)
        {
            if (item.Id.StartsWith("MR-") && int.TryParse(item.Id.Substring(3), out var n))
            {
                if (n > maxId) maxId = n;
            }
        }

        var entity = new MaintenanceRequest
        {
            Id = $"MR-{maxId + 1}",
            Issue = dto.Issue.Trim(),
            Category = string.IsNullOrWhiteSpace(dto.Category) ? "Plumbing" : dto.Category.Trim(),
            Shelter = string.IsNullOrWhiteSpace(dto.Shelter) ? "Riverton Central" : dto.Shelter.Trim(),
            Room = dto.Room.Trim(),
            Priority = string.IsNullOrWhiteSpace(dto.Priority) ? "Medium" : dto.Priority.Trim(),
            Technician = string.IsNullOrWhiteSpace(dto.Technician) ? "Unassigned" : dto.Technician.Trim(),
            Status = "Pending",
            Description = dto.Description.Trim(),
            ReportedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(entity, cancellationToken);
        return MapToDto(created);
    }

    public async Task<MaintenanceRequestDto?> UpdateRequestAsync(
        string id,
        UpdateMaintenanceRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing == null) return null;

        if (!string.IsNullOrWhiteSpace(dto.Issue))
            existing.Issue = dto.Issue.Trim();

        if (!string.IsNullOrWhiteSpace(dto.Category))
            existing.Category = dto.Category.Trim();

        if (!string.IsNullOrWhiteSpace(dto.Shelter))
            existing.Shelter = dto.Shelter.Trim();

        if (!string.IsNullOrWhiteSpace(dto.Room))
            existing.Room = dto.Room.Trim();

        if (!string.IsNullOrWhiteSpace(dto.Priority))
            existing.Priority = dto.Priority.Trim();

        if (!string.IsNullOrWhiteSpace(dto.Technician))
            existing.Technician = dto.Technician.Trim();

        if (!string.IsNullOrWhiteSpace(dto.Status))
            existing.Status = dto.Status.Trim();

        if (!string.IsNullOrWhiteSpace(dto.Description))
            existing.Description = dto.Description.Trim();

        existing.UpdatedAt = DateTime.UtcNow;

        var success = await _repository.UpdateAsync(existing, cancellationToken);
        if (!success) return null;

        var updated = await _repository.GetByIdAsync(id, cancellationToken);
        return updated != null ? MapToDto(updated) : null;
    }

    public async Task<bool> DeleteRequestAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _repository.DeleteAsync(id, cancellationToken);
    }

    private static MaintenanceRequestDto MapToDto(MaintenanceRequest entity)
    {
        return new MaintenanceRequestDto
        {
            Id = entity.Id,
            Issue = entity.Issue,
            Category = entity.Category,
            Shelter = entity.Shelter,
            Room = entity.Room,
            Reported = entity.ReportedAt.ToString("dd MMM · HH:mm", CultureInfo.InvariantCulture),
            ReportedAtUtc = entity.ReportedAt,
            Priority = entity.Priority,
            Technician = entity.Technician,
            Status = entity.Status,
            Description = entity.Description
        };
    }
}
