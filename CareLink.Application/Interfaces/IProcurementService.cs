namespace CareLink.Application.Interfaces;

using CareLink.Application.Common;
using CareLink.Application.DTOs;

public interface IProcurementService
{
    Task<ApiResponse<List<PurchaseOrderDto>>> GetOrdersAsync(ProcurementFilterParams? filters = null);
    Task<ApiResponse<PurchaseOrderDto>> GetOrderByIdAsync(string id);
    Task<ApiResponse<PurchaseOrderDto>> CreateOrderAsync(CreatePurchaseOrderRequest request);
    Task<ApiResponse<PurchaseOrderDto>> UpdateStatusAsync(string id, UpdatePurchaseOrderStatusRequest request);
    Task<ApiResponse<bool>> DeleteOrderAsync(string id);
    Task<ApiResponse<ProcurementSummaryDto>> GetSummaryAsync();
}
