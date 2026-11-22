using CareLink.Application.Common;
using CareLink.Application.DTOs.Organizations;
using CareLink.Application.DTOs.Users;

namespace CareLink.Application.Interfaces;

public interface IOrganizationService
{
    Task<ApiResponse<PaginatedListDto<OrganizationDto>>> GetOrganizationsAsync(OrganizationQueryDto query);
    Task<ApiResponse<OrganizationStatsDto>> GetStatsAsync();
    Task<ApiResponse<OrganizationDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<OrganizationDto>> CreateOrganizationAsync(CreateOrganizationRequestDto request, string actor);
    Task<ApiResponse<OrganizationDto>> UpdateOrganizationAsync(Guid id, UpdateOrganizationRequestDto request, string actor);
    Task<ApiResponse<bool>> UpdateStatusAsync(Guid id, string status, string actor);
    Task<ApiResponse<bool>> ApproveVerificationAsync(Guid id, string actor);
    Task<ApiResponse<bool>> DeleteOrganizationAsync(Guid id, string actor);
    Task<ApiResponse<IReadOnlyList<string>>> GetOrganizationTypesAsync();
}
