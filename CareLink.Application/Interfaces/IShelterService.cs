namespace CareLink.Application.Interfaces;

using CareLink.Application.Common;
using CareLink.Application.DTOs;

public interface IShelterService
{
    Task<ApiResponse<IEnumerable<ShelterDto>>> GetSheltersAsync(
        string? search = null,
        string? area = null,
        double? maxDistance = null,
        string? availability = null,
        bool? accessibleOnly = null,
        string? service = null);

    Task<ApiResponse<ShelterDto>> GetByIdAsync(string id);
    Task<ApiResponse<ShelterDto>> CreateAsync(CreateShelterRequest request);
    Task<ApiResponse<ShelterDto>> UpdateAsync(string id, UpdateShelterRequest request);
    Task<ApiResponse<bool>> DeleteAsync(string id);
    Task<ApiResponse<ShelterSummaryDto>> GetSummaryAsync(string? citizenLocation = null);
    Task<ApiResponse<ShelterBookingDto>> RequestSpaceAsync(RequestShelterSpacePayload request, string? citizenName = null);
    Task<ApiResponse<IEnumerable<ShelterBookingDto>>> GetCitizenBookingsAsync(string? citizenName = null);
}
