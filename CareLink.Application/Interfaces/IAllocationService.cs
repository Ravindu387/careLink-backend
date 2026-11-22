namespace CareLink.Application.Interfaces;

using CareLink.Application.Common;
using CareLink.Application.DTOs;

public interface IAllocationService
{
    Task<ApiResponse<IEnumerable<AllocationDto>>> GetAllocationsAsync(
        string? tab,
        string? search,
        string? warehouse,
        string? destination,
        string? status);
    Task<ApiResponse<AllocationDto>> GetAllocationByIdAsync(string id);
    Task<ApiResponse<AllocationSummaryDto>> GetAllocationSummaryAsync();
    Task<ApiResponse<AllocationDto>> CreateAllocationAsync(CreateAllocationRequest request);
    Task<ApiResponse<AllocationDto>> UpdateStatusAsync(string id, UpdateAllocationStatusRequest request);
    Task<ApiResponse<bool>> DeleteAllocationAsync(string id);
}
