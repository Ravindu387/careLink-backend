namespace CareLink.Application.Interfaces;

using CareLink.Application.Common;
using CareLink.Application.DTOs;

public interface IResourceDashboardService
{
    Task<ApiResponse<ResourceDashboardDto>> GetDashboardDataAsync();
    Task<ApiResponse<List<DashboardRequestItemDto>>> GetResourceRequestsAsync(ResourceRequestQueryParams? query = null);
    Task<ApiResponse<DashboardRequestItemDto>> CreateResourceRequestAsync(CreateDashboardResourceRequestDto dto);
    Task<ApiResponse<bool>> RecordStockReceiptAsync(RecordStockReceiptRequest request);
    Task<(byte[] Bytes, string ContentType, string FileName)> ExportDashboardSnapshotAsync(string format = "csv");
}
