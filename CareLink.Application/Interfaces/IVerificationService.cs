using CareLink.Application.Common;
using CareLink.Application.DTOs.Users;
using CareLink.Application.DTOs.Verification;

namespace CareLink.Application.Interfaces;

public interface IVerificationService
{
    Task<ApiResponse<PaginatedListDto<VerificationRequestDto>>> GetRequestsAsync(VerificationQueryDto query);
    Task<ApiResponse<VerificationRequestDto>> GetRequestByIdAsync(string id);
    Task<ApiResponse<VerificationStatsDto>> GetStatsAsync();
    Task<ApiResponse<IReadOnlyList<string>>> GetRequestTypesAsync();
    Task<ApiResponse<VerificationRequestDto>> CreateRequestAsync(CreateVerificationRequestDto request);
    Task<ApiResponse<VerificationRequestDto>> RecordDecisionAsync(string id, VerificationDecisionRequestDto request, string actor);
    Task<byte[]> ExportCsvAsync(VerificationQueryDto query);
}
