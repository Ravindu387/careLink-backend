namespace CareLink.Infrastructure.Repositories;

using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;

public class InMemoryStockTransferRepository : IStockTransferRepository
{
    private readonly ConcurrentDictionary<string, StockTransfer> _transfers = new();
    private readonly Lock _lock = new();

    public InMemoryStockTransferRepository()
    {
        SeedInitialData();
    }

    public Task<IEnumerable<StockTransfer>> GetAllAsync()
    {
        var list = _transfers.Values.OrderByDescending(t => t.CreatedAt).AsEnumerable();
        return Task.FromResult(list);
    }

    public Task<StockTransfer?> GetByIdAsync(string id)
    {
        _transfers.TryGetValue(id, out var transfer);
        return Task.FromResult(transfer);
    }

    public Task<StockTransfer> CreateAsync(StockTransfer transfer)
    {
        lock (_lock)
        {
            if (string.IsNullOrWhiteSpace(transfer.Id))
            {
                transfer.Id = GenerateNextId();
            }

            transfer.CreatedAt = DateTime.UtcNow;
            _transfers[transfer.Id] = transfer;
            return Task.FromResult(transfer);
        }
    }

    public Task<StockTransfer?> UpdateAsync(StockTransfer transfer)
    {
        lock (_lock)
        {
            if (!_transfers.ContainsKey(transfer.Id))
            {
                return Task.FromResult<StockTransfer?>(null);
            }

            transfer.UpdatedAt = DateTime.UtcNow;
            _transfers[transfer.Id] = transfer;
            return Task.FromResult<StockTransfer?>(transfer);
        }
    }

    public Task<bool> DeleteAsync(string id)
    {
        return Task.FromResult(_transfers.TryRemove(id, out _));
    }

    private string GenerateNextId()
    {
        var maxNumber = 581;
        foreach (var key in _transfers.Keys)
        {
            var match = Regex.Match(key, @"\d+");
            if (match.Success && int.TryParse(match.Value, out var num))
            {
                if (num > maxNumber) maxNumber = num;
            }
        }

        return $"TR-{(maxNumber + 1):D4}";
    }

    private void SeedInitialData()
    {
        var seeds = new List<StockTransfer>
        {
            new()
            {
                Id = "TR-0581",
                Source = "Central Logistics Hub",
                Destination = "North District Hub",
                Resource = "Drinking Water",
                Quantity = "4,000 units",
                RequestedBy = "Daniel Perera",
                Scheduled = "22 Jun · 15:30",
                Status = "Approved",
                CreatedAt = DateTime.UtcNow.AddHours(-1)
            },
            new()
            {
                Id = "TR-0578",
                Source = "West Coast Depot",
                Destination = "Eastbank Warehouse",
                Resource = "Hygiene Kits",
                Quantity = "500 units",
                RequestedBy = "Maya Silva",
                Scheduled = "22 Jun · 14:10",
                Status = "In Transit",
                CreatedAt = DateTime.UtcNow.AddHours(-3)
            },
            new()
            {
                Id = "TR-0572",
                Source = "Central Logistics Hub",
                Destination = "West Coast Depot",
                Resource = "Trauma Kits",
                Quantity = "20 units",
                RequestedBy = "Daniel Perera",
                Scheduled = "21 Jun · 17:42",
                Status = "Completed",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new()
            {
                Id = "TR-0564",
                Source = "North District Hub",
                Destination = "Central Logistics Hub",
                Resource = "Blankets",
                Quantity = "100 units",
                RequestedBy = "R. Khan",
                Scheduled = "21 Jun · 12:18",
                Status = "Rejected",
                CreatedAt = DateTime.UtcNow.AddDays(-1).AddHours(-4)
            },
            new()
            {
                Id = "TR-0560",
                Source = "Eastbank Warehouse",
                Destination = "North District Hub",
                Resource = "Family Food Packs",
                Quantity = "300 units",
                RequestedBy = "Maya Silva",
                Scheduled = "23 Jun · 09:00",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new()
            {
                Id = "TR-0558",
                Source = "West Coast Depot",
                Destination = "Central Logistics Hub",
                Resource = "Medical Kits",
                Quantity = "80 units",
                RequestedBy = "Daniel Perera",
                Scheduled = "23 Jun · 11:30",
                Status = "Approved",
                CreatedAt = DateTime.UtcNow.AddDays(-2).AddHours(-2)
            },
            new()
            {
                Id = "TR-0552",
                Source = "North District Hub",
                Destination = "Eastbank Warehouse",
                Resource = "Shelter Kits",
                Quantity = "150 units",
                RequestedBy = "R. Khan",
                Scheduled = "20 Jun · 14:00",
                Status = "Completed",
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            }
        };

        foreach (var item in seeds)
        {
            _transfers[item.Id] = item;
        }
    }
}
