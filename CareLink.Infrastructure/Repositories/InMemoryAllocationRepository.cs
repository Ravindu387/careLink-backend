namespace CareLink.Infrastructure.Repositories;

using System.Collections.Concurrent;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;

public class InMemoryAllocationRepository : IAllocationRepository
{
    private readonly ConcurrentDictionary<string, Allocation> _allocations = new(StringComparer.OrdinalIgnoreCase);
    private int _sequence = 1048;

    public InMemoryAllocationRepository()
    {
        SeedInitialAllocations();
    }

    public Task<IEnumerable<Allocation>> GetAllAsync()
    {
        var list = _allocations.Values.OrderByDescending(a => a.CreatedAt).AsEnumerable();
        return Task.FromResult(list);
    }

    public Task<Allocation?> GetByIdAsync(string id)
    {
        _allocations.TryGetValue(id, out var item);
        return Task.FromResult(item);
    }

    public Task<Allocation> CreateAsync(Allocation allocation)
    {
        if (string.IsNullOrWhiteSpace(allocation.Id))
        {
            var next = Interlocked.Increment(ref _sequence);
            allocation.Id = $"AL-{next}";
        }

        if (allocation.CreatedAt == default)
        {
            allocation.CreatedAt = DateTime.UtcNow;
        }

        _allocations[allocation.Id] = allocation;
        return Task.FromResult(allocation);
    }

    public Task<Allocation?> UpdateAsync(Allocation allocation)
    {
        if (_allocations.ContainsKey(allocation.Id))
        {
            _allocations[allocation.Id] = allocation;
            return Task.FromResult<Allocation?>(allocation);
        }
        return Task.FromResult<Allocation?>(null);
    }

    public Task<bool> DeleteAsync(string id)
    {
        return Task.FromResult(_allocations.TryRemove(id, out _));
    }

    private void SeedInitialAllocations()
    {
        var seeds = new List<Allocation>
        {
            new()
            {
                Id = "AL-1048",
                Request = "RR-2839",
                Resource = "Trauma Medical Kit",
                Quantity = "18 / 24",
                Source = "Central Hub",
                Destination = "Medical Team 3",
                ApprovedBy = "Daniel Perera",
                ApprovedAt = "22 Jun - 13:42",
                Status = "Partially Allocated",
                Warehouse = "Central Hub",
                CreatedAt = DateTime.UtcNow.AddHours(-1)
            },
            new()
            {
                Id = "AL-1044",
                Request = "RR-2828",
                Resource = "Fuel - Diesel",
                Quantity = "1,200 L",
                Source = "West Depot",
                Destination = "Mobility Team",
                ApprovedBy = "Daniel Perera",
                ApprovedAt = "22 Jun - 12:55",
                Status = "Allocated",
                Warehouse = "West Depot",
                CreatedAt = DateTime.UtcNow.AddHours(-2)
            },
            new()
            {
                Id = "AL-1039",
                Request = "RR-2812",
                Resource = "Shelter Kit",
                Quantity = "80 units",
                Source = "Eastbank",
                Destination = "CareLink Network",
                ApprovedBy = "Maya Silva",
                ApprovedAt = "22 Jun - 11:18",
                Status = "Released",
                Warehouse = "Eastbank",
                CreatedAt = DateTime.UtcNow.AddHours(-3)
            },
            new()
            {
                Id = "AL-1034",
                Request = "RR-2801",
                Resource = "Drinking Water",
                Quantity = "3,000 units",
                Source = "Central Hub",
                Destination = "North Ward",
                ApprovedBy = "Daniel Perera",
                ApprovedAt = "22 Jun - 09:20",
                Status = "Completed",
                Warehouse = "Central Hub",
                CreatedAt = DateTime.UtcNow.AddHours(-5)
            },
            new()
            {
                Id = "AL-1031",
                Request = "RR-2798",
                Resource = "Family Food Pack",
                Quantity = "500 units",
                Source = "North Hub",
                Destination = "Eastbank Community",
                ApprovedBy = "Maya Silva",
                ApprovedAt = "21 Jun - 16:30",
                Status = "Pending",
                Warehouse = "North Hub",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new()
            {
                Id = "AL-1028",
                Request = "RR-2791",
                Resource = "Thermal Blanket",
                Quantity = "200 units",
                Source = "West Depot",
                Destination = "Riverton Shelter",
                ApprovedBy = "Daniel Perera",
                ApprovedAt = "21 Jun - 14:10",
                Status = "Allocated",
                Warehouse = "West Depot",
                CreatedAt = DateTime.UtcNow.AddDays(-1).AddHours(-2)
            },
            new()
            {
                Id = "AL-1025",
                Request = "RR-2785",
                Resource = "Hygiene Kit",
                Quantity = "150 units",
                Source = "Eastbank",
                Destination = "South District",
                ApprovedBy = "Maya Silva",
                ApprovedAt = "21 Jun - 10:45",
                Status = "Completed",
                Warehouse = "Eastbank",
                CreatedAt = DateTime.UtcNow.AddDays(-1).AddHours(-5)
            }
        };

        foreach (var s in seeds)
        {
            _allocations[s.Id] = s;
        }
    }
}
