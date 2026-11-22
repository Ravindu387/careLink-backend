namespace CareLink.Application.Interfaces;

using CareLink.Application.Common;
using CareLink.Application.DTOs;

public interface IResourceRequestService
{
    Task<ApiResponse<List<ResourceRequestDto>>> GetRequestsAsync(ResourceRequestFilterParams? filters = null);
    Task<ApiResponse<ResourceRequestDto>> GetRequestByIdAsync(string id);
    Task<ApiResponse<ResourceRequestSummaryDto>> GetSummaryAsync();
    Task<ApiResponse<ResourceRequestDto>> CreateRequestAsync(CreateResourceRequestPayload payload);
    Task<ApiResponse<ResourceRequestDto>> UpdateRequestStatusAsync(string id, UpdateResourceRequestStatusPayload payload);
    Task<ApiResponse<bool>> DeleteRequestAsync(string id);
    Task<(byte[] Bytes, string ContentType, string FileName)> ExportRequestsAsync(ResourceRequestFilterParams? filters = null, string format = "csv");
}
