namespace CareLink.Application.Services;

using CareLink.Application.Common;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;

public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _repository;

    public InventoryService(IInventoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<List<InventoryItemDto>>> GetInventoryAsync(InventoryFilterParams? filters = null)
    {
        var items = await _repository.GetAllAsync(filters);
        var dtos = items.Select(MapToDto).ToList();
        return ApiResponse<List<InventoryItemDto>>.SuccessResponse(dtos, "Inventory retrieved successfully.");
    }

    public async Task<ApiResponse<InventoryItemDto>> GetInventoryByIdAsync(string id)
    {
        var item = await _repository.GetByIdAsync(id);
        if (item == null)
        {
            return ApiResponse<InventoryItemDto>.ErrorResponse($"Inventory item with ID '{id}' was not found.");
        }
        return ApiResponse<InventoryItemDto>.SuccessResponse(MapToDto(item));
    }

    public async Task<ApiResponse<InventoryItemDto>> CreateInventoryItemAsync(CreateInventoryItemRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Resource))
        {
            return ApiResponse<InventoryItemDto>.ErrorResponse("Resource name is required.");
        }

        var onHand = Math.Max(0, request.OnHand);
        var reserved = Math.Max(0, request.Reserved);
        var inTransit = Math.Max(0, request.InTransit);
        var available = Math.Max(0, onHand - reserved);

        string status;
        if (available <= 10)
        {
            status = "Critical";
        }
        else if (available <= 100)
        {
            status = "Low Stock";
        }
        else
        {
            status = "Healthy";
        }

        var item = new InventoryItem
        {
            Resource = request.Resource.Trim(),
            Category = string.IsNullOrWhiteSpace(request.Category) ? "Food & Water" : request.Category.Trim(),
            Warehouse = string.IsNullOrWhiteSpace(request.Warehouse) ? "Central Logistics Hub" : request.Warehouse.Trim(),
            OnHand = onHand,
            Reserved = reserved,
            InTransit = inTransit,
            Available = available,
            Status = status,
            Unit = string.IsNullOrWhiteSpace(request.Unit) ? "units" : request.Unit.Trim(),
            ExpiryDate = request.ExpiryDate,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(item);
        return ApiResponse<InventoryItemDto>.SuccessResponse(MapToDto(created), "Resource added to inventory successfully.");
    }

    public async Task<ApiResponse<InventoryItemDto>> UpdateInventoryItemAsync(string id, UpdateInventoryItemRequest request)
    {
        var updated = await _repository.UpdateAsync(id, request);
        if (updated == null)
        {
            return ApiResponse<InventoryItemDto>.ErrorResponse($"Inventory item with ID '{id}' was not found.");
        }
        return ApiResponse<InventoryItemDto>.SuccessResponse(MapToDto(updated), "Inventory item updated successfully.");
    }

    public async Task<ApiResponse<InventoryItemDto>> AdjustStockAsync(string id, AdjustStockRequest request)
    {
        var updated = await _repository.AdjustStockAsync(id, request.QuantityDelta);
        if (updated == null)
        {
            return ApiResponse<InventoryItemDto>.ErrorResponse($"Inventory item with ID '{id}' was not found.");
        }
        return ApiResponse<InventoryItemDto>.SuccessResponse(MapToDto(updated), "Stock adjusted successfully.");
    }

    public async Task<ApiResponse<bool>> DeleteInventoryItemAsync(string id)
    {
        var deleted = await _repository.DeleteAsync(id);
        if (!deleted)
        {
            return ApiResponse<bool>.ErrorResponse($"Inventory item with ID '{id}' was not found.");
        }
        return ApiResponse<bool>.SuccessResponse(true, "Inventory item removed successfully.");
    }

    public async Task<ApiResponse<InventorySummaryDto>> GetSummaryAsync()
    {
        var summary = await _repository.GetSummaryAsync();
        return ApiResponse<InventorySummaryDto>.SuccessResponse(summary, "Inventory summary retrieved successfully.");
    }

    private static InventoryItemDto MapToDto(InventoryItem entity)
    {
        return new InventoryItemDto
        {
            Id = entity.Id,
            Resource = entity.Resource,
            Category = entity.Category,
            Warehouse = entity.Warehouse,
            OnHand = entity.OnHand,
            Reserved = entity.Reserved,
            InTransit = entity.InTransit,
            Available = entity.Available,
            Status = entity.Status,
            Unit = entity.Unit,
            ExpiryDate = entity.ExpiryDate,
            CreatedAt = entity.CreatedAt
        };
    }
}
