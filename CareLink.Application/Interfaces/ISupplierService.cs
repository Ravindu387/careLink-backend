namespace CareLink.Application.Interfaces;

using CareLink.Application.Common;
using CareLink.Application.DTOs;

public interface ISupplierService
{
    Task<ApiResponse<IEnumerable<SupplierDto>>> GetSuppliersAsync(
        string? tab,
        string? search,
        string? category,
        string? region,
        string? verification,
        string? status);
    Task<ApiResponse<SupplierDto>> GetSupplierByIdAsync(string id);
    Task<ApiResponse<SupplierSummaryDto>> GetSummaryAsync();
    Task<ApiResponse<SupplierDto>> CreateSupplierAsync(CreateSupplierRequest request);
    Task<ApiResponse<SupplierDto>> UpdateStatusAsync(string id, UpdateSupplierStatusRequest request);
    Task<ApiResponse<bool>> DeleteSupplierAsync(string id);
}
