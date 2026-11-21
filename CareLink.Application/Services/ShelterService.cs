namespace CareLink.Application.Services;

using CareLink.Application.Common;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;

public class ShelterService : IShelterService
{
    private readonly IShelterRepository _repository;

    public ShelterService(IShelterRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<IEnumerable<ShelterDto>>> GetSheltersAsync(
        string? search = null,
        string? area = null,
        double? maxDistance = null,
        string? availability = null,
        bool? accessibleOnly = null,
        string? service = null)
    {
        var entities = await _repository.GetSheltersAsync(search, area, maxDistance, availability, accessibleOnly, service);
        var dtos = entities.Select(MapToDto).ToList();
        return ApiResponse<IEnumerable<ShelterDto>>.SuccessResponse(dtos, "Shelters retrieved successfully.");
    }

    public async Task<ApiResponse<ShelterDto>> GetByIdAsync(string id)
    {
        var entity = await _repository.GetByIdAsync(id);
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
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(entity);
        return ApiResponse<ShelterDto>.SuccessResponse(MapToDto(created), "Shelter created successfully.");
    }

    public async Task<ApiResponse<ShelterDto>> UpdateAsync(string id, UpdateShelterRequest request)
    {
        var existing = await _repository.GetByIdAsync(id);
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

        var updated = await _repository.UpdateAsync(id, existing);
        return ApiResponse<ShelterDto>.SuccessResponse(MapToDto(updated!), "Shelter updated successfully.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(string id)
    {
        var deleted = await _repository.DeleteAsync(id);
        if (!deleted)
        {
            return ApiResponse<bool>.ErrorResponse($"Shelter with ID '{id}' was not found or could not be deleted.");
        }
        return ApiResponse<bool>.SuccessResponse(true, "Shelter deleted successfully.");
    }

    public async Task<ApiResponse<ShelterSummaryDto>> GetSummaryAsync(string? citizenLocation = null)
    {
        var shelters = (await _repository.GetSheltersAsync()).ToList();

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

        var shelter = await _repository.GetByIdAsync(request.ShelterId);
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

        var created = await _repository.CreateBookingAsync(booking);

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
        var bookings = await _repository.GetBookingsAsync(citizenName);
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

    private static ShelterDto MapToDto(Shelter entity)
    {
        return new ShelterDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Area = entity.Area,
            Address = entity.Address,
            Distance = entity.Distance,
            Capacity = entity.Capacity,
            Availability = entity.Availability,
            Occupied = entity.Occupied,
            Services = entity.Services,
            Status = entity.Status,
            Accessible = entity.Accessible,
            ContactPhone = entity.ContactPhone,
            ManagerName = entity.ManagerName,
            OperatingHours = entity.OperatingHours,
            IsEvacuationHub = entity.IsEvacuationHub
        };
    }
}
