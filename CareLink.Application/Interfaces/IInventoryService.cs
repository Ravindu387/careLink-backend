namespace CareLink.Application.Interfaces;

using CareLink.Application.Common;
using CareLink.Application.DTOs;

public interface IInventoryService
{
    Task<ApiResponse<List<InventoryItemDto>>> GetInventoryAsync(InventoryFilterParams? filters = null);
    Task<ApiResponse<InventoryItemDto>> GetInventoryByIdAsync(string id);
    Task<ApiResponse<InventoryItemDto>> CreateInventoryItemAsync(CreateInventoryItemRequest request);
    Task<ApiResponse<InventoryItemDto>> UpdateInventoryItemAsync(string id, UpdateInventoryItemRequest request);
    Task<ApiResponse<InventoryItemDto>> AdjustStockAsync(string id, AdjustStockRequest request);
    Task<ApiResponse<bool>> DeleteInventoryItemAsync(string id);
    Task<ApiResponse<InventorySummaryDto>> GetSummaryAsync();
}
