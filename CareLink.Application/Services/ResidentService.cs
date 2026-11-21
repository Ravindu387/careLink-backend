using System.Globalization;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using CareLink.Core.Models;

namespace CareLink.Application.Services;

public class ResidentService : IResidentService
{
    private readonly IResidentRepository _residentRepository;
    private readonly IShelterRepository _shelterRepository;

    public ResidentService(
        IResidentRepository residentRepository,
        IShelterRepository shelterRepository)
    {
        _residentRepository = residentRepository;
        _shelterRepository = shelterRepository;
    }

    public async Task<PagedResultDto<ResidentDto>> GetPagedResidentsAsync(
        ResidentQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _residentRepository.GetPagedAsync(parameters, cancellationToken);

        return new PagedResultDto<ResidentDto>
        {
            Items = items.Select(MapToDto),
            TotalCount = totalCount,
            Page = parameters.Page,
            PageSize = parameters.PageSize
        };
    }

    public async Task<IEnumerable<ResidentDto>> GetAllResidentsAsync(CancellationToken cancellationToken = default)
    {
        var items = await _residentRepository.GetAllAsync(cancellationToken);
        return items.Select(MapToDto);
    }

    public async Task<ResidentDto?> GetResidentByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var resident = await _residentRepository.GetByIdAsync(id, cancellationToken);
        return resident != null ? MapToDto(resident) : null;
    }

    public async Task<ResidentSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var metrics = await _residentRepository.GetSummaryMetricsAsync(cancellationToken);

        return new ResidentSummaryDto
        {
            TotalRegistered = metrics.TotalRegistered,
            TotalShelters = metrics.TotalShelters,
            FamilyGroupsCount = metrics.FamilyGroupsCount,
            SingleResidentsCount = metrics.SingleResidentsCount,
            AssistanceNeedsCount = metrics.AssistanceNeedsCount,
            CheckedOutTodayCount = metrics.CheckedOutTodayCount,
            CheckedInTodayCount = metrics.CheckedInTodayCount
        };
    }

    public async Task<ResidentDto> CreateResidentAsync(
        CreateResidentDto dto,
        CancellationToken cancellationToken = default)
    {
        string shelterId = dto.ShelterId;
        string shelterName = dto.ShelterId;

        var allShelters = await _shelterRepository.GetAllAsync(cancellationToken);
        var matchedShelter = allShelters.FirstOrDefault(s =>
            s.Id.Equals(dto.ShelterId, StringComparison.OrdinalIgnoreCase) ||
            s.Name.Equals(dto.ShelterId, StringComparison.OrdinalIgnoreCase));

        if (matchedShelter != null)
        {
            shelterId = matchedShelter.Id;
            shelterName = matchedShelter.Name;
        }

        var randomNum = Random.Shared.Next(10000, 99999);
        var id = $"RES-{randomNum}";

        var assistance = string.IsNullOrWhiteSpace(dto.AssistanceNeeds) ? "None" : dto.AssistanceNeeds.Trim();
        var status = assistance != "None" ? "Assistance" : "Active";

        var entity = new Resident
        {
            Id = id,
            Name = dto.Name.Trim(),
            Age = dto.Age,
            Gender = dto.Gender.Trim(),
            Contact = dto.Contact.Trim(),
            FamilyGroup = string.IsNullOrWhiteSpace(dto.FamilyGroup) ? "Individual" : dto.FamilyGroup.Trim(),
            ShelterId = shelterId,
            ShelterName = shelterName,
            Room = dto.Room.Trim(),
            Bed = dto.Bed.Trim(),
            CheckIn = DateTime.UtcNow,
            Status = status,
            AssistanceNeeds = assistance,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _residentRepository.AddAsync(entity, cancellationToken);
        return MapToDto(created);
    }

    public async Task<ResidentDto?> UpdateResidentAsync(
        string id,
        UpdateResidentDto dto,
        CancellationToken cancellationToken = default)
    {
        var existing = await _residentRepository.GetByIdAsync(id, cancellationToken);
        if (existing == null) return null;

        if (!string.IsNullOrWhiteSpace(dto.Name)) existing.Name = dto.Name.Trim();
        if (dto.Age.HasValue) existing.Age = dto.Age.Value;
        if (!string.IsNullOrWhiteSpace(dto.Gender)) existing.Gender = dto.Gender.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Contact)) existing.Contact = dto.Contact.Trim();
        if (dto.FamilyGroup != null) existing.FamilyGroup = string.IsNullOrWhiteSpace(dto.FamilyGroup) ? "Individual" : dto.FamilyGroup.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Room)) existing.Room = dto.Room.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Bed)) existing.Bed = dto.Bed.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Status)) existing.Status = dto.Status.Trim();
        if (!string.IsNullOrWhiteSpace(dto.AssistanceNeeds)) existing.AssistanceNeeds = dto.AssistanceNeeds.Trim();

        if (!string.IsNullOrWhiteSpace(dto.ShelterId))
        {
            var allShelters = await _shelterRepository.GetAllAsync(cancellationToken);
            var matched = allShelters.FirstOrDefault(s =>
                s.Id.Equals(dto.ShelterId, StringComparison.OrdinalIgnoreCase) ||
                s.Name.Equals(dto.ShelterId, StringComparison.OrdinalIgnoreCase));

            if (matched != null)
            {
                existing.ShelterId = matched.Id;
                existing.ShelterName = matched.Name;
            }
            else
            {
                existing.ShelterId = dto.ShelterId;
            }
        }

        existing.UpdatedAt = DateTime.UtcNow;

        var updated = await _residentRepository.UpdateAsync(existing, cancellationToken);
        return updated != null ? MapToDto(updated) : null;
    }

    public async Task<ResidentDto?> UpdateStatusAsync(
        string id,
        UpdateResidentStatusDto dto,
        CancellationToken cancellationToken = default)
    {
        var success = await _residentRepository.UpdateStatusAsync(id, dto.Status.Trim(), cancellationToken);
        if (!success) return null;

        var resident = await _residentRepository.GetByIdAsync(id, cancellationToken);
        return resident != null ? MapToDto(resident) : null;
    }

    public async Task<ResidentDto?> CheckoutResidentAsync(string id, CancellationToken cancellationToken = default)
    {
        var success = await _residentRepository.CheckoutResidentAsync(id, cancellationToken);
        if (!success) return null;

        var resident = await _residentRepository.GetByIdAsync(id, cancellationToken);
        return resident != null ? MapToDto(resident) : null;
    }

    public async Task<bool> DeleteResidentAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _residentRepository.DeleteAsync(id, cancellationToken);
    }

    private static ResidentDto MapToDto(Resident entity)
    {
        return new ResidentDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Age = entity.Age,
            Gender = entity.Gender,
            Contact = entity.Contact,
            FamilyGroup = entity.FamilyGroup,
            Shelter = entity.ShelterName,
            ShelterId = entity.ShelterId,
            Room = entity.Room,
            Bed = entity.Bed,
            CheckIn = entity.CheckIn.ToString("dd MMM", CultureInfo.InvariantCulture),
            CheckInDate = entity.CheckIn,
            Status = entity.Status,
            AssistanceNeeds = entity.AssistanceNeeds,
            CheckedOutAt = entity.CheckedOutAt
        };
    }
}
