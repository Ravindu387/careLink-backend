using CareLink.Application.Common;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using CareLink.Core.Models;

namespace CareLink.Application.Services;

public class ShelterService : IShelterService
{
    private readonly CareLink.Core.Interfaces.IShelterRepository _shelterRepository;
    private readonly CareLink.Application.Interfaces.IShelterRepository? _citizenShelterRepository;

    public ShelterService(
        CareLink.Core.Interfaces.IShelterRepository shelterRepository,
        CareLink.Application.Interfaces.IShelterRepository? citizenShelterRepository = null)
    {
        _shelterRepository = shelterRepository;
        _citizenShelterRepository = citizenShelterRepository ?? (shelterRepository as CareLink.Application.Interfaces.IShelterRepository);
    }

    private CareLink.Application.Interfaces.IShelterRepository GetCitizenRepo() =>
        _citizenShelterRepository ?? throw new InvalidOperationException("Citizen shelter repository is not available.");

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

    #region Citizen & Community Methods (Oshadi)

    public async Task<ApiResponse<IEnumerable<ShelterDto>>> GetSheltersAsync(
        string? search = null,
        string? area = null,
        double? maxDistance = null,
        string? availability = null,
        bool? accessibleOnly = null,
        string? service = null)
    {
        var entities = await GetCitizenRepo().GetSheltersAsync(search, area, maxDistance, availability, accessibleOnly, service);
        var dtos = entities.Select(MapToDto).ToList();
        return ApiResponse<IEnumerable<ShelterDto>>.SuccessResponse(dtos, "Shelters retrieved successfully.");
    }

    public async Task<ApiResponse<ShelterDto>> GetByIdAsync(string id)
    {
        var entity = await GetCitizenRepo().GetByIdAsync(id);
        if (entity == null)
        {
            return ApiResponse<ShelterDto>.ErrorResponse($"Shelter with ID '{id}' was not found.");
        }
        return ApiResponse<ShelterDto>.SuccessResponse(MapToDto(entity), "Shelter retrieved successfully.");
    }

    public async Task<ApiResponse<ShelterDto>> CreateAsync(CreateShelterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return ApiResponse<ShelterDto>.ErrorResponse("Shelter name is required.");
        }

        var capacity = request.Capacity > 0 ? request.Capacity : 200;
        var availability = request.Availability >= 0 ? request.Availability : capacity;

        var entity = new Shelter
        {
            Name = request.Name.Trim(),
            Area = request.Area.Trim(),
            Address = request.Address.Trim(),
            Distance = request.Distance,
            Capacity = capacity,
            Availability = availability,
            Occupied = capacity - availability,
            Services = request.Services ?? new List<string>(),
            Status = availability > 0 ? "Space available" : "Full",
            Accessible = request.Accessible,
            ContactPhone = !string.IsNullOrWhiteSpace(request.ContactPhone) ? request.ContactPhone.Trim() : "+94 11 234 5678",
            ManagerName = !string.IsNullOrWhiteSpace(request.ManagerName) ? request.ManagerName.Trim() : "Shelter In-Charge",
            OperatingHours = !string.IsNullOrWhiteSpace(request.OperatingHours) ? request.OperatingHours.Trim() : "24/7 during crisis operations",
            IsEvacuationHub = request.IsEvacuationHub,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var created = await GetCitizenRepo().CreateAsync(entity);
        return ApiResponse<ShelterDto>.SuccessResponse(MapToDto(created), "Shelter created successfully.");
    }

    public async Task<ApiResponse<ShelterDto>> UpdateAsync(string id, UpdateShelterRequest request)
    {
        var existing = await GetCitizenRepo().GetByIdAsync(id);
        if (existing == null)
        {
            return ApiResponse<ShelterDto>.ErrorResponse($"Shelter with ID '{id}' was not found.");
        }

        if (!string.IsNullOrWhiteSpace(request.Name)) existing.Name = request.Name.Trim();
        if (!string.IsNullOrWhiteSpace(request.Area)) existing.Area = request.Area.Trim();
        if (!string.IsNullOrWhiteSpace(request.Address)) existing.Address = request.Address.Trim();
        if (request.Distance.HasValue) existing.Distance = request.Distance.Value;
        if (request.Capacity.HasValue) existing.Capacity = request.Capacity.Value;
        if (request.Availability.HasValue)
        {
            existing.Availability = request.Availability.Value;
            existing.Occupied = Math.Max(0, existing.Capacity - existing.Availability);
            existing.Status = existing.Availability > 0 ? "Space available" : "Full";
        }
        if (request.Services != null) existing.Services = request.Services;
        if (request.Accessible.HasValue) existing.Accessible = request.Accessible.Value;
        if (!string.IsNullOrWhiteSpace(request.Status)) existing.Status = request.Status.Trim();
        if (!string.IsNullOrWhiteSpace(request.ContactPhone)) existing.ContactPhone = request.ContactPhone.Trim();
        if (!string.IsNullOrWhiteSpace(request.ManagerName)) existing.ManagerName = request.ManagerName.Trim();
        if (!string.IsNullOrWhiteSpace(request.OperatingHours)) existing.OperatingHours = request.OperatingHours.Trim();
        if (request.IsEvacuationHub.HasValue) existing.IsEvacuationHub = request.IsEvacuationHub.Value;

        var updated = await GetCitizenRepo().UpdateAsync(id, existing);
        return ApiResponse<ShelterDto>.SuccessResponse(MapToDto(updated!), "Shelter updated successfully.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(string id)
    {
        var deleted = await GetCitizenRepo().DeleteAsync(id);
        if (!deleted)
        {
            return ApiResponse<bool>.ErrorResponse($"Shelter with ID '{id}' was not found or could not be deleted.");
        }
        return ApiResponse<bool>.SuccessResponse(true, "Shelter deleted successfully.");
    }

    public async Task<ApiResponse<ShelterSummaryDto>> GetCitizenSummaryAsync(string? citizenLocation = null)
    {
        var shelters = (await GetCitizenRepo().GetSheltersAsync()).ToList();

        var available = shelters.Where(s => s.Availability > 0).OrderBy(s => s.Distance).ToList();
        var nearest = available.FirstOrDefault();

        var summary = new ShelterSummaryDto
        {
            TotalShelters = shelters.Count,
            AvailableSheltersCount = available.Count,
            FullSheltersCount = shelters.Count(s => s.Availability <= 0),
            TotalCapacity = shelters.Sum(s => s.Capacity),
            TotalAvailableSpaces = shelters.Sum(s => s.Availability),
            TotalOccupiedSpaces = shelters.Sum(s => s.Occupied),
            AccessibleCount = shelters.Count(s => s.Accessible),
            NearestAvailableShelter = nearest != null ? MapToDto(nearest) : null
        };

        return ApiResponse<ShelterSummaryDto>.SuccessResponse(summary, "Shelter summary retrieved.");
    }

    public async Task<ApiResponse<ShelterBookingDto>> RequestSpaceAsync(RequestShelterSpacePayload request, string? citizenName = null)
    {
        if (string.IsNullOrWhiteSpace(request.ShelterId))
        {
            return ApiResponse<ShelterBookingDto>.ErrorResponse("Shelter ID is required.");
        }

        var shelter = await GetCitizenRepo().GetByIdAsync(request.ShelterId);
        if (shelter == null)
        {
            return ApiResponse<ShelterBookingDto>.ErrorResponse($"Shelter with ID '{request.ShelterId}' was not found.");
        }

        var count = Math.Max(1, request.NumberOfPeople);
        if (shelter.Availability < count)
        {
            return ApiResponse<ShelterBookingDto>.ErrorResponse(
                $"Insufficient space available. Only {shelter.Availability} spaces left at {shelter.Name}.");
        }

        var booking = new ShelterBooking
        {
            ShelterId = shelter.Id,
            ShelterName = shelter.Name,
            CitizenName = !string.IsNullOrWhiteSpace(request.CitizenName) ? request.CitizenName.Trim() : (!string.IsNullOrWhiteSpace(citizenName) ? citizenName.Trim() : "Samira Khan"),
            ContactPhone = !string.IsNullOrWhiteSpace(request.ContactPhone) ? request.ContactPhone.Trim() : "+94 77 123 4567",
            NumberOfPeople = count,
            SpecialNeeds = request.SpecialNeeds?.Trim(),
            Status = "Confirmed",
            BookedAt = DateTime.UtcNow
        };

        var created = await GetCitizenRepo().CreateBookingAsync(booking);

        var dto = new ShelterBookingDto
        {
            BookingReference = created.Id,
            ShelterId = shelter.Id,
            ShelterName = shelter.Name,
            CitizenName = created.CitizenName,
            ContactPhone = created.ContactPhone,
            NumberOfPeople = created.NumberOfPeople,
            SpecialNeeds = created.SpecialNeeds,
            Status = created.Status,
            BookedAt = created.BookedAt,
            ConfirmationMessage = $"Successfully reserved {count} space(s) at {shelter.Name}. Please show this reference on arrival."
        };

        return ApiResponse<ShelterBookingDto>.SuccessResponse(dto, "Space requested and confirmed successfully.");
    }

    public async Task<ApiResponse<IEnumerable<ShelterBookingDto>>> GetCitizenBookingsAsync(string? citizenName = null)
    {
        var bookings = await GetCitizenRepo().GetBookingsAsync(citizenName);
        var dtos = bookings.Select(b => new ShelterBookingDto
        {
            BookingReference = b.Id,
            ShelterId = b.ShelterId,
            ShelterName = b.ShelterName,
            CitizenName = b.CitizenName,
            ContactPhone = b.ContactPhone,
            NumberOfPeople = b.NumberOfPeople,
            SpecialNeeds = b.SpecialNeeds,
            Status = b.Status,
            BookedAt = b.BookedAt,
            ConfirmationMessage = $"Confirmed reservation for {b.NumberOfPeople} at {b.ShelterName}"
        }).ToList();

        return ApiResponse<IEnumerable<ShelterBookingDto>>.SuccessResponse(dtos, "Citizen shelter bookings retrieved.");
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
            UpdatedAt = s.UpdatedAt,
            Area = s.Area,
            Address = s.Address,
            Distance = s.Distance,
            Availability = s.Availability,
            Accessible = s.Accessible,
            ManagerName = s.ManagerName,
            OperatingHours = s.OperatingHours,
            IsEvacuationHub = s.IsEvacuationHub
        };
    }
}
