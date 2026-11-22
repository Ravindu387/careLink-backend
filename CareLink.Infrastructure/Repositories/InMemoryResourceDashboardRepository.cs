namespace CareLink.Infrastructure.Repositories;

using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;

public class InMemoryResourceDashboardRepository : IResourceDashboardRepository
{
    private readonly List<ResourceRequest> _requests = [];
    private readonly List<DashboardStockMovementDto> _movement = [];
    private readonly List<DashboardActivityDto> _activities = [];
    private readonly Lock _lock = new();

    public InMemoryResourceDashboardRepository()
    {
        SeedInitialData();
    }

    public Task<IEnumerable<ResourceRequest>> GetResourceRequestsAsync(ResourceRequestQueryParams? query = null)
    {
        lock (_lock)
        {
            IEnumerable<ResourceRequest> result = _requests.OrderByDescending(r => r.CreatedAt).ToList();

            if (query != null)
            {
                if (!string.IsNullOrWhiteSpace(query.Search))
                {
                    var s = query.Search.Trim().ToLowerInvariant();
                    result = result.Where(r =>
                        r.Id.ToLowerInvariant().Contains(s) ||
                        r.Unit.ToLowerInvariant().Contains(s) ||
                        r.Resource.ToLowerInvariant().Contains(s) ||
                        r.Source.ToLowerInvariant().Contains(s)
                    );
                }

                if (!string.IsNullOrWhiteSpace(query.Priority) &&
                    !query.Priority.Equals("All priorities", StringComparison.OrdinalIgnoreCase))
                {
                    result = result.Where(r => r.Priority.Equals(query.Priority, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrWhiteSpace(query.Status) &&
                    !query.Status.Equals("All statuses", StringComparison.OrdinalIgnoreCase))
                {
                    result = result.Where(r => r.Status.Equals(query.Status, StringComparison.OrdinalIgnoreCase));
                }
            }

            return Task.FromResult(result);
        }
    }

    public Task<ResourceRequest?> GetResourceRequestByIdAsync(string id)
    {
        lock (_lock)
        {
            var item = _requests.FirstOrDefault(r => r.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(item);
        }
    }

    public Task<ResourceRequest> AddResourceRequestAsync(ResourceRequest request)
    {
        lock (_lock)
        {
            if (string.IsNullOrWhiteSpace(request.Id))
            {
                request.Id = $"RR-{_requests.Count + 2842}";
            }
            request.CreatedAt = DateTime.UtcNow;
            _requests.Insert(0, request);
            return Task.FromResult(request);
        }
    }

    public Task<IEnumerable<DashboardStockMovementDto>> GetStockMovementAsync()
    {
        lock (_lock)
        {
            return Task.FromResult<IEnumerable<DashboardStockMovementDto>>(_movement.ToList());
        }
    }

    public Task<IEnumerable<DashboardActivityDto>> GetRecentActivitiesAsync()
    {
        lock (_lock)
        {
            return Task.FromResult<IEnumerable<DashboardActivityDto>>(_activities.Take(6).ToList());
        }
    }

    public Task AddActivityAsync(DashboardActivityDto activity)
    {
        lock (_lock)
        {
            _activities.Insert(0, activity);
            if (_activities.Count > 20)
            {
                _activities.RemoveAt(_activities.Count - 1);
            }
            return Task.CompletedTask;
        }
    }

    private void SeedInitialData()
    {
        _requests.AddRange([
            new ResourceRequest
            {
                Id = "RR-2841",
                Unit = "North Ward Shelter Network",
                Resource = "Drinking Water - 10L",
                Quantity = "2,400 units",
                Priority = "Critical",
                Category = "Food & Water",
                Source = "Central Hub",
                Age = "18 min",
                Status = "Awaiting Approval",
                CreatedAt = DateTime.UtcNow.AddMinutes(-18)
            },
            new ResourceRequest
            {
                Id = "RR-2839",
                Unit = "Medical Team 3",
                Resource = "Trauma Medical Kit",
                Quantity = "24 units",
                Priority = "Critical",
                Category = "Medical",
                Source = "Central Hub",
                Age = "34 min",
                Status = "Partially Available",
                CreatedAt = DateTime.UtcNow.AddMinutes(-34)
            },
            new ResourceRequest
            {
                Id = "RR-2836",
                Unit = "Eastbank Community Centre",
                Resource = "Family Food Pack",
                Quantity = "500 units",
                Priority = "High",
                Category = "Food & Water",
                Source = "North Hub",
                Age = "49 min",
                Status = "Awaiting Approval",
                CreatedAt = DateTime.UtcNow.AddMinutes(-49)
            },
            new ResourceRequest
            {
                Id = "RR-2828",
                Unit = "Mobility Response Team",
                Resource = "Fuel - Diesel",
                Quantity = "1,200 L",
                Priority = "High",
                Category = "Transport & Fuel",
                Source = "West Depot",
                Age = "1h 12m",
                Status = "Allocated",
                CreatedAt = DateTime.UtcNow.AddMinutes(-72)
            }
        ]);

        _movement.AddRange([
            new DashboardStockMovementDto { Day = "Mon", Received = 68, Issued = 45 },
            new DashboardStockMovementDto { Day = "Tue", Received = 42, Issued = 61 },
            new DashboardStockMovementDto { Day = "Wed", Received = 85, Issued = 55 },
            new DashboardStockMovementDto { Day = "Thu", Received = 60, Issued = 70 },
            new DashboardStockMovementDto { Day = "Fri", Received = 96, Issued = 73 },
            new DashboardStockMovementDto { Day = "Sat", Received = 48, Issued = 38 },
            new DashboardStockMovementDto { Day = "Today", Received = 78, Issued = 60 }
        ]);

        _activities.AddRange([
            new DashboardActivityDto
            {
                Icon = "Truck",
                Title = "Dispatch DSP-1092 departed",
                Detail = "2,400 water units · North Ward",
                Time = "4 min"
            },
            new DashboardActivityDto
            {
                Icon = "GitBranch",
                Title = "Allocation AL-1048 approved",
                Detail = "18 trauma kits reserved",
                Time = "12 min"
            },
            new DashboardActivityDto
            {
                Icon = "AlertTriangle",
                Title = "Low-stock threshold reached",
                Detail = "Family Food Packs · North Hub",
                Time = "21 min"
            },
            new DashboardActivityDto
            {
                Icon = "Package",
                Title = "Stock receipt completed",
                Detail = "500 hygiene kits · West Depot",
                Time = "38 min"
            }
        ]);
    }
}
