namespace CareLink.Infrastructure.Repositories;

using System.Collections.Concurrent;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;

public class InMemoryAuditLogRepository : IAuditLogRepository
{
    private readonly List<AuditLog> _logs = [];
    private readonly Lock _lock = new();

    public InMemoryAuditLogRepository()
    {
        SeedInitialData();
    }

    public Task<IEnumerable<AuditLog>> GetAllAsync(AuditLogFilterParams? filters = null)
    {
        lock (_lock)
        {
            IEnumerable<AuditLog> query = _logs.OrderByDescending(x => x.CreatedAt).ToList();

            if (filters != null)
            {
                if (!string.IsNullOrWhiteSpace(filters.Search))
                {
                    var s = filters.Search.Trim().ToLowerInvariant();
                    query = query.Where(x =>
                        x.Id.ToLowerInvariant().Contains(s) ||
                        x.Action.ToLowerInvariant().Contains(s) ||
                        x.Actor.ToLowerInvariant().Contains(s) ||
                        x.Record.ToLowerInvariant().Contains(s) ||
                        x.Change.ToLowerInvariant().Contains(s) ||
                        x.Rationale.ToLowerInvariant().Contains(s)
                    );
                }

                if (!string.IsNullOrWhiteSpace(filters.ActionType) &&
                    !filters.ActionType.Equals("All action types", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(x => x.ActionType.Equals(filters.ActionType, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrWhiteSpace(filters.Actor) &&
                    !filters.Actor.Equals("All users", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(x => x.Actor.Equals(filters.Actor, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrWhiteSpace(filters.RecordType) &&
                    !filters.RecordType.Equals("All record types", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(x => x.RecordType.Equals(filters.RecordType, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrWhiteSpace(filters.DateFilter) &&
                    !filters.DateFilter.Equals("All dates", StringComparison.OrdinalIgnoreCase))
                {
                    var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
                    if (filters.DateFilter.Equals("Today", StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(x => x.Date == today || x.Date == "2025-06-22");
                    }
                    else if (filters.DateFilter.Equals("Last 7 days", StringComparison.OrdinalIgnoreCase))
                    {
                        var validDates = new HashSet<string> { "2025-06-22", "2025-06-21", "2025-06-20", today };
                        query = query.Where(x => validDates.Contains(x.Date));
                    }
                }
            }

            return Task.FromResult(query);
        }
    }

    public Task<AuditLog?> GetByIdAsync(string id)
    {
        lock (_lock)
        {
            var log = _logs.FirstOrDefault(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(log);
        }
    }

    public Task<AuditLog> AddAsync(AuditLog log)
    {
        lock (_lock)
        {
            _logs.Insert(0, log);
            return Task.FromResult(log);
        }
    }

    public Task<AuditLogSummaryDto> GetSummaryAsync()
    {
        lock (_lock)
        {
            var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
            var summary = new AuditLogSummaryDto
            {
                TotalLogs = _logs.Count,
                ApprovalsCount = _logs.Count(x => x.ActionType.Equals("Approval", StringComparison.OrdinalIgnoreCase)),
                DispatchesCount = _logs.Count(x => x.ActionType.Equals("Dispatch", StringComparison.OrdinalIgnoreCase)),
                InventoryCount = _logs.Count(x => x.ActionType.Equals("Inventory", StringComparison.OrdinalIgnoreCase)),
                TodayCount = _logs.Count(x => x.Date == today || x.Date == "2025-06-22")
            };

            return Task.FromResult(summary);
        }
    }

    private void SeedInitialData()
    {
        _logs.AddRange([
            new AuditLog
            {
                Id = "AUD-482194",
                Action = "Allocation approved",
                Actor = "Daniel Perera",
                Timestamp = "22 Jun · 13:51",
                Record = "AL-1048",
                Change = "18 trauma kits reserved",
                Rationale = "Emergency medical request",
                ActionType = "Approval",
                RecordType = "Allocation",
                Date = "2025-06-22",
                CreatedAt = DateTime.UtcNow.AddMinutes(-30)
            },
            new AuditLog
            {
                Id = "AUD-482188",
                Action = "Dispatch released",
                Actor = "Daniel Perera",
                Timestamp = "22 Jun · 13:28",
                Record = "DSP-1092",
                Change = "Pending → In Transit",
                Rationale = "Priority water shortage",
                ActionType = "Dispatch",
                RecordType = "Dispatch",
                Date = "2025-06-22",
                CreatedAt = DateTime.UtcNow.AddMinutes(-50)
            },
            new AuditLog
            {
                Id = "AUD-482174",
                Action = "Stock adjustment",
                Actor = "Maya Silva",
                Timestamp = "22 Jun · 12:52",
                Record = "RES-00402",
                Change = "88 → 86 units",
                Rationale = "Cycle count correction",
                ActionType = "Inventory",
                RecordType = "Resource",
                Date = "2025-06-22",
                CreatedAt = DateTime.UtcNow.AddHours(-1)
            },
            new AuditLog
            {
                Id = "AUD-482161",
                Action = "Transfer approved",
                Actor = "Daniel Perera",
                Timestamp = "22 Jun · 12:10",
                Record = "TR-0581",
                Change = "Draft → Approved",
                Rationale = "North Hub rebalancing",
                ActionType = "Approval",
                RecordType = "Transfer",
                Date = "2025-06-22",
                CreatedAt = DateTime.UtcNow.AddHours(-2)
            },
            new AuditLog
            {
                Id = "AUD-482155",
                Action = "Purchase order created",
                Actor = "Maya Silva",
                Timestamp = "22 Jun · 11:45",
                Record = "PO-2025-184",
                Change = "New purchase order created",
                Rationale = "Emergency medical supplies",
                ActionType = "Procurement",
                RecordType = "Purchase Order",
                Date = "2025-06-22",
                CreatedAt = DateTime.UtcNow.AddHours(-3)
            },
            new AuditLog
            {
                Id = "AUD-482142",
                Action = "Supplier verified",
                Actor = "Daniel Perera",
                Timestamp = "22 Jun · 10:30",
                Record = "SUP-0014",
                Change = "Pending → Verified",
                Rationale = "Document verification completed",
                ActionType = "Verification",
                RecordType = "Supplier",
                Date = "2025-06-22",
                CreatedAt = DateTime.UtcNow.AddHours(-4)
            },
            new AuditLog
            {
                Id = "AUD-482130",
                Action = "Warehouse stock received",
                Actor = "Maya Silva",
                Timestamp = "21 Jun · 16:20",
                Record = "RES-00128",
                Change = "500 → 1000 units",
                Rationale = "Incoming stock receipt",
                ActionType = "Inventory",
                RecordType = "Resource",
                Date = "2025-06-21",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new AuditLog
            {
                Id = "AUD-482115",
                Action = "Dispatch completed",
                Actor = "Daniel Perera",
                Timestamp = "21 Jun · 14:10",
                Record = "DSP-1084",
                Change = "In Transit → Delivered",
                Rationale = "Proof of delivery received",
                ActionType = "Dispatch",
                RecordType = "Dispatch",
                Date = "2025-06-21",
                CreatedAt = DateTime.UtcNow.AddDays(-1).AddHours(-2)
            },
            new AuditLog
            {
                Id = "AUD-482102",
                Action = "Request approved",
                Actor = "Maya Silva",
                Timestamp = "21 Jun · 11:30",
                Record = "RR-2839",
                Change = "Pending → Approved",
                Rationale = "Critical medical request",
                ActionType = "Approval",
                RecordType = "Request",
                Date = "2025-06-21",
                CreatedAt = DateTime.UtcNow.AddDays(-1).AddHours(-5)
            },
            new AuditLog
            {
                Id = "AUD-482090",
                Action = "Transfer rejected",
                Actor = "Daniel Perera",
                Timestamp = "20 Jun · 15:45",
                Record = "TR-0570",
                Change = "Pending → Rejected",
                Rationale = "Insufficient available stock",
                ActionType = "Transfer",
                RecordType = "Transfer",
                Date = "2025-06-20",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            }
        ]);
    }
}
