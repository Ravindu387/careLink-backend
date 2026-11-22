namespace CareLink.Infrastructure.Repositories;

using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;

public class InMemorySupplierRepository : ISupplierRepository
{
    private readonly ConcurrentDictionary<string, Supplier> _suppliers = new();
    private readonly Lock _lock = new();

    public InMemorySupplierRepository()
    {
        SeedInitialData();
    }

    public Task<IEnumerable<Supplier>> GetAllAsync()
    {
        var list = _suppliers.Values.OrderByDescending(s => s.CreatedAt).AsEnumerable();
        return Task.FromResult(list);
    }

    public Task<Supplier?> GetByIdAsync(string id)
    {
        _suppliers.TryGetValue(id, out var supplier);
        return Task.FromResult(supplier);
    }

    public Task<Supplier> CreateAsync(Supplier supplier)
    {
        lock (_lock)
        {
            if (string.IsNullOrWhiteSpace(supplier.Id))
            {
                supplier.Id = GenerateNextId();
            }

            supplier.CreatedAt = DateTime.UtcNow;
            _suppliers[supplier.Id] = supplier;
            return Task.FromResult(supplier);
        }
    }

    public Task<Supplier?> UpdateAsync(Supplier supplier)
    {
        lock (_lock)
        {
            if (!_suppliers.ContainsKey(supplier.Id))
            {
                return Task.FromResult<Supplier?>(null);
            }

            supplier.UpdatedAt = DateTime.UtcNow;
            _suppliers[supplier.Id] = supplier;
            return Task.FromResult<Supplier?>(supplier);
        }
    }

    public Task<bool> DeleteAsync(string id)
    {
        return Task.FromResult(_suppliers.TryRemove(id, out _));
    }

    private string GenerateNextId()
    {
        var maxNumber = 65;
        foreach (var key in _suppliers.Keys)
        {
            var match = Regex.Match(key, @"\d+");
            if (match.Success && int.TryParse(match.Value, out var num))
            {
                if (num > maxNumber) maxNumber = num;
            }
        }

        return $"SUP-{(maxNumber + 1):D4}";
    }

    private void SeedInitialData()
    {
        var seeds = new List<Supplier>
        {
            new()
            {
                Id = "SUP-0014",
                Name = "MediSupply International",
                Category = "Medical",
                Coverage = "National",
                OnTimeRate = "98%",
                LeadTime = "2.1 days",
                Verification = "Verified",
                Status = "Active",
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new()
            {
                Id = "SUP-0021",
                Name = "AquaSafe Ltd.",
                Category = "Food & Water",
                Coverage = "Central - North",
                OnTimeRate = "96%",
                LeadTime = "1.4 days",
                Verification = "Verified",
                Status = "Active",
                CreatedAt = DateTime.UtcNow.AddDays(-9)
            },
            new()
            {
                Id = "SUP-0032",
                Name = "ShelterWorks Cooperative",
                Category = "Shelter",
                Coverage = "All districts",
                OnTimeRate = "91%",
                LeadTime = "3.2 days",
                Verification = "Verified",
                Status = "Active",
                CreatedAt = DateTime.UtcNow.AddDays(-8)
            },
            new()
            {
                Id = "SUP-0048",
                Name = "Regional Food Services",
                Category = "Food & Water",
                Coverage = "Central Region",
                OnTimeRate = "94%",
                LeadTime = "1.8 days",
                Verification = "Verified",
                Status = "Active",
                CreatedAt = DateTime.UtcNow.AddDays(-7)
            },
            new()
            {
                Id = "SUP-0052",
                Name = "HealthCare Essentials",
                Category = "Medical",
                Coverage = "North Region",
                OnTimeRate = "88%",
                LeadTime = "2.8 days",
                Verification = "Pending",
                Status = "Active",
                CreatedAt = DateTime.UtcNow.AddDays(-6)
            },
            new()
            {
                Id = "SUP-0057",
                Name = "SafeHome Supplies",
                Category = "Shelter",
                Coverage = "West Region",
                OnTimeRate = "85%",
                LeadTime = "4.2 days",
                Verification = "Pending",
                Status = "Inactive",
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new()
            {
                Id = "SUP-0061",
                Name = "CleanWater Solutions",
                Category = "WASH",
                Coverage = "All districts",
                OnTimeRate = "97%",
                LeadTime = "1.5 days",
                Verification = "Verified",
                Status = "Active",
                CreatedAt = DateTime.UtcNow.AddDays(-4)
            },
            new()
            {
                Id = "SUP-0065",
                Name = "Emergency Logistics Ltd.",
                Category = "Transport",
                Coverage = "Central Region",
                OnTimeRate = "90%",
                LeadTime = "2.5 days",
                Verification = "Rejected",
                Status = "Suspended",
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            }
        };

        foreach (var item in seeds)
        {
            _suppliers[item.Id] = item;
        }
    }
}
