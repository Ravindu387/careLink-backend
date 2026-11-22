using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;

namespace CareLink.Application.Services;

public class ShelterService : IShelterService
{
    private readonly IShelterRepository _repository;

    public ShelterService(IShelterRepository repository)
    {
        _repository = repository;
    }

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

        var list = await _repository.GetAllAsync(filter, cancellationToken);
        return list.Select(MapToDto).ToList();
    }

    public async Task<ShelterDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken);
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

        var entity = new Shelter
        {
            Name = dto.Name.Trim(),
            Location = dto.Location.Trim(),
            Capacity = dto.Capacity,
            Occupied = Math.Clamp(dto.Occupied, 0, dto.Capacity),
            Services = services.Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList(),
            ContactPerson = dto.ContactPerson?.Trim() ?? string.Empty,
            ContactPhone = dto.ContactPhone?.Trim() ?? string.Empty,
            Notes = dto.Notes?.Trim()
        };

        var created = await _repository.AddAsync(entity, cancellationToken);
        return MapToDto(created);
    }

    public async Task<ShelterDto?> UpdateAsync(int id, UpdateShelterDto dto, CancellationToken cancellationToken = default)
    {
        var services = dto.Services ?? new List<string>();

        var entity = new Shelter
        {
            Id = id,
            Name = dto.Name.Trim(),
            Location = dto.Location.Trim(),
            Capacity = dto.Capacity,
            Occupied = Math.Clamp(dto.Occupied, 0, dto.Capacity),
            Services = services.Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList(),
            ContactPerson = dto.ContactPerson?.Trim() ?? string.Empty,
            ContactPhone = dto.ContactPhone?.Trim() ?? string.Empty,
            Notes = dto.Notes?.Trim()
        };

        var updated = await _repository.UpdateAsync(entity, cancellationToken);
        return updated == null ? null : MapToDto(updated);
    }

    public async Task<ShelterDto?> UpdateOccupancyAsync(int id, int occupied, CancellationToken cancellationToken = default)
    {
        var updated = await _repository.UpdateOccupancyAsync(id, occupied, cancellationToken);
        return updated == null ? null : MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _repository.DeleteAsync(id, cancellationToken);
    }

    public async Task<ShelterMetricsDto> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        var m = await _repository.GetMetricsAsync(cancellationToken);
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
        await _repository.ResetToDefaultAsync(cancellationToken);
    }

    private static ShelterDto MapToDto(Shelter s)
    {
        return new ShelterDto
        {
            Id = s.Id,
            Name = s.Name,
            Location = s.Location,
            Capacity = s.Capacity,
            Occupied = s.Occupied,
            Remaining = s.Remaining,
            Services = new List<string>(s.Services),
            Status = s.Status,
            ContactPerson = s.ContactPerson,
            ContactPhone = s.ContactPhone,
            Notes = s.Notes,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt
        };
    }
}
