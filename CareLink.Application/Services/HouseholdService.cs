namespace CareLink.Application.Services;

using CareLink.Application.Common;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;

public class HouseholdService : IHouseholdService
{
    private readonly IHouseholdRepository _repository;

    public HouseholdService(IHouseholdRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<HouseholdProfileDto>> GetProfileAsync(string? citizenName = null)
    {
        var profile = await _repository.GetProfileAsync(citizenName);
        return ApiResponse<HouseholdProfileDto>.SuccessResponse(MapToDto(profile), "Household profile retrieved successfully");
    }

    public async Task<ApiResponse<IEnumerable<HouseholdMemberDto>>> GetMembersAsync(string? citizenName = null)
    {
        var profile = await _repository.GetProfileAsync(citizenName);
        var memberDtos = profile.Members.Select(MapMemberToDto).ToList();
        return ApiResponse<IEnumerable<HouseholdMemberDto>>.SuccessResponse(memberDtos, "Household members retrieved successfully");
    }

    public async Task<ApiResponse<HouseholdMemberDto>> GetMemberByIdAsync(string memberId, string? citizenName = null)
    {
        var member = await _repository.GetMemberByIdAsync(memberId, citizenName);
        if (member == null)
        {
            return ApiResponse<HouseholdMemberDto>.ErrorResponse($"Household member with ID '{memberId}' was not found.");
        }

        return ApiResponse<HouseholdMemberDto>.SuccessResponse(MapMemberToDto(member), "Household member retrieved successfully");
    }

    public async Task<ApiResponse<HouseholdMemberDto>> AddMemberAsync(CreateHouseholdMemberRequest request, string? citizenName = null)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return ApiResponse<HouseholdMemberDto>.ErrorResponse("Household member name is required.");
        }

        var member = new HouseholdMember
        {
            Name = request.Name.Trim(),
            Role = string.IsNullOrWhiteSpace(request.Role) ? "Household member" : request.Role.Trim(),
            SupportNeeds = string.IsNullOrWhiteSpace(request.Support) ? "No support needs" : request.Support.Trim(),
            Age = request.Age,
            MedicalNotes = request.MedicalNotes?.Trim(),
            DietaryNotes = request.DietaryNotes?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.AddMemberAsync(member, citizenName);
        return ApiResponse<HouseholdMemberDto>.SuccessResponse(MapMemberToDto(created), "Household member added successfully");
    }

    public async Task<ApiResponse<HouseholdMemberDto>> UpdateMemberAsync(string memberId, UpdateHouseholdMemberRequest request, string? citizenName = null)
    {
        var existing = await _repository.GetMemberByIdAsync(memberId, citizenName);
        if (existing == null)
        {
            return ApiResponse<HouseholdMemberDto>.ErrorResponse($"Household member with ID '{memberId}' was not found.");
        }

        existing.Name = string.IsNullOrWhiteSpace(request.Name) ? existing.Name : request.Name.Trim();
        existing.Role = string.IsNullOrWhiteSpace(request.Role) ? existing.Role : request.Role.Trim();
        existing.SupportNeeds = string.IsNullOrWhiteSpace(request.Support) ? existing.SupportNeeds : request.Support.Trim();
        existing.Age = request.Age ?? existing.Age;
        existing.MedicalNotes = request.MedicalNotes ?? existing.MedicalNotes;
        existing.DietaryNotes = request.DietaryNotes ?? existing.DietaryNotes;

        var updated = await _repository.UpdateMemberAsync(existing, citizenName);
        return ApiResponse<HouseholdMemberDto>.SuccessResponse(MapMemberToDto(updated!), "Household member updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteMemberAsync(string memberId, string? citizenName = null)
    {
        var deleted = await _repository.DeleteMemberAsync(memberId, citizenName);
        if (!deleted)
        {
            return ApiResponse<bool>.ErrorResponse($"Household member with ID '{memberId}' was not found.");
        }

        return ApiResponse<bool>.SuccessResponse(true, "Household member removed successfully");
    }

    public async Task<ApiResponse<HouseholdProfileDto>> UpdateAddressAsync(UpdateAddressRequest request, string? citizenName = null)
    {
        if (string.IsNullOrWhiteSpace(request.Address))
        {
            return ApiResponse<HouseholdProfileDto>.ErrorResponse("Address is required.");
        }

        var profile = await _repository.UpdateAddressAsync(request.Address, request.Ward, request.City, citizenName);
        return ApiResponse<HouseholdProfileDto>.SuccessResponse(MapToDto(profile), "Household address updated successfully");
    }

    public async Task<ApiResponse<HouseholdProfileDto>> UpdateEmergencyContactAsync(UpdateEmergencyContactRequest request, string? citizenName = null)
    {
        if (string.IsNullOrWhiteSpace(request.ContactName) || string.IsNullOrWhiteSpace(request.ContactPhone))
        {
            return ApiResponse<HouseholdProfileDto>.ErrorResponse("Contact name and phone number are required.");
        }

        var profile = await _repository.UpdateEmergencyContactAsync(request.ContactName, request.ContactPhone, request.Relationship, citizenName);
        return ApiResponse<HouseholdProfileDto>.SuccessResponse(MapToDto(profile), "Emergency contact updated successfully");
    }

    public async Task<ApiResponse<IEnumerable<ReadinessChecklistItemDto>>> ToggleReadinessAsync(ToggleReadinessRequest request, string? citizenName = null)
    {
        if (string.IsNullOrWhiteSpace(request.Key))
        {
            return ApiResponse<IEnumerable<ReadinessChecklistItemDto>>.ErrorResponse("Readiness checklist key is required.");
        }

        var list = await _repository.ToggleReadinessItemAsync(request.Key, request.IsCompleted, citizenName);
        var dtos = list.Select(MapReadinessToDto).ToList();
        return ApiResponse<IEnumerable<ReadinessChecklistItemDto>>.SuccessResponse(dtos, "Readiness status updated successfully");
    }

    public async Task<ApiResponse<HouseholdSummaryDto>> GetSummaryAsync(string? citizenName = null)
    {
        var profile = await _repository.GetProfileAsync(citizenName);
        var summary = CalculateSummary(profile);
        return ApiResponse<HouseholdSummaryDto>.SuccessResponse(summary, "Household summary retrieved successfully");
    }

    private static HouseholdProfileDto MapToDto(HouseholdProfile profile)
    {
        return new HouseholdProfileDto
        {
            Id = profile.Id,
            CitizenName = profile.CitizenName,
            CitizenId = profile.CitizenId,
            Address = profile.Address,
            Ward = profile.Ward,
            City = profile.City,
            EmergencyContactName = profile.EmergencyContactName,
            EmergencyContactPhone = profile.EmergencyContactPhone,
            EmergencyContactRelationship = profile.EmergencyContactRelationship,
            Members = profile.Members.Select(MapMemberToDto).ToList(),
            ReadinessChecklist = profile.ReadinessChecklist.Select(MapReadinessToDto).ToList(),
            Summary = CalculateSummary(profile)
        };
    }

    private static HouseholdMemberDto MapMemberToDto(HouseholdMember member)
    {
        return new HouseholdMemberDto
        {
            Id = member.Id,
            Name = member.Name,
            Initials = member.Initials,
            Role = member.Role,
            Support = member.SupportNeeds,
            Age = member.Age,
            MedicalNotes = member.MedicalNotes,
            DietaryNotes = member.DietaryNotes,
            CreatedAt = member.CreatedAt
        };
    }

    private static ReadinessChecklistItemDto MapReadinessToDto(ReadinessChecklistItem item)
    {
        return new ReadinessChecklistItemDto
        {
            Key = item.Key,
            Title = item.Title,
            IsCompleted = item.IsCompleted,
            ActionText = item.ActionText
        };
    }

    private static HouseholdSummaryDto CalculateSummary(HouseholdProfile profile)
    {
        var total = profile.Members.Count;
        var supportNeeded = profile.Members.Count(m =>
            !string.IsNullOrWhiteSpace(m.SupportNeeds) &&
            !m.SupportNeeds.Equals("No support needs", StringComparison.OrdinalIgnoreCase));

        var readinessTotal = profile.ReadinessChecklist.Count;
        var readinessCompleted = profile.ReadinessChecklist.Count(r => r.IsCompleted);
        var readinessPct = readinessTotal > 0 ? (int)Math.Round((double)readinessCompleted / readinessTotal * 100) : 0;

        return new HouseholdSummaryDto
        {
            TotalMembers = total,
            SupportNeededCount = supportNeeded,
            ReadinessCompletedCount = readinessCompleted,
            ReadinessTotalCount = readinessTotal,
            ReadinessPercentage = readinessPct
        };
    }
}
