namespace CareLink.Infrastructure.Repositories;

using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;

public class InMemoryResourceRequestRepository : IResourceRequestRepository
{
    private readonly List<ResourceRequest> _requests = [];
    private readonly Lock _lock = new();

    public InMemoryResourceRequestRepository()
    {
        SeedInitialData();
    }

    public Task<IEnumerable<ResourceRequest>> GetAllAsync(ResourceRequestFilterParams? filters = null)
    {
        lock (_lock)
        {
            IEnumerable<ResourceRequest> query = _requests.OrderByDescending(r => r.CreatedAt).ToList();

            if (filters != null)
            {
                if (!string.IsNullOrWhiteSpace(filters.Search))
                {
                    var s = filters.Search.Trim().ToLowerInvariant();
                    query = query.Where(r =>
                        r.Id.ToLowerInvariant().Contains(s) ||
                        r.Unit.ToLowerInvariant().Contains(s) ||
                        r.Resource.ToLowerInvariant().Contains(s) ||
                        r.Source.ToLowerInvariant().Contains(s) ||
                        r.Category.ToLowerInvariant().Contains(s)
                    );
                }

                if (!string.IsNullOrWhiteSpace(filters.Priority) &&
                    !filters.Priority.Equals("All priorities", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(r => r.Priority.Equals(filters.Priority, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrWhiteSpace(filters.Category) &&
                    !filters.Category.Equals("All categories", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(r => r.Category.Equals(filters.Category, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrWhiteSpace(filters.Requester) &&
                    !filters.Requester.Equals("All requesters", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(r => r.Unit.Equals(filters.Requester, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrWhiteSpace(filters.Status) &&
                    !filters.Status.Equals("All statuses", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(r => r.Status.Equals(filters.Status, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrWhiteSpace(filters.Tab))
                {
                    var tab = filters.Tab.Trim();
                    if (tab.Equals("Needs action", StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(r => r.Status.Equals("Awaiting Approval", StringComparison.OrdinalIgnoreCase) ||
                                                 r.Status.Equals("Partially Available", StringComparison.OrdinalIgnoreCase));
                    }
                    else if (tab.Equals("Active", StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(r => r.Status.Equals("Allocated", StringComparison.OrdinalIgnoreCase) ||
                                                 r.Status.Equals("Processing", StringComparison.OrdinalIgnoreCase));
                    }
                    else if (tab.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(r => r.Status.Equals("Fulfilled", StringComparison.OrdinalIgnoreCase));
                    }
                    else if (tab.Equals("Exceptions", StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(r => r.Status.Equals("Partially Available", StringComparison.OrdinalIgnoreCase));
                    }
                }
            }

            return Task.FromResult(query);
        }
    }

    public Task<ResourceRequest?> GetByIdAsync(string id)
    {
        lock (_lock)
        {
            var item = _requests.FirstOrDefault(r => r.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(item);
        }
    }

    public Task<ResourceRequest> AddAsync(ResourceRequest request)
    {
        lock (_lock)
        {
            if (string.IsNullOrWhiteSpace(request.Id))
            {
                var nextNum = 2842 + _requests.Count;
                request.Id = $"RR-{nextNum}";
            }

            request.CreatedAt = DateTime.UtcNow;
            _requests.Insert(0, request);
            return Task.FromResult(request);
        }
    }

    public Task<ResourceRequest?> UpdateStatusAsync(string id, string status, string? source = null)
    {
        lock (_lock)
        {
            var item = _requests.FirstOrDefault(r => r.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (item == null) return Task.FromResult<ResourceRequest?>(null);

            item.Status = status.Trim();
            if (!string.IsNullOrWhiteSpace(source))
            {
                item.Source = source.Trim();
            }

            return Task.FromResult<ResourceRequest?>(item);
        }
    }

    public Task<bool> DeleteAsync(string id)
    {
        lock (_lock)
        {
            var index = _requests.FindIndex(r => r.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (index == -1) return Task.FromResult(false);

            _requests.RemoveAt(index);
            return Task.FromResult(true);
        }
    }

    public Task<ResourceRequestSummaryDto> GetSummaryAsync()
    {
        lock (_lock)
        {
            var summary = new ResourceRequestSummaryDto
            {
                TotalCount = _requests.Count,
                OpenCount = _requests.Count(r => !r.Status.Equals("Fulfilled", StringComparison.OrdinalIgnoreCase)),
                AwaitingApprovalCount = _requests.Count(r => r.Status.Equals("Awaiting Approval", StringComparison.OrdinalIgnoreCase)),
                PartiallyAvailableCount = _requests.Count(r => r.Status.Equals("Partially Available", StringComparison.OrdinalIgnoreCase)),
                FulfilledTodayCount = _requests.Count(r => r.Status.Equals("Fulfilled", StringComparison.OrdinalIgnoreCase)),
                NeedsActionCount = _requests.Count(r => r.Status.Equals("Awaiting Approval", StringComparison.OrdinalIgnoreCase) ||
                                                       r.Status.Equals("Partially Available", StringComparison.OrdinalIgnoreCase))
            };

            return Task.FromResult(summary);
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
            },
            new ResourceRequest
            {
                Id = "RR-2821",
                Unit = "Riverton High Shelter",
                Resource = "Thermal Blanket",
                Quantity = "180 units",
                Priority = "Medium",
                Category = "Shelter",
                Source = "North Hub",
                Age = "2h 04m",
                Status = "Processing",
                CreatedAt = DateTime.UtcNow.AddMinutes(-124)
            },
            new ResourceRequest
            {
                Id = "RR-2818",
                Unit = "Emergency Medical Unit",
                Resource = "First Aid Kit",
                Quantity = "60 units",
                Priority = "High",
                Category = "Medical",
                Source = "Central Hub",
                Age = "2h 30m",
                Status = "Fulfilled",
                CreatedAt = DateTime.UtcNow.AddMinutes(-150)
            },
            new ResourceRequest
            {
                Id = "RR-2815",
                Unit = "South District Centre",
                Resource = "Hygiene Kit",
                Quantity = "300 units",
                Priority = "Medium",
                Category = "WASH",
                Source = "West Depot",
                Age = "3h 10m",
                Status = "Awaiting Approval",
                CreatedAt = DateTime.UtcNow.AddMinutes(-190)
            }
        ]);
    }
}
