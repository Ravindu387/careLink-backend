using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using CareLink.Core.Models;

namespace CareLink.Application.Services;

public class ShelterService : IShelterService
{
    private readonly IShelterRepository _shelterRepository;

    public ShelterService(IShelterRepository shelterRepository)
    {
        _shelterRepository = shelterRepository;
    }

    #region Operational & Coordinator Methods (Safna)

    public async Task<IReadOnlyList<ShelterDto>> GetAllAsync(
        ShelterQueryParameters query,
        CancellationToken cancellationToken = default)
    {
        var filter = new ShelterFilterParams
        {
            Search = query?.Search,
            Area = query?.Area,
            Status = query?.Status,
            Availability = query?.Availability,
            Service = query?.Service
        };

        var list = await _shelterRepository.GetAllAsync(filter, cancellationToken);
        return list.Select(MapToDto).ToList();
    }

    public async Task<ShelterDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var item = await _shelterRepository.GetByIdAsync(id, cancellationToken);
        return item == null ? null : MapToDto(item);
    }

    public async Task<ShelterDto> CreateAsync(CreateShelterDto dto, CancellationToken cancellationToken = default)
    {
        var services = dto.Services ?? new List<string>();
        if (services.Count == 0)
        {
            services.Add("Food");
            services.Add("Medical");
        }

        var id = $"SH-{Random.Shared.Next(100, 999)}";

        var entity = new Shelter
        {
            Id = id,
            Name = dto.Name.Trim(),
            Location = string.IsNullOrWhiteSpace(dto.Location) ? "Central Ward" : dto.Location.Trim(),
            District = dto.District?.Trim() ?? string.Empty,
            Type = dto.Type?.Trim() ?? string.Empty,
            Capacity = dto.Capacity,
            CurrentOccupancy = Math.Clamp(dto.Occupied, 0, dto.Capacity),
            Reserved = 0,
            Manager = dto.Manager?.Trim() ?? dto.ContactPerson?.Trim() ?? string.Empty,
            Status = string.IsNullOrWhiteSpace(dto.Status) ? "Active" : dto.Status.Trim(),
            Services = services.Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList(),
            ContactPhone = dto.ContactPhone?.Trim() ?? string.Empty,
            Notes = dto.Notes?.Trim()
        };

        var created = await _shelterRepository.AddAsync(entity, cancellationToken);
        return MapToDto(created);
    }

    public async Task<ShelterDto?> UpdateAsync(int id, UpdateShelterDto dto, CancellationToken cancellationToken = default)
    {
        var services = dto.Services ?? new List<string>();

        var existing = await _shelterRepository.GetByIdAsync(id, cancellationToken);
        var shelterId = existing?.Id ?? id.ToString();

        var entity = new Shelter
        {
            Id = shelterId,
            Name = dto.Name.Trim(),
            Location = dto.Location.Trim(),
            Capacity = dto.Capacity,
            CurrentOccupancy = Math.Clamp(dto.Occupied, 0, dto.Capacity),
            Services = services.Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList(),
            ContactPhone = dto.ContactPhone?.Trim() ?? string.Empty,
            Notes = dto.Notes?.Trim(),
            Manager = dto.ContactPerson?.Trim() ?? existing?.Manager ?? string.Empty,
            Status = existing?.Status ?? "Active",
            District = existing?.District ?? string.Empty,
            Type = existing?.Type ?? string.Empty,
        };

        var updated = await _shelterRepository.UpdateAsync(entity, cancellationToken);
        return updated == null ? null : MapToDto(updated);
    }

    public async Task<ShelterDto?> UpdateOccupancyAsync(int id, int occupied, CancellationToken cancellationToken = default)
    {
        var updated = await _shelterRepository.UpdateOccupancyAsync(id, occupied, cancellationToken);
        return updated == null ? null : MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _shelterRepository.DeleteAsync(id, cancellationToken);
    }

    public async Task<ShelterMetricsDto> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        var m = await _shelterRepository.GetMetricsAsync(cancellationToken);
        return new ShelterMetricsDto
        {
            OperationalShelters = m.OperationalShelters,
            TotalCapacity = m.TotalCapacity,
            TotalOccupied = m.TotalOccupied,
            AvailableSpaces = m.AvailableSpaces,
            AcceptingArrivals = m.AcceptingArrivals,
            NearCapacityCount = m.NearCapacityCount,
            FullCount = m.FullCount,
            TotalShelters = m.TotalShelters,
            LastUpdated = m.LastUpdated
        };
    }

    public async Task ResetToDefaultAsync(CancellationToken cancellationToken = default)
    {
        await _shelterRepository.ResetToDefaultAsync(cancellationToken);
    }

    #endregion

    #region Shelter Management Methods (Prabhani)

    public async Task<IEnumerable<ShelterDto>> GetAllSheltersAsync(CancellationToken cancellationToken = default)
    {
        var shelters = await _shelterRepository.GetAllAsync(cancellationToken);
        return shelters.Select(MapToDto);
    }

    public async Task<ShelterDto?> GetShelterByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var s = await _shelterRepository.GetByIdAsync(id, cancellationToken);
        if (s == null) return null;
        return MapToDto(s);
    }

    public async Task<PagedResultDto<ShelterDto>> GetSheltersPagedAsync(ShelterQueryParameters parameters, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _shelterRepository.GetPagedAsync(parameters, cancellationToken);

        var page = parameters.Page.GetValueOrDefault(1);
        if (page < 1) page = 1;
        var pageSize = parameters.PageSize.GetValueOrDefault(10);
        if (pageSize < 1) pageSize = 10;

        return new PagedResultDto<ShelterDto>
        {
            Items = items.Select(MapToDto),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ShelterSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var (registered, operational, nearCapacity, maintenance) = await _shelterRepository.GetSummaryMetricsAsync(cancellationToken);

        return new ShelterSummaryDto
        {
            Registered = registered,
            Operational = operational,
            NearCapacity = nearCapacity,
            Maintenance = maintenance
        };
    }

    public async Task<ShelterDto> CreateShelterAsync(CreateShelterDto dto, CancellationToken cancellationToken = default)
    {
        var id = $"SH-{Random.Shared.Next(100, 999)}";

        var shelter = new Shelter
        {
            Id = id,
            Name = dto.Name.Trim(),
            District = dto.District?.Trim() ?? string.Empty,
            Type = dto.Type?.Trim() ?? string.Empty,
            Capacity = dto.Capacity,
            CurrentOccupancy = 0,
            Reserved = 0,
            Manager = string.IsNullOrWhiteSpace(dto.Manager) ? "Not assigned" : dto.Manager.Trim(),
            Location = string.IsNullOrWhiteSpace(dto.Location) ? $"{dto.District?.Trim()}, Sri Lanka" : dto.Location.Trim(),
            Status = string.IsNullOrWhiteSpace(dto.Status) ? "Active" : dto.Status.Trim(),
            Services = dto.Services ?? new List<string>()
        };

        var created = await _shelterRepository.AddAsync(shelter, cancellationToken);
        return MapToDto(created);
    }

    #endregion

    private static ShelterDto MapToDto(Shelter s)
    {
        return new ShelterDto
        {
            Id = s.Id,
            Name = s.Name,
            Location = s.Location,
            District = s.District,
            Type = s.Type,
            Manager = s.Manager,
            Status = s.Status,
            Capacity = s.Capacity,
            CurrentOccupancy = s.CurrentOccupancy,
            Occupied = s.CurrentOccupancy,
            Reserved = s.Reserved,
            Remaining = s.Remaining,
            Services = new List<string>(s.Services),
            ContactPerson = s.ContactPerson,
            ContactPhone = s.ContactPhone,
            Notes = s.Notes,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt
        };
    }
}
