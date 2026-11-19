using CareLink.Application.Common;
using CareLink.Application.DTOs.SecurityEvents;
using CareLink.Application.DTOs.Users;

namespace CareLink.Application.Interfaces;

public interface ISecurityEventService
{
    Task<ApiResponse<PaginatedListDto<SecurityEventDto>>> GetEventsAsync(SecurityEventQueryDto query);
    Task<ApiResponse<SecurityEventDto>> GetEventByIdAsync(string id);
    Task<ApiResponse<SecurityEventStatsDto>> GetStatsAsync();
    Task<ApiResponse<IReadOnlyList<string>>> GetEventTypesAsync();
    Task<ApiResponse<SecurityEventDto>> CreateEventAsync(CreateSecurityEventRequestDto request, string? actor = null);
    Task<ApiResponse<SecurityEventDto>> UpdateEventStatusAsync(string id, UpdateSecurityEventStatusRequestDto request, string? actor = null);
    Task<ApiResponse<SecurityPolicyActionDto>> RecordPolicyActionAsync(RecordSecurityPolicyActionRequestDto request, string? actor = null);
    Task<byte[]> ExportEventsCsvAsync(SecurityEventQueryDto query);
}
