namespace CareLink.Application.Interfaces;

using CareLink.Application.Common;
using CareLink.Application.DTOs;

public interface IWarehouseService
{
    Task<ApiResponse<IEnumerable<WarehouseDto>>> GetWarehousesAsync(
        string? tab,
        string? search,
        string? district,
        string? capacityState,
        string? status);
    Task<ApiResponse<WarehouseDto>> GetWarehouseByIdAsync(string id);
    Task<ApiResponse<WarehouseSummaryDto>> GetSummaryAsync();
    Task<ApiResponse<WarehouseDto>> CreateWarehouseAsync(CreateWarehouseRequest request);
    Task<ApiResponse<WarehouseDto>> UpdateStatusAsync(string id, UpdateWarehouseStatusRequest request);
    Task<ApiResponse<bool>> DeleteWarehouseAsync(string id);
}
