namespace CareLink.Application.Interfaces;

using CareLink.Application.Common;
using CareLink.Application.DTOs;

public interface IHouseholdService
{
    Task<ApiResponse<HouseholdProfileDto>> GetProfileAsync(string? citizenName = null);
    Task<ApiResponse<IEnumerable<HouseholdMemberDto>>> GetMembersAsync(string? citizenName = null);
    Task<ApiResponse<HouseholdMemberDto>> GetMemberByIdAsync(string memberId, string? citizenName = null);
    Task<ApiResponse<HouseholdMemberDto>> AddMemberAsync(CreateHouseholdMemberRequest request, string? citizenName = null);
    Task<ApiResponse<HouseholdMemberDto>> UpdateMemberAsync(string memberId, UpdateHouseholdMemberRequest request, string? citizenName = null);
    Task<ApiResponse<bool>> DeleteMemberAsync(string memberId, string? citizenName = null);
    Task<ApiResponse<HouseholdProfileDto>> UpdateAddressAsync(UpdateAddressRequest request, string? citizenName = null);
    Task<ApiResponse<HouseholdProfileDto>> UpdateEmergencyContactAsync(UpdateEmergencyContactRequest request, string? citizenName = null);
    Task<ApiResponse<IEnumerable<ReadinessChecklistItemDto>>> ToggleReadinessAsync(ToggleReadinessRequest request, string? citizenName = null);
    Task<ApiResponse<HouseholdSummaryDto>> GetSummaryAsync(string? citizenName = null);
}
