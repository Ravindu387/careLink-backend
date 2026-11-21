namespace CareLink.Application.Interfaces;

using CareLink.Application.DTOs;
using CareLink.Core.Entities;

public interface IResourceDashboardRepository
{
    Task<IEnumerable<ResourceRequest>> GetResourceRequestsAsync(ResourceRequestQueryParams? query = null);
    Task<ResourceRequest?> GetResourceRequestByIdAsync(string id);
    Task<ResourceRequest> AddResourceRequestAsync(ResourceRequest request);
    Task<IEnumerable<DashboardStockMovementDto>> GetStockMovementAsync();
    Task<IEnumerable<DashboardActivityDto>> GetRecentActivitiesAsync();
    Task AddActivityAsync(DashboardActivityDto activity);
}
