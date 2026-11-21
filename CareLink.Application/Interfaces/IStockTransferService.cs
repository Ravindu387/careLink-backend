namespace CareLink.Application.Interfaces;

using CareLink.Application.Common;
using CareLink.Application.DTOs;

public interface IStockTransferService
{
    Task<ApiResponse<IEnumerable<StockTransferDto>>> GetTransfersAsync(
        string? tab,
        string? search,
        string? source,
        string? destination,
        string? status);
    Task<ApiResponse<StockTransferDto>> GetTransferByIdAsync(string id);
    Task<ApiResponse<StockTransferSummaryDto>> GetSummaryAsync();
    Task<ApiResponse<StockTransferDto>> CreateTransferAsync(CreateStockTransferRequest request);
    Task<ApiResponse<StockTransferDto>> UpdateStatusAsync(string id, UpdateStockTransferStatusRequest request);
    Task<ApiResponse<bool>> DeleteTransferAsync(string id);
}
