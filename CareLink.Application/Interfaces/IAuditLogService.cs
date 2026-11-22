using CareLink.Application.Common;
using CareLink.Application.DTOs;
using CareLink.Application.DTOs.AuditLogs;
using CareLink.Application.DTOs.Users;

namespace CareLink.Application.Interfaces;

public interface IAuditLogService
{
    // Platform Administrative Audit Logs (HEAD)
    Task<ApiResponse<PaginatedListDto<AuditLogDto>>> GetAuditLogsAsync(AuditLogQueryDto query);
    Task<ApiResponse<AuditLogDto>> GetAuditLogByIdAsync(string id);
    Task<ApiResponse<AuditLogStatsDto>> GetStatsAsync();
    Task<ApiResponse<IReadOnlyList<string>>> GetActionTypesAsync();
    Task<ApiResponse<IReadOnlyList<string>>> GetRecordTypesAsync();
    Task<ApiResponse<IReadOnlyList<string>>> GetActorsAsync();
    Task<ApiResponse<AuditLogDto>> CreateAuditLogAsync(CreateAuditLogRequestDto request);
    Task<(byte[] Bytes, AuditLogDto LogEntry)> ExportAuditLogAsync(ExportAuditLogRequestDto request, string actor);

    // Resource Manager Audit Logs (Oshadi)
    Task<ApiResponse<List<AuditLogDto>>> GetAuditLogsAsync(AuditLogFilterParams? filters = null);
    Task<ApiResponse<AuditLogDto>> CreateAuditLogAsync(CreateAuditLogRequest request);
    Task<ApiResponse<AuditLogSummaryDto>> GetSummaryAsync();
}
