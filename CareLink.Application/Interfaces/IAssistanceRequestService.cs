using CareLink.Application.Common;
using CareLink.Application.DTOs.AssistanceRequests;
using CareLink.Application.DTOs.Users;

namespace CareLink.Application.Interfaces;

public interface IAssistanceRequestService
{
    Task<ApiResponse<PaginatedListDto<AssistanceRequestDto>>> GetRequestsAsync(AssistanceRequestQueryDto query);
    Task<ApiResponse<AssistanceRequestStatsDto>> GetStatsAsync();
    Task<ApiResponse<AssistanceRequestDto>> GetByIdAsync(string id);
    Task<ApiResponse<AssistanceRequestDto>> CreateRequestAsync(CreateAssistanceRequestDto request, string actor);
    Task<ApiResponse<AssistanceRequestDto>> UpdateRequestAsync(string id, UpdateAssistanceRequestDto request, string actor);
    Task<ApiResponse<AssistanceRequestDto>> UpdateStatusAsync(string id, UpdateRequestStatusDto request, string actor);
    Task<ApiResponse<AssistanceRequestDto>> UpdatePriorityAsync(string id, UpdateRequestPriorityDto request, string actor);
    Task<ApiResponse<AssistanceRequestDto>> AssignRequestAsync(string id, AssignRequestDto request, string actor);
    Task<ApiResponse<bool>> DeleteRequestAsync(string id, string actor);
}
