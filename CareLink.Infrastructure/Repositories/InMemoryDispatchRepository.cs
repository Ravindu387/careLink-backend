namespace CareLink.Infrastructure.Repositories;

using System.Text.RegularExpressions;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;

public class InMemoryDispatchRepository : IDispatchRepository
{
    private readonly List<Dispatch> _dispatches = [];
    private readonly Lock _lock = new();

    public InMemoryDispatchRepository()
    {
        SeedInitialData();
    }

    public Task<IEnumerable<Dispatch>> GetAllAsync(DispatchFilterParams? filters = null)
    {
        lock (_lock)
        {
            IEnumerable<Dispatch> query = _dispatches.OrderByDescending(x => x.CreatedAt).ToList();

            if (filters != null)
            {
                if (!string.IsNullOrWhiteSpace(filters.Search))
                {
                    var s = filters.Search.Trim().ToLowerInvariant();
                    query = query.Where(x =>
                        x.Id.ToLowerInvariant().Contains(s) ||
                        x.Route.ToLowerInvariant().Contains(s) ||
                        x.Resource.ToLowerInvariant().Contains(s) ||
                        x.Vehicle.ToLowerInvariant().Contains(s) ||
                        x.Driver.ToLowerInvariant().Contains(s) ||
                        (x.Notes != null && x.Notes.ToLowerInvariant().Contains(s))
                    );
                }

                if (!string.IsNullOrWhiteSpace(filters.Status) &&
                    !filters.Status.Equals("All delivery states", StringComparison.OrdinalIgnoreCase) &&
                    !filters.Status.Equals("All", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(x => x.Status.Equals(filters.Status, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrWhiteSpace(filters.Route) &&
                    !filters.Route.Equals("All routes", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(x => x.Route.Equals(filters.Route, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrWhiteSpace(filters.Vehicle) &&
                    !filters.Vehicle.Equals("All vehicles", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(x => x.Vehicle.Equals(filters.Vehicle, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrWhiteSpace(filters.Tab))
                {
                    var tab = filters.Tab.Trim();
                    if (tab.Equals("Needs action", StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(x => x.Status.Equals("Delayed", StringComparison.OrdinalIgnoreCase) ||
                                                 x.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase));
                    }
                    else if (tab.Equals("Active", StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(x => x.Status.Equals("In Transit", StringComparison.OrdinalIgnoreCase));
                    }
                    else if (tab.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(x => x.Status.Equals("Delivered", StringComparison.OrdinalIgnoreCase));
                    }
                    else if (tab.Equals("Exceptions", StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(x => x.Status.Equals("Delayed", StringComparison.OrdinalIgnoreCase));
                    }
                }
            }

            return Task.FromResult(query);
        }
    }

    public Task<Dispatch?> GetByIdAsync(string id)
    {
        lock (_lock)
        {
            var item = _dispatches.FirstOrDefault(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(item);
        }
    }

    public Task<Dispatch> AddAsync(Dispatch dispatch)
    {
        lock (_lock)
        {
            if (string.IsNullOrWhiteSpace(dispatch.Id))
            {
                var nextNumber = 1093;
                var maxId = _dispatches
                    .Select(d => Regex.Match(d.Id, @"\d+").Value)
                    .Where(v => !string.IsNullOrEmpty(v))
                    .Select(int.Parse)
                    .DefaultIfEmpty(1092)
                    .Max();

                if (maxId >= 1000)
                {
                    nextNumber = maxId + 1;
                }
                dispatch.Id = $"DSP-{nextNumber}";
            }

            dispatch.CreatedAt = DateTime.UtcNow;
            _dispatches.Insert(0, dispatch);
            return Task.FromResult(dispatch);
        }
    }

    public Task<Dispatch?> UpdateStatusAsync(string id, string status, string? currentLocation = null, string? notes = null)
    {
        lock (_lock)
        {
            var item = _dispatches.FirstOrDefault(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (item == null)
            {
                return Task.FromResult<Dispatch?>(null);
            }

            item.Status = status;
            if (!string.IsNullOrWhiteSpace(currentLocation))
            {
                item.CurrentLocation = currentLocation;
            }
            if (!string.IsNullOrWhiteSpace(notes))
            {
                item.Notes = notes;
            }
            item.UpdatedAt = DateTime.UtcNow;

            return Task.FromResult<Dispatch?>(item);
        }
    }

    public Task<bool> DeleteAsync(string id)
    {
        lock (_lock)
        {
            var index = _dispatches.FindIndex(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (index == -1)
            {
                return Task.FromResult(false);
            }
            _dispatches.RemoveAt(index);
            return Task.FromResult(true);
        }
    }

    public Task<DispatchSummaryDto> GetSummaryAsync()
    {
        lock (_lock)
        {
            var summary = new DispatchSummaryDto
            {
                Total = _dispatches.Count,
                Active = _dispatches.Count(x => !x.Status.Equals("Delivered", StringComparison.OrdinalIgnoreCase)),
                OnSchedule = _dispatches.Count(x => x.Status.Equals("In Transit", StringComparison.OrdinalIgnoreCase)),
                Delayed = _dispatches.Count(x => x.Status.Equals("Delayed", StringComparison.OrdinalIgnoreCase)),
                Delivered = _dispatches.Count(x => x.Status.Equals("Delivered", StringComparison.OrdinalIgnoreCase)),
                NeedsAction = _dispatches.Count(x => x.Status.Equals("Delayed", StringComparison.OrdinalIgnoreCase) ||
                                                    x.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
            };

            return Task.FromResult(summary);
        }
    }

    private void SeedInitialData()
    {
        _dispatches.AddRange([
            new Dispatch
            {
                Id = "DSP-1092",
                Route = "Central Hub → North Ward",
                Resource = "Drinking Water",
                Quantity = "2,400 units",
                Vehicle = "Truck · RT-204",
                Driver = "Amir Khan",
                Departed = "13:28",
                Eta = "14:12",
                Status = "In Transit",
                CurrentLocation = "Sector 3 Highway Junction",
                CreatedAt = DateTime.UtcNow.AddMinutes(-40)
            },
            new Dispatch
            {
                Id = "DSP-1089",
                Route = "West Depot → Central Ward",
                Resource = "Fuel · Diesel",
                Quantity = "1,200 L",
                Vehicle = "Tanker · TK-018",
                Driver = "L. Perera",
                Departed = "12:55",
                Eta = "13:48",
                Status = "Delayed",
                CurrentLocation = "Bridge 4 Detour Route",
                Notes = "Heavy flood waters causing route detour",
                CreatedAt = DateTime.UtcNow.AddHours(-1)
            },
            new Dispatch
            {
                Id = "DSP-1084",
                Route = "Eastbank → CareLink Shelter",
                Resource = "Shelter Kits",
                Quantity = "80 units",
                Vehicle = "Truck · RT-118",
                Driver = "N. Okafor",
                Departed = "11:22",
                Eta = "12:14",
                Status = "Delivered",
                CurrentLocation = "CareLink Shelter Main Entrance",
                CreatedAt = DateTime.UtcNow.AddHours(-2)
            },
            new Dispatch
            {
                Id = "DSP-1078",
                Route = "North Hub → Riverton High",
                Resource = "Food Packs",
                Quantity = "420 units",
                Vehicle = "Van · VN-042",
                Driver = "S. Mensah",
                Departed = "10:42",
                Eta = "11:38",
                Status = "Delivered",
                CurrentLocation = "Riverton High Shelter Hall",
                CreatedAt = DateTime.UtcNow.AddHours(-3)
            },
            new Dispatch
            {
                Id = "DSP-1075",
                Route = "Central Hub → South Shelter",
                Resource = "Medical Kits",
                Quantity = "150 units",
                Vehicle = "Truck · RT-205",
                Driver = "Daniel Silva",
                Departed = "09:15",
                Eta = "10:30",
                Status = "In Transit",
                CurrentLocation = "South Avenue Bridge",
                CreatedAt = DateTime.UtcNow.AddHours(-4)
            },
            new Dispatch
            {
                Id = "DSP-1072",
                Route = "West Depot → Eastbank",
                Resource = "Hygiene Kits",
                Quantity = "300 units",
                Vehicle = "Van · VN-051",
                Driver = "A. Fernando",
                Departed = "08:45",
                Eta = "09:50",
                Status = "Pending",
                CurrentLocation = "West Depot Loading Bay 2",
                CreatedAt = DateTime.UtcNow.AddHours(-5)
            },
            new Dispatch
            {
                Id = "DSP-1069",
                Route = "North Hub → Central Ward",
                Resource = "Thermal Blankets",
                Quantity = "200 units",
                Vehicle = "Truck · RT-120",
                Driver = "M. Perera",
                Departed = "07:30",
                Eta = "08:45",
                Status = "Delayed",
                CurrentLocation = "North Corridor Checkpoint",
                Notes = "Awaiting safety clearance for debris clearing",
                CreatedAt = DateTime.UtcNow.AddHours(-6)
            }
        ]);
    }
}
