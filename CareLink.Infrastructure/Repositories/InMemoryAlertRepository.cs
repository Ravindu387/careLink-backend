namespace CareLink.Infrastructure.Repositories;

using System.Collections.Concurrent;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;

public class InMemoryAlertRepository : IAlertRepository
{
    private readonly ConcurrentDictionary<int, Alert> _alerts = new();
    private AlertRuleSettings _rules = new();
    private int _nextId = 1;

    public InMemoryAlertRepository()
    {
        SeedInitialAlerts();
    }

    public Task<IEnumerable<Alert>> GetAllAsync()
    {
        var list = _alerts.Values.OrderByDescending(a => a.CreatedAt).AsEnumerable();
        return Task.FromResult(list);
    }

    public Task<Alert?> GetByIdAsync(int id)
    {
        _alerts.TryGetValue(id, out var alert);
        return Task.FromResult(alert);
    }

    public Task<Alert> CreateAsync(Alert alert)
    {
        var id = Interlocked.Increment(ref _nextId);
        alert.Id = id;
        if (alert.CreatedAt == default)
        {
            alert.CreatedAt = DateTime.UtcNow;
        }
        _alerts[id] = alert;
        return Task.FromResult(alert);
    }

    public Task<Alert?> UpdateAsync(Alert alert)
    {
        if (_alerts.ContainsKey(alert.Id))
        {
            _alerts[alert.Id] = alert;
            return Task.FromResult<Alert?>(alert);
        }
        return Task.FromResult<Alert?>(null);
    }

    public Task<int> MarkAllOpenAsAcknowledgedAsync()
    {
        var count = 0;
        foreach (var alert in _alerts.Values)
        {
            if (string.Equals(alert.Status, "Open", StringComparison.OrdinalIgnoreCase))
            {
                alert.Status = "Acknowledged";
                alert.AcknowledgedAt = DateTime.UtcNow;
                count++;
            }
        }
        return Task.FromResult(count);
    }

    public Task<AlertRuleSettings> GetRulesAsync()
    {
        return Task.FromResult(_rules);
    }

    public Task<AlertRuleSettings> UpdateRulesAsync(AlertRuleSettings rules)
    {
        _rules = rules;
        return Task.FromResult(_rules);
    }

    private void SeedInitialAlerts()
    {
        var seeds = new List<Alert>
        {
            new()
            {
                Id = 1,
                Title = "Critical stock shortage · Trauma Medical Kits",
                Description = "Central Logistics Hub · 4 units available · threshold 20",
                Location = "Central Logistics Hub",
                Time = "8 min ago",
                Severity = "Critical",
                Status = "Open",
                Icon = "alert",
                CreatedAt = DateTime.UtcNow.AddMinutes(-8)
            },
            new()
            {
                Id = 2,
                Title = "Dispatch delayed · DSP-1089 · Fuel delivery",
                Description = "West Depot → Central Ward · ETA delayed by 42 minutes",
                Location = "West Depot",
                Time = "18 min ago",
                Severity = "Delayed",
                Status = "Open",
                Icon = "truck",
                CreatedAt = DateTime.UtcNow.AddMinutes(-18)
            },
            new()
            {
                Id = 3,
                Title = "Low stock threshold · Family Food Packs",
                Description = "North District Hub · 94 units available · 500 requested",
                Location = "North District Hub",
                Time = "26 min ago",
                Severity = "Low Stock",
                Status = "Open",
                Icon = "alert",
                CreatedAt = DateTime.UtcNow.AddMinutes(-26)
            },
            new()
            {
                Id = 4,
                Title = "Warehouse capacity warning · North District Hub",
                Description = "Storage utilization reached 91% · Rebalancing recommended",
                Location = "North District Hub",
                Time = "35 min ago",
                Severity = "Near Capacity",
                Status = "Open",
                Icon = "warehouse",
                CreatedAt = DateTime.UtcNow.AddMinutes(-35)
            },
            new()
            {
                Id = 5,
                Title = "Expiring supplies · Medical Stock",
                Description = "Medical supplies expiring within the next 7 days",
                Location = "Central Logistics Hub",
                Time = "42 min ago",
                Severity = "Expiring Soon",
                Status = "Acknowledged",
                Icon = "clock",
                CreatedAt = DateTime.UtcNow.AddMinutes(-42),
                AcknowledgedAt = DateTime.UtcNow.AddMinutes(-20)
            },
            new()
            {
                Id = 6,
                Title = "Procurement approval pending · PO-2025-174",
                Description = "Shelter kits · Awaiting manager approval",
                Location = "Central Logistics Hub",
                Time = "1 hour ago",
                Severity = "Warning",
                Status = "Open",
                Icon = "alert",
                CreatedAt = DateTime.UtcNow.AddHours(-1)
            },
            new()
            {
                Id = 7,
                Title = "Fuel stock below recommended level",
                Description = "West Coast Depot · Replenishment required",
                Location = "West Coast Depot",
                Time = "2 hours ago",
                Severity = "Low Stock",
                Status = "Resolved",
                Icon = "alert",
                CreatedAt = DateTime.UtcNow.AddHours(-2),
                ResolvedAt = DateTime.UtcNow.AddHours(-1)
            },
            new()
            {
                Id = 8,
                Title = "Delivery route delay · DSP-1092",
                Description = "North Ward · Delivery schedule updated",
                Location = "North Ward",
                Time = "3 hours ago",
                Severity = "Delayed",
                Status = "Acknowledged",
                Icon = "truck",
                CreatedAt = DateTime.UtcNow.AddHours(-3),
                AcknowledgedAt = DateTime.UtcNow.AddHours(-2)
            },
            new()
            {
                Id = 9,
                Title = "Warehouse temperature warning",
                Description = "Eastbank Warehouse · Temperature requires inspection",
                Location = "Eastbank Warehouse",
                Time = "4 hours ago",
                Severity = "Warning",
                Status = "Resolved",
                Icon = "warehouse",
                CreatedAt = DateTime.UtcNow.AddHours(-4),
                ResolvedAt = DateTime.UtcNow.AddHours(-2)
            }
        };

        foreach (var s in seeds)
        {
            _alerts[s.Id] = s;
        }

        _nextId = 9;
    }
}
