namespace CareLink.Application.Interfaces;

using CareLink.Application.DTOs;
using CareLink.Core.Entities;

public interface IInventoryRepository
{
    Task<IEnumerable<InventoryItem>> GetAllAsync(InventoryFilterParams? filters = null);
    Task<InventoryItem?> GetByIdAsync(string id);
    Task<InventoryItem> AddAsync(InventoryItem item);
    Task<InventoryItem?> UpdateAsync(string id, UpdateInventoryItemRequest request);
    Task<InventoryItem?> AdjustStockAsync(string id, int delta);
    Task<bool> DeleteAsync(string id);
    Task<InventorySummaryDto> GetSummaryAsync();
}
