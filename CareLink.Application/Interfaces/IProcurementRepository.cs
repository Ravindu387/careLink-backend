namespace CareLink.Application.Interfaces;

using CareLink.Application.DTOs;
using CareLink.Core.Entities;

public interface IProcurementRepository
{
    Task<IEnumerable<PurchaseOrder>> GetAllAsync(ProcurementFilterParams? filters = null);
    Task<PurchaseOrder?> GetByIdAsync(string id);
    Task<PurchaseOrder> AddAsync(PurchaseOrder order);
    Task<PurchaseOrder?> UpdateStatusAsync(string id, string status, string? notes = null);
    Task<bool> DeleteAsync(string id);
    Task<ProcurementSummaryDto> GetSummaryAsync();
}
