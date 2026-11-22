namespace CareLink.Application.Interfaces;

using CareLink.Application.DTOs;
using CareLink.Core.Entities;

public interface IResourceRequestRepository
{
    Task<IEnumerable<ResourceRequest>> GetAllAsync(ResourceRequestFilterParams? filters = null);
    Task<ResourceRequest?> GetByIdAsync(string id);
    Task<ResourceRequest> AddAsync(ResourceRequest request);
    Task<ResourceRequest?> UpdateStatusAsync(string id, string status, string? source = null);
    Task<bool> DeleteAsync(string id);
    Task<ResourceRequestSummaryDto> GetSummaryAsync();
}
