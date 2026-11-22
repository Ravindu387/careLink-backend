using CareLink.Application.Common;
using CareLink.Application.DTOs;
using CareLink.Application.DTOs.Users;
using AdminDtos = CareLink.Application.DTOs.AssistanceRequests;

namespace CareLink.Application.Interfaces;

public interface IAssistanceRequestService
{
    // Operational & Coordinator Methods (Safna)
    Task<IReadOnlyList<AssistanceRequestResponseDto>> GetAllAsync(
        AssistanceRequestQueryParameters query,
        CancellationToken cancellationToken = default);

    Task<AssistanceRequestResponseDto?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default);

    Task<AssistanceRequestResponseDto> CreateAsync(
        CareLink.Application.DTOs.CreateAssistanceRequestDto dto,
        CancellationToken cancellationToken = default);

    Task<AssistanceRequestResponseDto?> UpdateAsync(
        string id,
        CareLink.Application.DTOs.UpdateAssistanceRequestDto dto,
        CancellationToken cancellationToken = default);

    Task<AssistanceRequestResponseDto?> AssignAsync(
        string id,
        AssignAssistanceRequestDto dto,
        CancellationToken cancellationToken = default);

    Task<AssistanceRequestResponseDto?> UpdateStatusAsync(
        string id,
        UpdateAssistanceRequestStatusDto dto,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        string id,
        CancellationToken cancellationToken = default);

    Task<AssistanceRequestMetricsDto> GetMetricsAsync(
        CancellationToken cancellationToken = default);

    Task ResetDataAsync(
        CancellationToken cancellationToken = default);

    // Administrative & Management Methods (dev / HEAD)
    Task<ApiResponse<PaginatedListDto<AdminDtos.AssistanceRequestDto>>> GetRequestsAsync(
        AdminDtos.AssistanceRequestQueryDto query);

    Task<ApiResponse<AdminDtos.AssistanceRequestStatsDto>> GetStatsAsync();

    Task<ApiResponse<AdminDtos.AssistanceRequestDto>> GetAdminByIdAsync(string id);

    Task<ApiResponse<AdminDtos.AssistanceRequestDto>> CreateRequestAsync(
        AdminDtos.CreateAssistanceRequestDto request,
        string actor);

    Task<ApiResponse<AdminDtos.AssistanceRequestDto>> UpdateRequestAsync(
        string id,
        AdminDtos.UpdateAssistanceRequestDto request,
        string actor);

    Task<ApiResponse<AdminDtos.AssistanceRequestDto>> UpdateStatusAsync(
        string id,
        AdminDtos.UpdateRequestStatusDto request,
        string actor);

    Task<ApiResponse<AdminDtos.AssistanceRequestDto>> UpdatePriorityAsync(
        string id,
        AdminDtos.UpdateRequestPriorityDto request,
        string actor);

    Task<ApiResponse<AdminDtos.AssistanceRequestDto>> AssignRequestAsync(
        string id,
        AdminDtos.AssignRequestDto request,
        string actor);

    Task<ApiResponse<bool>> DeleteRequestAsync(
        string id,
        string actor);
}
