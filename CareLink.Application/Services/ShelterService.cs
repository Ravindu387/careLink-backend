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
            District = dto.District.Trim(),
            Type = dto.Type.Trim(),
            Capacity = dto.Capacity,
            CurrentOccupancy = 0,
            Reserved = 0,
            Manager = string.IsNullOrWhiteSpace(dto.Manager) ? "Not assigned" : dto.Manager.Trim(),
            Location = string.IsNullOrWhiteSpace(dto.Location) ? $"{dto.District.Trim()}, Sri Lanka" : dto.Location.Trim(),
            Status = string.IsNullOrWhiteSpace(dto.Status) ? "Active" : dto.Status.Trim()
        };

        var created = await _shelterRepository.AddAsync(shelter, cancellationToken);
        return MapToDto(created);
    }

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
            Reserved = s.Reserved
        };
    }
}
