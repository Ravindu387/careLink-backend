namespace CareLink.Application.Services;

using CareLink.Application.Common;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;

public class ProcurementService : IProcurementService
{
    private readonly IProcurementRepository _repository;

    public ProcurementService(IProcurementRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<List<PurchaseOrderDto>>> GetOrdersAsync(ProcurementFilterParams? filters = null)
    {
        var orders = await _repository.GetAllAsync(filters);
        var dtos = orders.Select(MapToDto).ToList();
        return ApiResponse<List<PurchaseOrderDto>>.SuccessResponse(dtos, "Purchase orders retrieved successfully.");
    }

    public async Task<ApiResponse<PurchaseOrderDto>> GetOrderByIdAsync(string id)
    {
        var order = await _repository.GetByIdAsync(id);
        if (order == null)
        {
            return ApiResponse<PurchaseOrderDto>.ErrorResponse($"Purchase order with ID '{id}' was not found.");
        }
        return ApiResponse<PurchaseOrderDto>.SuccessResponse(MapToDto(order));
    }

    public async Task<ApiResponse<PurchaseOrderDto>> CreateOrderAsync(CreatePurchaseOrderRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Supplier))
        {
            return ApiResponse<PurchaseOrderDto>.ErrorResponse("Supplier name is required.");
        }
        if (string.IsNullOrWhiteSpace(request.Items))
        {
            return ApiResponse<PurchaseOrderDto>.ErrorResponse("Items description is required.");
        }
        if (string.IsNullOrWhiteSpace(request.Quantity))
        {
            return ApiResponse<PurchaseOrderDto>.ErrorResponse("Quantity is required.");
        }
        if (string.IsNullOrWhiteSpace(request.Value))
        {
            return ApiResponse<PurchaseOrderDto>.ErrorResponse("Total value is required.");
        }

        var formattedValue = request.Value.Trim();
        if (!formattedValue.StartsWith("$"))
        {
            formattedValue = $"${formattedValue}";
        }

        var order = new PurchaseOrder
        {
            Supplier = request.Supplier.Trim(),
            Items = request.Items.Trim(),
            Quantity = request.Quantity.Trim(),
            Value = formattedValue,
            Type = string.IsNullOrWhiteSpace(request.Type) ? "Standard" : request.Type.Trim(),
            DeliveryDue = string.IsNullOrWhiteSpace(request.DeliveryDue) ? "TBD" : request.DeliveryDue.Trim(),
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Awaiting Approval" : request.Status.Trim(),
            Notes = request.Notes?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(order);
        return ApiResponse<PurchaseOrderDto>.SuccessResponse(MapToDto(created), "Purchase order created successfully.");
    }

    public async Task<ApiResponse<PurchaseOrderDto>> UpdateStatusAsync(string id, UpdatePurchaseOrderStatusRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Status))
        {
            return ApiResponse<PurchaseOrderDto>.ErrorResponse("Status is required.");
        }

        var validStatuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Approved", "In Transit", "Awaiting Approval", "Delivered", "Rejected"
        };

        if (!validStatuses.Contains(request.Status.Trim()))
        {
            return ApiResponse<PurchaseOrderDto>.ErrorResponse($"Invalid status '{request.Status}'. Valid values are: Approved, In Transit, Awaiting Approval, Delivered, Rejected.");
        }

        var updated = await _repository.UpdateStatusAsync(id, request.Status.Trim(), request.Notes);
        if (updated == null)
        {
            return ApiResponse<PurchaseOrderDto>.ErrorResponse($"Purchase order with ID '{id}' was not found.");
        }

        return ApiResponse<PurchaseOrderDto>.SuccessResponse(MapToDto(updated), $"Purchase order status updated to {updated.Status}.");
    }

    public async Task<ApiResponse<bool>> DeleteOrderAsync(string id)
    {
        var deleted = await _repository.DeleteAsync(id);
        if (!deleted)
        {
            return ApiResponse<bool>.ErrorResponse($"Purchase order with ID '{id}' was not found.");
        }
        return ApiResponse<bool>.SuccessResponse(true, "Purchase order cancelled/deleted successfully.");
    }

    public async Task<ApiResponse<ProcurementSummaryDto>> GetSummaryAsync()
    {
        var summary = await _repository.GetSummaryAsync();
        return ApiResponse<ProcurementSummaryDto>.SuccessResponse(summary, "Procurement summary retrieved successfully.");
    }

    private static PurchaseOrderDto MapToDto(PurchaseOrder entity)
    {
        return new PurchaseOrderDto
        {
            Id = entity.Id,
            Supplier = entity.Supplier,
            Items = entity.Items,
            Quantity = entity.Quantity,
            Value = entity.Value,
            Type = entity.Type,
            DeliveryDue = entity.DeliveryDue,
            Status = entity.Status,
            Notes = entity.Notes,
            CreatedAt = entity.CreatedAt
        };
    }
}
