namespace CareLink.Application.Interfaces;

using CareLink.Application.DTOs;
using CareLink.Core.Entities;

public interface IAuditLogRepository
{
    Task<IEnumerable<AuditLog>> GetAllAsync(AuditLogFilterParams? filters = null);
    Task<AuditLog?> GetByIdAsync(string id);
    Task<AuditLog> AddAsync(AuditLog log);
    Task<AuditLogSummaryDto> GetSummaryAsync();
}
