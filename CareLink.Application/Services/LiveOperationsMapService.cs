using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;

namespace CareLink.Application.Services;

public class LiveOperationsMapService : ILiveOperationsMapService
{
    private readonly ILiveOperationsMapRepository _repository;

    public LiveOperationsMapService(ILiveOperationsMapRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<OperationPointDto>> GetAllAsync(
        OperationPointQueryParameters query,
        CancellationToken cancellationToken = default)
    {
        var filter = new OperationPointFilterParams
        {
            Search = query.Search,
            District = query.District,
            Layer = query.Layer,
            Priority = query.Priority,
            Status = query.Status
        };

        var entities = await _repository.GetAllAsync(filter, cancellationToken);
        return entities.Select(MapToDto).ToList();
    }

    public async Task<OperationPointDto?> GetByIdAsync(
        string idOrRequestId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(idOrRequestId, cancellationToken);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<OperationPointDto> CreateAsync(
        CreateOperationPointDto dto,
        CancellationToken cancellationToken = default)
    {
        var id = Guid.NewGuid().ToString("N")[..8];
        var prefix = dto.Type switch
        {
            "Shelter" => "SHEL",
            "Warehouse" => "WH",
            "Team" => "TEAM",
            _ => "REQ"
        };
        var randomNum = new Random().Next(100, 999);
        var requestId = $"{prefix}-{randomNum}";

        var entity = new OperationPoint
        {
            Id = id,
            RequestId = requestId,
            Title = dto.Title.Trim(),
            District = dto.District.Trim(),
            Location = dto.Location.Trim(),
            Type = dto.Type,
            Priority = string.IsNullOrWhiteSpace(dto.Priority) ? "Medium" : dto.Priority,
            People = dto.People,
            Confidence = dto.Confidence > 0 ? dto.Confidence : 90,
            NearestShelter = string.IsNullOrWhiteSpace(dto.NearestShelter)
                ? (dto.Type == "Shelter" ? "Current location" : "1.8 km")
                : dto.NearestShelter,
            AssignedTeam = string.IsNullOrWhiteSpace(dto.AssignedTeam)
                ? (dto.Type == "Request" ? "Unassigned" : $"{dto.Type} Unit")
                : dto.AssignedTeam,
            Status = string.IsNullOrWhiteSpace(dto.Status) ? "Active" : dto.Status,
            Time = "Just now",
            X = Math.Clamp(dto.X, 5.0, 95.0),
            Y = Math.Clamp(dto.Y, 5.0, 95.0),
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            ContactName = dto.ContactName,
            ContactPhone = dto.ContactPhone,
            Capacity = dto.Capacity,
            CurrentOccupancy = dto.Type == "Shelter" ? dto.People : null,
            SuppliesStatus = dto.SuppliesStatus,
            Notes = dto.Notes,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = null
        };

        var created = await _repository.AddAsync(entity, cancellationToken);
        return MapToDto(created);
    }

    public async Task<OperationPointDto?> UpdateAsync(
        string id,
        UpdateOperationPointDto dto,
        CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing == null) return null;

        if (!string.IsNullOrWhiteSpace(dto.Title)) existing.Title = dto.Title.Trim();
        if (!string.IsNullOrWhiteSpace(dto.District)) existing.District = dto.District.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Location)) existing.Location = dto.Location.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Type)) existing.Type = dto.Type;
        if (!string.IsNullOrWhiteSpace(dto.Priority)) existing.Priority = dto.Priority;
        if (dto.People.HasValue) existing.People = dto.People.Value;
        if (dto.Confidence.HasValue) existing.Confidence = dto.Confidence.Value;
        if (!string.IsNullOrWhiteSpace(dto.NearestShelter)) existing.NearestShelter = dto.NearestShelter;
        if (!string.IsNullOrWhiteSpace(dto.AssignedTeam)) existing.AssignedTeam = dto.AssignedTeam;
        if (!string.IsNullOrWhiteSpace(dto.Status)) existing.Status = dto.Status;
        if (dto.X.HasValue) existing.X = Math.Clamp(dto.X.Value, 5.0, 95.0);
        if (dto.Y.HasValue) existing.Y = Math.Clamp(dto.Y.Value, 5.0, 95.0);
        if (dto.Latitude.HasValue) existing.Latitude = dto.Latitude.Value;
        if (dto.Longitude.HasValue) existing.Longitude = dto.Longitude.Value;
        if (dto.ContactName != null) existing.ContactName = dto.ContactName;
        if (dto.ContactPhone != null) existing.ContactPhone = dto.ContactPhone;
        if (dto.Capacity.HasValue) existing.Capacity = dto.Capacity.Value;
        if (dto.CurrentOccupancy.HasValue) existing.CurrentOccupancy = dto.CurrentOccupancy.Value;
        if (dto.SuppliesStatus != null) existing.SuppliesStatus = dto.SuppliesStatus;
        if (dto.Notes != null) existing.Notes = dto.Notes;

        existing.UpdatedAt = DateTimeOffset.UtcNow;
        existing.Time = "Updated recently";

        var updated = await _repository.UpdateAsync(existing, cancellationToken);
        return updated == null ? null : MapToDto(updated);
    }

    public async Task<OperationPointDto?> AssignTeamAsync(
        string id,
        AssignOperationPointDto dto,
        CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing == null) return null;

        existing.AssignedTeam = dto.AssignedTeam;
        if (!string.IsNullOrWhiteSpace(dto.Status))
        {
            existing.Status = dto.Status;
        }
        else if (existing.Status == "Active" || existing.AssignedTeam != "Unassigned")
        {
            existing.Status = "Dispatched";
        }

        if (!string.IsNullOrWhiteSpace(dto.Notes))
        {
            existing.Notes = string.IsNullOrWhiteSpace(existing.Notes)
                ? dto.Notes
                : $"{existing.Notes}; {dto.Notes}";
        }

        existing.UpdatedAt = DateTimeOffset.UtcNow;
        existing.Time = "Assigned recently";

        var updated = await _repository.UpdateAsync(existing, cancellationToken);
        return updated == null ? null : MapToDto(updated);
    }

    public async Task<OperationPointDto?> UpdateCoordinatesAsync(
        string id,
        UpdateCoordinatesDto dto,
        CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing == null) return null;

        existing.X = Math.Clamp(dto.X, 2.0, 98.0);
        existing.Y = Math.Clamp(dto.Y, 2.0, 98.0);
        if (dto.Latitude.HasValue) existing.Latitude = dto.Latitude.Value;
        if (dto.Longitude.HasValue) existing.Longitude = dto.Longitude.Value;

        existing.UpdatedAt = DateTimeOffset.UtcNow;
        existing.Time = "Repositioned recently";

        var updated = await _repository.UpdateAsync(existing, cancellationToken);
        return updated == null ? null : MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _repository.DeleteAsync(id, cancellationToken);
    }

    public async Task<LiveOperationsMapMetricsDto> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        var m = await _repository.GetMetricsAsync(cancellationToken);
        return new LiveOperationsMapMetricsDto
        {
            TotalPoints = m.TotalPoints,
            RequestCount = m.RequestCount,
            ShelterCount = m.ShelterCount,
            WarehouseCount = m.WarehouseCount,
            TeamCount = m.TeamCount,
            CriticalCount = m.CriticalCount,
            HighCount = m.HighCount,
            MediumCount = m.MediumCount,
            TotalPeopleAffected = m.TotalPeopleAffected,
            Districts = m.Districts,
            Layers = m.Layers,
            LastSyncedAt = m.LastSyncedAt
        };
    }

    public async Task ResetDefaultDataAsync(CancellationToken cancellationToken = default)
    {
        await _repository.ResetDefaultDataAsync(cancellationToken);
    }

    private static OperationPointDto MapToDto(OperationPoint point)
    {
        return new OperationPointDto
        {
            Id = point.Id,
            RequestId = point.RequestId,
            Title = point.Title,
            District = point.District,
            Location = point.Location,
            Type = point.Type,
            Priority = point.Priority,
            People = point.People,
            Confidence = point.Confidence,
            NearestShelter = point.NearestShelter,
            AssignedTeam = point.AssignedTeam,
            Status = point.Status,
            Time = FormatTime(point),
            X = point.X,
            Y = point.Y,
            Latitude = point.Latitude,
            Longitude = point.Longitude,
            ContactName = point.ContactName,
            ContactPhone = point.ContactPhone,
            Capacity = point.Capacity,
            CurrentOccupancy = point.CurrentOccupancy,
            SuppliesStatus = point.SuppliesStatus,
            Notes = point.Notes,
            CreatedAt = point.CreatedAt,
            UpdatedAt = point.UpdatedAt
        };
    }

    private static string FormatTime(OperationPoint point)
    {
        if (!string.IsNullOrWhiteSpace(point.Time))
        {
            return point.Time;
        }

        var refTime = point.UpdatedAt ?? point.CreatedAt;
        var elapsed = DateTimeOffset.UtcNow - refTime;

        if (elapsed.TotalMinutes < 2) return "Just now";
        if (elapsed.TotalMinutes < 60) return $"{(int)elapsed.TotalMinutes} minutes ago";
        if (elapsed.TotalHours < 24) return $"{(int)elapsed.TotalHours} hours ago";
        return refTime.ToString("yyyy-MM-dd HH:mm");
    }
}
