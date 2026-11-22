namespace CareLink.Infrastructure.Repositories;

using System.Text.RegularExpressions;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;

public class InMemoryInventoryRepository : IInventoryRepository
{
    private readonly List<InventoryItem> _items = [];
    private readonly Lock _lock = new();

    public InMemoryInventoryRepository()
    {
        SeedInitialData();
    }

    public Task<IEnumerable<InventoryItem>> GetAllAsync(InventoryFilterParams? filters = null)
    {
        lock (_lock)
        {
            IEnumerable<InventoryItem> query = _items.OrderByDescending(x => x.CreatedAt).ToList();

            if (filters != null)
            {
                if (!string.IsNullOrWhiteSpace(filters.Search))
                {
                    var s = filters.Search.Trim().ToLowerInvariant();
                    query = query.Where(x =>
                        x.Id.ToLowerInvariant().Contains(s) ||
                        x.Resource.ToLowerInvariant().Contains(s) ||
                        x.Category.ToLowerInvariant().Contains(s) ||
                        x.Warehouse.ToLowerInvariant().Contains(s)
                    );
                }

                if (!string.IsNullOrWhiteSpace(filters.Category) &&
                    !filters.Category.Equals("All categories", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(x => x.Category.Equals(filters.Category, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrWhiteSpace(filters.Warehouse) &&
                    !filters.Warehouse.Equals("All warehouses", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(x => x.Warehouse.Equals(filters.Warehouse, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrWhiteSpace(filters.Status) &&
                    !filters.Status.Equals("All stock states", StringComparison.OrdinalIgnoreCase) &&
                    !filters.Status.Equals("All", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(x => x.Status.Equals(filters.Status, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrWhiteSpace(filters.Tab))
                {
                    var tab = filters.Tab.Trim();
                    if (tab.Equals("Needs action", StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(x => !x.Status.Equals("Healthy", StringComparison.OrdinalIgnoreCase));
                    }
                    else if (tab.Equals("Active", StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(x => x.Status.Equals("Healthy", StringComparison.OrdinalIgnoreCase));
                    }
                    else if (tab.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(x => x.Available > 0);
                    }
                    else if (tab.Equals("Exceptions", StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(x => x.Status.Equals("Critical", StringComparison.OrdinalIgnoreCase));
                    }
                }
            }

            return Task.FromResult(query);
        }
    }

    public Task<InventoryItem?> GetByIdAsync(string id)
    {
        lock (_lock)
        {
            var item = _items.FirstOrDefault(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(item);
        }
    }

    public Task<InventoryItem> AddAsync(InventoryItem item)
    {
        lock (_lock)
        {
            if (string.IsNullOrWhiteSpace(item.Id))
            {
                var nextNumber = 920;
                var maxId = _items
                    .Select(d => Regex.Match(d.Id, @"\d+").Value)
                    .Where(v => !string.IsNullOrEmpty(v))
                    .Select(int.Parse)
                    .DefaultIfEmpty(915)
                    .Max();

                if (maxId > 0)
                {
                    nextNumber = maxId + 1;
                }
                item.Id = $"RES-{nextNumber:D5}";
            }

            item.CreatedAt = DateTime.UtcNow;
            _items.Insert(0, item);
            return Task.FromResult(item);
        }
    }

    public Task<InventoryItem?> UpdateAsync(string id, UpdateInventoryItemRequest request)
    {
        lock (_lock)
        {
            var item = _items.FirstOrDefault(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (item == null)
            {
                return Task.FromResult<InventoryItem?>(null);
            }

            if (!string.IsNullOrWhiteSpace(request.Resource)) item.Resource = request.Resource.Trim();
            if (!string.IsNullOrWhiteSpace(request.Category)) item.Category = request.Category.Trim();
            if (!string.IsNullOrWhiteSpace(request.Warehouse)) item.Warehouse = request.Warehouse.Trim();
            if (!string.IsNullOrWhiteSpace(request.Unit)) item.Unit = request.Unit.Trim();
            if (request.OnHand.HasValue) item.OnHand = Math.Max(0, request.OnHand.Value);
            if (request.Reserved.HasValue) item.Reserved = Math.Max(0, request.Reserved.Value);
            if (request.InTransit.HasValue) item.InTransit = Math.Max(0, request.InTransit.Value);

            item.Available = Math.Max(0, item.OnHand - item.Reserved);

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                item.Status = request.Status.Trim();
            }
            else
            {
                item.Status = item.Available <= 10 ? "Critical" : item.Available <= 100 ? "Low Stock" : "Healthy";
            }

            item.UpdatedAt = DateTime.UtcNow;
            return Task.FromResult<InventoryItem?>(item);
        }
    }

    public Task<InventoryItem?> AdjustStockAsync(string id, int delta)
    {
        lock (_lock)
        {
            var item = _items.FirstOrDefault(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (item == null)
            {
                return Task.FromResult<InventoryItem?>(null);
            }

            item.OnHand = Math.Max(0, item.OnHand + delta);
            item.Available = Math.Max(0, item.OnHand - item.Reserved);
            item.Status = item.Available <= 10 ? "Critical" : item.Available <= 100 ? "Low Stock" : "Healthy";
            item.UpdatedAt = DateTime.UtcNow;

            return Task.FromResult<InventoryItem?>(item);
        }
    }

    public Task<bool> DeleteAsync(string id)
    {
        lock (_lock)
        {
            var index = _items.FindIndex(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (index == -1)
            {
                return Task.FromResult(false);
            }
            _items.RemoveAt(index);
            return Task.FromResult(true);
        }
    }

    public Task<InventorySummaryDto> GetSummaryAsync()
    {
        lock (_lock)
        {
            var summary = new InventorySummaryDto
            {
                TotalSkus = _items.Count,
                LowStock = _items.Count(x => x.Status.Equals("Low Stock", StringComparison.OrdinalIgnoreCase)),
                OutOfStock = _items.Count(x => x.Status.Equals("Critical", StringComparison.OrdinalIgnoreCase) || x.Available == 0),
                ExpiringSoon = 42,
                TotalAvailable = _items.Sum(x => x.Available)
            };

            return Task.FromResult(summary);
        }
    }

    private void SeedInitialData()
    {
        _items.AddRange([
            new InventoryItem
            {
                Id = "RES-00128",
                Resource = "Drinking Water - 10L",
                Category = "Food & Water",
                Warehouse = "Central Logistics Hub",
                OnHand = 8420,
                Reserved = 3100,
                InTransit = 2500,
                Available = 2820,
                Status = "Healthy",
                Unit = "bottles",
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new InventoryItem
            {
                Id = "RES-00241",
                Resource = "Family Food Pack",
                Category = "Food & Water",
                Warehouse = "North District Hub",
                OnHand = 1284,
                Reserved = 940,
                InTransit = 250,
                Available = 94,
                Status = "Low Stock",
                Unit = "packs",
                CreatedAt = DateTime.UtcNow.AddDays(-9)
            },
            new InventoryItem
            {
                Id = "RES-00314",
                Resource = "Emergency Shelter Kit",
                Category = "Shelter",
                Warehouse = "Eastbank Warehouse",
                OnHand = 342,
                Reserved = 218,
                InTransit = 76,
                Available = 48,
                Status = "Healthy",
                Unit = "kits",
                CreatedAt = DateTime.UtcNow.AddDays(-8)
            },
            new InventoryItem
            {
                Id = "RES-00402",
                Resource = "Trauma Medical Kit",
                Category = "Medical",
                Warehouse = "Central Logistics Hub",
                OnHand = 86,
                Reserved = 64,
                InTransit = 18,
                Available = 4,
                Status = "Critical",
                Unit = "kits",
                CreatedAt = DateTime.UtcNow.AddDays(-7)
            },
            new InventoryItem
            {
                Id = "RES-00518",
                Resource = "Hygiene Kit",
                Category = "WASH",
                Warehouse = "West Coast Depot",
                OnHand = 2450,
                Reserved = 1220,
                InTransit = 600,
                Available = 630,
                Status = "Healthy",
                Unit = "kits",
                CreatedAt = DateTime.UtcNow.AddDays(-6)
            },
            new InventoryItem
            {
                Id = "RES-00610",
                Resource = "Thermal Blanket",
                Category = "Non-food Item",
                Warehouse = "North District Hub",
                OnHand = 520,
                Reserved = 412,
                InTransit = 90,
                Available = 18,
                Status = "Low Stock",
                Unit = "units",
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new InventoryItem
            {
                Id = "RES-00721",
                Resource = "Diesel Fuel",
                Category = "Transport & Fuel",
                Warehouse = "West Coast Depot",
                OnHand = 1800,
                Reserved = 1500,
                InTransit = 100,
                Available = 200,
                Status = "Critical",
                Unit = "L",
                CreatedAt = DateTime.UtcNow.AddDays(-4)
            },
            new InventoryItem
            {
                Id = "RES-00832",
                Resource = "Emergency Tent",
                Category = "Shelter",
                Warehouse = "Eastbank Warehouse",
                OnHand = 680,
                Reserved = 210,
                InTransit = 100,
                Available = 370,
                Status = "Healthy",
                Unit = "units",
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new InventoryItem
            {
                Id = "RES-00915",
                Resource = "First Aid Kit",
                Category = "Medical",
                Warehouse = "Central Logistics Hub",
                OnHand = 340,
                Reserved = 180,
                InTransit = 40,
                Available = 120,
                Status = "Healthy",
                Unit = "kits",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            }
        ]);
    }
}
