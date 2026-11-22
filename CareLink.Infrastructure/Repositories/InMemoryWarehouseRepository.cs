namespace CareLink.Infrastructure.Repositories;

using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;

public class InMemoryWarehouseRepository : IWarehouseRepository
{
    private readonly ConcurrentDictionary<string, Warehouse> _warehouses = new();
    private readonly Lock _lock = new();

    public InMemoryWarehouseRepository()
    {
        SeedInitialData();
    }

    public Task<IEnumerable<Warehouse>> GetAllAsync()
    {
        var list = _warehouses.Values.OrderByDescending(w => w.CreatedAt).AsEnumerable();
        return Task.FromResult(list);
    }

    public Task<Warehouse?> GetByIdAsync(string id)
    {
        _warehouses.TryGetValue(id, out var warehouse);
        return Task.FromResult(warehouse);
    }

    public Task<Warehouse> CreateAsync(Warehouse warehouse)
    {
        lock (_lock)
        {
            if (string.IsNullOrWhiteSpace(warehouse.Id))
            {
                warehouse.Id = GenerateNextId();
            }

            warehouse.CreatedAt = DateTime.UtcNow;
            _warehouses[warehouse.Id] = warehouse;
            return Task.FromResult(warehouse);
        }
    }

    public Task<Warehouse?> UpdateAsync(Warehouse warehouse)
    {
        lock (_lock)
        {
            if (!_warehouses.ContainsKey(warehouse.Id))
            {
                return Task.FromResult<Warehouse?>(null);
            }

            warehouse.UpdatedAt = DateTime.UtcNow;
            _warehouses[warehouse.Id] = warehouse;
            return Task.FromResult<Warehouse?>(warehouse);
        }
    }

    public Task<bool> DeleteAsync(string id)
    {
        return Task.FromResult(_warehouses.TryRemove(id, out _));
    }

    private string GenerateNextId()
    {
        var maxNumber = 25;
        foreach (var key in _warehouses.Keys)
        {
            var match = Regex.Match(key, @"\d+");
            if (match.Success && int.TryParse(match.Value, out var num))
            {
                if (num > maxNumber) maxNumber = num;
            }
        }

        return $"WH-{(maxNumber + 1):D3}";
    }

    private void SeedInitialData()
    {
        var seeds = new List<Warehouse>
        {
            new()
            {
                Id = "WH-001",
                Name = "Central Logistics Hub",
                Location = "Central Ward",
                Capacity = 8420,
                Utilisation = 78,
                StockLines = 1284,
                Staff = 24,
                Status = "Operational",
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new()
            {
                Id = "WH-004",
                Name = "North District Hub",
                Location = "North District",
                Capacity = 4800,
                Utilisation = 91,
                StockLines = 684,
                Staff = 16,
                Status = "Near Capacity",
                CreatedAt = DateTime.UtcNow.AddDays(-9)
            },
            new()
            {
                Id = "WH-007",
                Name = "Eastbank Warehouse",
                Location = "Eastbank",
                Capacity = 3600,
                Utilisation = 62,
                StockLines = 342,
                Staff = 11,
                Status = "Operational",
                CreatedAt = DateTime.UtcNow.AddDays(-8)
            },
            new()
            {
                Id = "WH-012",
                Name = "West Coast Depot",
                Location = "West District",
                Capacity = 5200,
                Utilisation = 48,
                StockLines = 520,
                Staff = 14,
                Status = "Operational",
                CreatedAt = DateTime.UtcNow.AddDays(-7)
            },
            new()
            {
                Id = "WH-015",
                Name = "South Emergency Store",
                Location = "South District",
                Capacity = 2800,
                Utilisation = 85,
                StockLines = 410,
                Staff = 9,
                Status = "Near Capacity",
                CreatedAt = DateTime.UtcNow.AddDays(-6)
            },
            new()
            {
                Id = "WH-018",
                Name = "Riverside Storage",
                Location = "Riverside",
                Capacity = 4200,
                Utilisation = 55,
                StockLines = 360,
                Staff = 12,
                Status = "Operational",
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new()
            {
                Id = "WH-021",
                Name = "Emergency Medical Store",
                Location = "Central Ward",
                Capacity = 1900,
                Utilisation = 35,
                StockLines = 220,
                Staff = 8,
                Status = "Under Maintenance",
                CreatedAt = DateTime.UtcNow.AddDays(-4)
            },
            new()
            {
                Id = "WH-025",
                Name = "Eastern Relief Centre",
                Location = "East District",
                Capacity = 3100,
                Utilisation = 67,
                StockLines = 290,
                Staff = 10,
                Status = "Operational",
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            }
        };

        foreach (var item in seeds)
        {
            _warehouses[item.Id] = item;
        }
    }
}
