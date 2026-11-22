using System.Globalization;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using CareLink.Core.Models;

namespace CareLink.Application.Services;

public class AccommodationRequestService : IAccommodationRequestService
{
    private readonly IAccommodationRequestRepository _repository;
    private readonly CareLink.Core.Interfaces.IShelterRepository _shelterRepository;

    public AccommodationRequestService(
        IAccommodationRequestRepository repository,
        CareLink.Core.Interfaces.IShelterRepository shelterRepository)
    {
        _repository = repository;
        _shelterRepository = shelterRepository;
    }

    public async Task<PagedResultDto<AccommodationRequestDto>> GetPagedRequestsAsync(
        AccommodationRequestQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _repository.GetPagedAsync(parameters, cancellationToken);

        return new PagedResultDto<AccommodationRequestDto>
        {
            Items = items.Select(MapToDto),
            TotalCount = totalCount,
            Page = parameters.Page,
            PageSize = parameters.PageSize
        };
    }

    public async Task<IEnumerable<AccommodationRequestDto>> GetAllRequestsAsync(CancellationToken cancellationToken = default)
    {
        var items = await _repository.GetAllAsync(cancellationToken);
        return items.Select(MapToDto);
    }

    public async Task<AccommodationRequestDto?> GetRequestByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken);
        return item != null ? MapToDto(item) : null;
    }

    public async Task<AccommodationRequestCountsDto> GetStatusCountsAsync(CancellationToken cancellationToken = default)
    {
        var rawCounts = await _repository.GetStatusCountsAsync(cancellationToken);

        var total = rawCounts.Values.Sum();
        rawCounts.TryGetValue("Pending", out var pending);
        rawCounts.TryGetValue("Under Review", out var underReview);
        rawCounts.TryGetValue("Approved", out var approved);
        rawCounts.TryGetValue("Assigned", out var assigned);

        return new AccommodationRequestCountsDto
        {
            AllRequests = total,
            Pending = pending,
            UnderReview = underReview,
            Approved = approved,
            Assigned = assigned
        };
    }

    public async Task<AccommodationRequestDto> CreateRequestAsync(
        CreateAccommodationRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        // Resolve shelter
        string shelterId = dto.RequestedShelter;
        string shelterName = dto.RequestedShelter;

        var allShelters = await _shelterRepository.GetAllAsync(cancellationToken);
        var matchedShelter = allShelters.FirstOrDefault(s =>
            s.Id.Equals(dto.RequestedShelter, StringComparison.OrdinalIgnoreCase) ||
            s.Name.Equals(dto.RequestedShelter, StringComparison.OrdinalIgnoreCase));

        if (matchedShelter != null)
        {
            shelterId = matchedShelter.Id;
            shelterName = matchedShelter.Name;
        }

        // Generate ID like AR-2841
        var randomNum = Random.Shared.Next(1000, 9999);
        var id = $"AR-{randomNum}";

        var entity = new AccommodationRequest
        {
            Id = id,
            Applicant = dto.Applicant.Trim(),
            FamilyMembers = dto.FamilyMembers,
            CurrentLocation = dto.CurrentLocation.Trim(),
            ShelterId = shelterId,
            RequestedShelter = shelterName,
            Priority = string.IsNullOrWhiteSpace(dto.Priority) ? "Medium" : dto.Priority.Trim(),
            Status = "Pending",
            RequestDate = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(entity, cancellationToken);
        return MapToDto(created);
    }

    public async Task<AccommodationRequestDto?> UpdateStatusAsync(
        string id,
        UpdateAccommodationRequestStatusDto dto,
        CancellationToken cancellationToken = default)
    {
        var success = await _repository.UpdateStatusAsync(id, dto.Status.Trim(), cancellationToken);
        if (!success) return null;

        var updated = await _repository.GetByIdAsync(id, cancellationToken);
        return updated != null ? MapToDto(updated) : null;
    }

    public async Task<bool> DeleteRequestAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _repository.DeleteAsync(id, cancellationToken);
    }

    private static AccommodationRequestDto MapToDto(AccommodationRequest entity)
    {
        return new AccommodationRequestDto
        {
            Id = entity.Id,
            Applicant = entity.Applicant,
            FamilyMembers = entity.FamilyMembers,
            CurrentLocation = entity.CurrentLocation,
            RequestedShelter = entity.RequestedShelter,
            ShelterId = entity.ShelterId,
            Priority = entity.Priority,
            Status = entity.Status,
            RequestDate = entity.RequestDate.ToString("dd MMM · HH:mm", CultureInfo.InvariantCulture),
            RequestDateUtc = entity.RequestDate
        };
    }
}
