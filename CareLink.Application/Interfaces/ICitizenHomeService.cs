namespace CareLink.Application.Interfaces;

using CareLink.Application.Common;
using CareLink.Application.DTOs;

public interface ICitizenHomeService
{
    Task<ApiResponse<CitizenHomeOverviewDto>> GetHomeOverviewAsync(string? citizenName = null);
    Task<ApiResponse<IEnumerable<CitizenRequestDto>>> GetRequestsAsync(string? citizenName = null, string? search = null, string? status = null);
    Task<ApiResponse<CitizenRequestDto>> GetRequestByIdAsync(string id);
    Task<ApiResponse<CitizenRequestDto>> CreateRequestAsync(CreateCitizenRequest request);
    Task<ApiResponse<CitizenRequestDto>> UpdateRequestStatusAsync(string id, UpdateCitizenRequestStatus request);
    Task<ApiResponse<bool>> DeleteRequestAsync(string id);
    Task<ApiResponse<CitizenRequestsSummaryDto>> GetRequestsSummaryAsync(string? citizenName = null);
    Task<ApiResponse<IEnumerable<NearbyFacilityDto>>> GetFacilitiesAsync(string? search = null, string? category = null, string? availability = null, string? distance = null);
    Task<ApiResponse<NearbyFacilityDto>> GetFacilityByIdAsync(string id);
    Task<ApiResponse<NearbySupportSummaryDto>> GetNearbySupportSummaryAsync();
    Task<ApiResponse<NearbyFacilityDto>> UpdateFacilityStatusAsync(string id, string status);
    Task<ApiResponse<NearbyFacilityDto>> CreateFacilityAsync(CreateFacilityRequest request);
    Task<ApiResponse<CrisisOperationDto>> GetActiveOperationAsync();
}
