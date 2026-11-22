namespace CareLink.Application.Interfaces;

using CareLink.Application.Common;
using CareLink.Application.DTOs;

public interface IDispatchService
{
    Task<ApiResponse<List<DispatchDto>>> GetDispatchesAsync(DispatchFilterParams? filters = null);
    Task<ApiResponse<DispatchDto>> GetDispatchByIdAsync(string id);
    Task<ApiResponse<DispatchDto>> CreateDispatchAsync(CreateDispatchRequest request);
    Task<ApiResponse<DispatchDto>> UpdateStatusAsync(string id, UpdateDispatchStatusRequest request);
    Task<ApiResponse<bool>> DeleteDispatchAsync(string id);
    Task<ApiResponse<DispatchSummaryDto>> GetSummaryAsync();
}
