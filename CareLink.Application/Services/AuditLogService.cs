namespace CareLink.Application.Services;

using CareLink.Application.Common;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;

public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _repository;

    public AuditLogService(IAuditLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<List<AuditLogDto>>> GetAuditLogsAsync(AuditLogFilterParams? filters = null)
    {
        var logs = await _repository.GetAllAsync(filters);
        var dtos = logs.Select(MapToDto).ToList();
        return ApiResponse<List<AuditLogDto>>.SuccessResponse(dtos, "Audit logs retrieved successfully.");
    }

    public async Task<ApiResponse<AuditLogDto>> GetAuditLogByIdAsync(string id)
    {
        var log = await _repository.GetByIdAsync(id);
        if (log == null)
        {
            return ApiResponse<AuditLogDto>.ErrorResponse($"Audit log with ID '{id}' was not found.");
        }

        return ApiResponse<AuditLogDto>.SuccessResponse(MapToDto(log));
    }

    public async Task<ApiResponse<AuditLogDto>> CreateAuditLogAsync(CreateAuditLogRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Action))
        {
            return ApiResponse<AuditLogDto>.ErrorResponse("Action is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Record))
        {
            return ApiResponse<AuditLogDto>.ErrorResponse("Record identifier is required.");
        }

        var now = DateTime.UtcNow;
        var dateStr = string.IsNullOrWhiteSpace(request.Date)
            ? now.ToString("yyyy-MM-dd")
            : request.Date;

        var randomCode = Random.Shared.Next(100000, 999999).ToString();

        var auditLog = new AuditLog
        {
            Id = $"AUD-{randomCode}",
            Action = request.Action.Trim(),
            Actor = string.IsNullOrWhiteSpace(request.Actor) ? "Daniel Perera" : request.Actor.Trim(),
            Timestamp = now.ToString("dd MMM · HH:mm"),
            Record = request.Record.Trim(),
            Change = request.Change.Trim(),
            Rationale = request.Rationale.Trim(),
            ActionType = string.IsNullOrWhiteSpace(request.ActionType) ? "Verification" : request.ActionType.Trim(),
            RecordType = string.IsNullOrWhiteSpace(request.RecordType) ? "System" : request.RecordType.Trim(),
            Date = dateStr,
            CreatedAt = now
        };

        var created = await _repository.AddAsync(auditLog);
        return ApiResponse<AuditLogDto>.SuccessResponse(MapToDto(created), "Audit log recorded successfully.");
    }

    public async Task<ApiResponse<AuditLogSummaryDto>> GetSummaryAsync()
    {
        var summary = await _repository.GetSummaryAsync();
        return ApiResponse<AuditLogSummaryDto>.SuccessResponse(summary, "Audit log summary retrieved successfully.");
    }

    private static AuditLogDto MapToDto(AuditLog entity)
    {
        return new AuditLogDto
        {
            Id = entity.Id,
            Action = entity.Action,
            Actor = entity.Actor,
            Timestamp = entity.Timestamp,
            Record = entity.Record,
            Change = entity.Change,
            Rationale = entity.Rationale,
            ActionType = entity.ActionType,
            RecordType = entity.RecordType,
            Date = entity.Date,
            CreatedAt = entity.CreatedAt
        };
    }
}
