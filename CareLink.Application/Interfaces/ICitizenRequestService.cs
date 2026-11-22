namespace CareLink.Application.Interfaces;

using CareLink.Application.Common;
using CareLink.Application.DTOs;

public interface ICitizenRequestService
{
    Task<ApiResponse<IEnumerable<CitizenHelpRequestDto>>> GetRequestsAsync(string? citizenName = null, string? search = null, string? status = null, string? category = null);
    Task<ApiResponse<CitizenHelpRequestDto>> GetByIdAsync(string id);
    Task<ApiResponse<CitizenHelpRequestDto>> CreateRequestAsync(CreateCitizenHelpRequestPayload request, string? citizenName = null);
    Task<ApiResponse<CitizenHelpRequestDto>> UpdateStatusAsync(string id, string status, int? progress = null);
    Task<ApiResponse<bool>> DeleteRequestAsync(string id);
    Task<ApiResponse<CitizenRequestsSummaryDto>> GetSummaryAsync(string? citizenName = null);
}
