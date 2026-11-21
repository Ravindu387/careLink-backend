namespace CareLink.Application.Interfaces;

using CareLink.Application.DTOs;
using CareLink.Core.Entities;

public interface IDispatchRepository
{
    Task<IEnumerable<Dispatch>> GetAllAsync(DispatchFilterParams? filters = null);
    Task<Dispatch?> GetByIdAsync(string id);
    Task<Dispatch> AddAsync(Dispatch dispatch);
    Task<Dispatch?> UpdateStatusAsync(string id, string status, string? currentLocation = null, string? notes = null);
    Task<bool> DeleteAsync(string id);
    Task<DispatchSummaryDto> GetSummaryAsync();
}
