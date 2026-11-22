namespace CareLink.Infrastructure.Repositories;

using System.Text.RegularExpressions;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;

public class InMemoryProcurementRepository : IProcurementRepository
{
    private readonly List<PurchaseOrder> _orders = [];
    private readonly Lock _lock = new();

    public InMemoryProcurementRepository()
    {
        SeedInitialData();
    }

    public Task<IEnumerable<PurchaseOrder>> GetAllAsync(ProcurementFilterParams? filters = null)
    {
        lock (_lock)
        {
            IEnumerable<PurchaseOrder> query = _orders.OrderByDescending(x => x.CreatedAt).ToList();

            if (filters != null)
            {
                if (!string.IsNullOrWhiteSpace(filters.Search))
                {
                    var s = filters.Search.Trim().ToLowerInvariant();
                    query = query.Where(x =>
                        x.Id.ToLowerInvariant().Contains(s) ||
                        x.Supplier.ToLowerInvariant().Contains(s) ||
                        x.Items.ToLowerInvariant().Contains(s) ||
                        x.Quantity.ToLowerInvariant().Contains(s) ||
                        x.Value.ToLowerInvariant().Contains(s) ||
                        (x.Notes != null && x.Notes.ToLowerInvariant().Contains(s))
                    );
                }

                if (!string.IsNullOrWhiteSpace(filters.Supplier) &&
                    !filters.Supplier.Equals("All suppliers", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(x => x.Supplier.Equals(filters.Supplier, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrWhiteSpace(filters.Type) &&
                    !filters.Type.Equals("All procurement types", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(x => x.Type.Equals(filters.Type, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrWhiteSpace(filters.Status) &&
                    !filters.Status.Equals("All statuses", StringComparison.OrdinalIgnoreCase) &&
                    !filters.Status.Equals("All", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(x => x.Status.Equals(filters.Status, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrWhiteSpace(filters.Tab))
                {
                    var tab = filters.Tab.Trim();
                    if (tab.Equals("Needs action", StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(x => x.Status.Equals("Awaiting Approval", StringComparison.OrdinalIgnoreCase));
                    }
                    else if (tab.Equals("Active", StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(x => x.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase) ||
                                                 x.Status.Equals("In Transit", StringComparison.OrdinalIgnoreCase));
                    }
                    else if (tab.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(x => x.Status.Equals("Delivered", StringComparison.OrdinalIgnoreCase));
                    }
                    else if (tab.Equals("Exceptions", StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(x => x.Status.Equals("Rejected", StringComparison.OrdinalIgnoreCase));
                    }
                }
            }

            return Task.FromResult(query);
        }
    }

    public Task<PurchaseOrder?> GetByIdAsync(string id)
    {
        lock (_lock)
        {
            var order = _orders.FirstOrDefault(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(order);
        }
    }

    public Task<PurchaseOrder> AddAsync(PurchaseOrder order)
    {
        lock (_lock)
        {
            if (string.IsNullOrWhiteSpace(order.Id))
            {
                var nextNumber = 185;
                var maxId = _orders
                    .Select(o => Regex.Match(o.Id, @"\d+$").Value)
                    .Where(v => !string.IsNullOrEmpty(v))
                    .Select(int.Parse)
                    .DefaultIfEmpty(184)
                    .Max();

                if (maxId >= 100)
                {
                    nextNumber = maxId + 1;
                }

                var currentYear = DateTime.UtcNow.Year;
                order.Id = $"PO-{currentYear}-{nextNumber}";
            }

            order.CreatedAt = DateTime.UtcNow;
            _orders.Insert(0, order);
            return Task.FromResult(order);
        }
    }

    public Task<PurchaseOrder?> UpdateStatusAsync(string id, string status, string? notes = null)
    {
        lock (_lock)
        {
            var order = _orders.FirstOrDefault(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (order == null)
            {
                return Task.FromResult<PurchaseOrder?>(null);
            }

            order.Status = status;
            if (!string.IsNullOrWhiteSpace(notes))
            {
                order.Notes = notes;
            }
            order.UpdatedAt = DateTime.UtcNow;

            return Task.FromResult<PurchaseOrder?>(order);
        }
    }

    public Task<bool> DeleteAsync(string id)
    {
        lock (_lock)
        {
            var index = _orders.FindIndex(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (index == -1)
            {
                return Task.FromResult(false);
            }
            _orders.RemoveAt(index);
            return Task.FromResult(true);
        }
    }

    public Task<ProcurementSummaryDto> GetSummaryAsync()
    {
        lock (_lock)
        {
            var summary = new ProcurementSummaryDto
            {
                OpenOrders = 24 + _orders.Count - 7,
                EmergencyOrders = _orders.Count(x => x.Type.Equals("Emergency", StringComparison.OrdinalIgnoreCase)) + 5,
                DueThisWeek = 12,
                SupplierFulfilment = "95.4%",
                TotalCommittedValue = "$186K",
                AwaitingApprovalCount = _orders.Count(x => x.Status.Equals("Awaiting Approval", StringComparison.OrdinalIgnoreCase))
            };

            return Task.FromResult(summary);
        }
    }

    private void SeedInitialData()
    {
        _orders.AddRange([
            new PurchaseOrder
            {
                Id = "PO-2025-184",
                Supplier = "MediSupply International",
                Items = "Trauma Medical Kits",
                Quantity = "200 units",
                Value = "$18,400",
                Type = "Emergency",
                DeliveryDue = "24 Jun 2025",
                Status = "Approved",
                CreatedAt = DateTime.UtcNow.AddHours(-1)
            },
            new PurchaseOrder
            {
                Id = "PO-2025-181",
                Supplier = "AquaSafe Ltd.",
                Items = "Drinking Water - 10L",
                Quantity = "10,000 units",
                Value = "$12,800",
                Type = "Emergency",
                DeliveryDue = "23 Jun 2025",
                Status = "In Transit",
                CreatedAt = DateTime.UtcNow.AddHours(-3)
            },
            new PurchaseOrder
            {
                Id = "PO-2025-174",
                Supplier = "ShelterWorks Cooperative",
                Items = "Shelter Kits",
                Quantity = "500 units",
                Value = "$42,500",
                Type = "Standard",
                DeliveryDue = "28 Jun 2025",
                Status = "Awaiting Approval",
                CreatedAt = DateTime.UtcNow.AddHours(-6)
            },
            new PurchaseOrder
            {
                Id = "PO-2025-169",
                Supplier = "Regional Food Services",
                Items = "Family Food Packs",
                Quantity = "4,000 units",
                Value = "$31,200",
                Type = "Standard",
                DeliveryDue = "21 Jun 2025",
                Status = "Delivered",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new PurchaseOrder
            {
                Id = "PO-2025-165",
                Supplier = "MediSupply International",
                Items = "First Aid Supplies",
                Quantity = "300 units",
                Value = "$9,500",
                Type = "Emergency",
                DeliveryDue = "29 Jun 2025",
                Status = "Approved",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new PurchaseOrder
            {
                Id = "PO-2025-162",
                Supplier = "AquaSafe Ltd.",
                Items = "Water Purification Tablets",
                Quantity = "2,000 packs",
                Value = "$6,800",
                Type = "Standard",
                DeliveryDue = "30 Jun 2025",
                Status = "In Transit",
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new PurchaseOrder
            {
                Id = "PO-2025-158",
                Supplier = "ShelterWorks Cooperative",
                Items = "Thermal Blankets",
                Quantity = "600 units",
                Value = "$15,000",
                Type = "Standard",
                DeliveryDue = "20 Jun 2025",
                Status = "Rejected",
                Notes = "Price variance exceeded agreed framework cap",
                CreatedAt = DateTime.UtcNow.AddDays(-4)
            }
        ]);
    }
}
