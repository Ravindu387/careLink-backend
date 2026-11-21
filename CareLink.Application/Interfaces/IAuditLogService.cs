namespace CareLink.Application.Interfaces;

using CareLink.Application.Common;
using CareLink.Application.DTOs;

public interface IAuditLogService
{
    Task<ApiResponse<List<AuditLogDto>>> GetAuditLogsAsync(AuditLogFilterParams? filters = null);
    Task<ApiResponse<AuditLogDto>> GetAuditLogByIdAsync(string id);
    Task<ApiResponse<AuditLogDto>> CreateAuditLogAsync(CreateAuditLogRequest request);
    Task<ApiResponse<AuditLogSummaryDto>> GetSummaryAsync();
}
