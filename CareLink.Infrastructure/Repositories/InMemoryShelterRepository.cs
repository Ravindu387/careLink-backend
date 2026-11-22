using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using CareLink.Core.Models;

namespace CareLink.Infrastructure.Repositories;

public class InMemoryShelterRepository : CareLink.Core.Interfaces.IShelterRepository, CareLink.Application.Interfaces.IShelterRepository
{
    private readonly object _lock = new();
    private List<Shelter> _shelters;
    private readonly List<ShelterBooking> _bookings = new();
    private int _bookingCounter = 1001;

    public InMemoryShelterRepository()
    {
        _shelters = CreateDefaultShelters();
    }

    #region Operational & Coordinator Methods (Safna / Core)

    public Task<IReadOnlyList<Shelter>> GetAllAsync(
        ShelterFilterParams? filter = null,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var query = _shelters.AsEnumerable();

            if (filter != null)
            {
                if (!string.IsNullOrWhiteSpace(filter.Search))
                {
                    var s = filter.Search.Trim().ToLowerInvariant();
                    query = query.Where(item =>
                        item.Name.ToLowerInvariant().Contains(s) ||
                        item.Location.ToLowerInvariant().Contains(s) ||
                        item.District.ToLowerInvariant().Contains(s) ||
                        item.Area.ToLowerInvariant().Contains(s) ||
                        item.Address.ToLowerInvariant().Contains(s) ||
                        item.Services.Any(svc => svc.ToLowerInvariant().Contains(s)) ||
                        (item.Notes != null && item.Notes.ToLowerInvariant().Contains(s)));
                }

                if (!string.IsNullOrWhiteSpace(filter.Area) &&
                    !filter.Area.Equals("All operational areas", StringComparison.OrdinalIgnoreCase) &&
                    !filter.Area.Equals("All", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(item =>
                        item.Location.Equals(filter.Area.Trim(), StringComparison.OrdinalIgnoreCase) ||
                        item.District.Equals(filter.Area.Trim(), StringComparison.OrdinalIgnoreCase) ||
                        item.Area.Equals(filter.Area.Trim(), StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrWhiteSpace(filter.Status) &&
                    !filter.Status.Equals("All statuses", StringComparison.OrdinalIgnoreCase) &&
                    !filter.Status.Equals("All", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(item =>
                        item.Status.Equals(filter.Status.Trim(), StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrWhiteSpace(filter.Availability) &&
                    !filter.Availability.Equals("Any availability", StringComparison.OrdinalIgnoreCase) &&
                    !filter.Availability.Equals("All", StringComparison.OrdinalIgnoreCase))
                {
                    if (filter.Availability.Equals("Available", StringComparison.OrdinalIgnoreCase) ||
                        filter.Availability.Equals("Space available", StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(item => item.Remaining > 0);
                    }
                    else if (filter.Availability.Equals("Full", StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(item => item.Remaining <= 0);
                    }
                }

                if (!string.IsNullOrWhiteSpace(filter.Service))
                {
                    var svcLower = filter.Service.Trim().ToLowerInvariant();
                    query = query.Where(item => item.Services.Any(svc => svc.ToLowerInvariant().Contains(svcLower)));
                }
            }

            var result = query.OrderBy(item => item.Id).Select(CloneShelter).ToList();
            return Task.FromResult<IReadOnlyList<Shelter>>(result);
        }
    }

    public Task<IEnumerable<Shelter>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var list = _shelters.OrderBy(s => s.Id).Select(CloneShelter).ToList();
            return Task.FromResult<IEnumerable<Shelter>>(list);
        }
    }

    public Task<Shelter?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var idStr = id.ToString();
            var code = $"SH-{id:D3}";
            var shelter = _shelters.FirstOrDefault(s => s.Id == idStr || s.Id == code)
                ?? _shelters.FirstOrDefault(s => int.TryParse(s.Id.Replace("SH-", ""), out var parsed) && parsed == id);
            return Task.FromResult(shelter == null ? null : CloneShelter(shelter));
        }
    }

    public Task<Shelter?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var shelter = _shelters.FirstOrDefault(s => s.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (shelter == null && int.TryParse(id, out var intId))
            {
                var idStr = intId.ToString();
                var code = $"SH-{intId:D3}";
                shelter = _shelters.FirstOrDefault(s => s.Id == idStr || s.Id == code);
            }
            return Task.FromResult(shelter == null ? null : CloneShelter(shelter));
        }
    }

    public Task<Shelter> AddAsync(Shelter shelter, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (string.IsNullOrWhiteSpace(shelter.Id))
            {
                int maxId = 0;
                foreach (var s in _shelters)
                {
                    var clean = s.Id.Replace("SH-", "");
                    if (int.TryParse(clean, out var parsed) && parsed > maxId) maxId = parsed;
                }
                shelter.Id = $"SH-{maxId + 1:D3}";
            }
            if (string.IsNullOrWhiteSpace(shelter.District))
            {
                shelter.District = shelter.Location;
            }
            shelter.CreatedAt = DateTimeOffset.UtcNow;
            shelter.UpdatedAt = DateTimeOffset.UtcNow;
            shelter.Status = ComputeStatus(shelter.Capacity, shelter.CurrentOccupancy);

            _shelters.Add(CloneShelter(shelter));
            return Task.FromResult(CloneShelter(shelter));
        }
    }

    public Task<Shelter?> UpdateAsync(Shelter shelter, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var existing = _shelters.FirstOrDefault(s => s.Id.Equals(shelter.Id, StringComparison.OrdinalIgnoreCase));
            if (existing == null && int.TryParse(shelter.Id, out var num))
            {
                var code = $"SH-{num:D3}";
                existing = _shelters.FirstOrDefault(s => s.Id.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            if (existing == null)
            {
                return Task.FromResult<Shelter?>(null);
            }

            existing.Name = shelter.Name.Trim();
            existing.Location = shelter.Location.Trim();
            existing.District = shelter.District.Trim();
            existing.Type = shelter.Type.Trim();
            existing.Manager = shelter.Manager.Trim();
            existing.Capacity = shelter.Capacity;
            existing.CurrentOccupancy = Math.Clamp(shelter.CurrentOccupancy, 0, shelter.Capacity);
            existing.Reserved = shelter.Reserved;
            existing.Services = shelter.Services.Select(s => s.Trim()).Distinct().ToList();
            existing.Status = ComputeStatus(existing.Capacity, existing.CurrentOccupancy);
            existing.ContactPhone = shelter.ContactPhone?.Trim() ?? string.Empty;
            existing.Notes = shelter.Notes?.Trim();
            if (!string.IsNullOrWhiteSpace(shelter.Address)) existing.Address = shelter.Address.Trim();
            if (shelter.Distance > 0) existing.Distance = shelter.Distance;
            existing.Accessible = shelter.Accessible;
            if (!string.IsNullOrWhiteSpace(shelter.OperatingHours)) existing.OperatingHours = shelter.OperatingHours.Trim();
            existing.IsEvacuationHub = shelter.IsEvacuationHub;
            existing.UpdatedAt = DateTimeOffset.UtcNow;

            return Task.FromResult<Shelter?>(CloneShelter(existing));
        }
    }

    public Task<Shelter?> UpdateOccupancyAsync(
        int id,
        int occupied,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var idStr = id.ToString();
            var code = $"SH-{id:D3}";
            var existing = _shelters.FirstOrDefault(s => s.Id == idStr || s.Id == code)
                ?? _shelters.FirstOrDefault(s => int.TryParse(s.Id.Replace("SH-", ""), out var parsed) && parsed == id);
            if (existing == null)
            {
                return Task.FromResult<Shelter?>(null);
            }

            existing.CurrentOccupancy = Math.Clamp(occupied, 0, existing.Capacity);
            existing.Status = ComputeStatus(existing.Capacity, existing.CurrentOccupancy);
            existing.UpdatedAt = DateTimeOffset.UtcNow;

            return Task.FromResult<Shelter?>(CloneShelter(existing));
        }
    }

    public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        return DeleteAsync(id.ToString(), cancellationToken);
    }

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var existing = _shelters.FirstOrDefault(s => s.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (existing == null && int.TryParse(id, out var intId))
            {
                var idStr = intId.ToString();
                var code = $"SH-{intId:D3}";
                existing = _shelters.FirstOrDefault(s => s.Id == idStr || s.Id == code);
            }
            if (existing == null)
            {
                return Task.FromResult(false);
            }

            _shelters.Remove(existing);
            return Task.FromResult(true);
        }
    }

    public Task<ShelterMetrics> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            int totalCap = _shelters.Sum(s => s.Capacity);
            int totalOcc = _shelters.Sum(s => s.CurrentOccupancy);
            int available = Math.Max(0, totalCap - totalOcc);
            int accepting = _shelters.Count(s => s.Remaining > 0);
            int nearCap = _shelters.Count(s => s.Status == "Near Capacity");
            int full = _shelters.Count(s => s.Status == "Full");
            int operational = _shelters.Count(s => s.Status == "Operational" || s.Status == "Active");

            var metrics = new ShelterMetrics
            {
                TotalShelters = _shelters.Count,
                TotalCapacity = totalCap,
                CurrentOccupancy = totalOcc,
                AvailableCapacity = available,
                AcceptingCount = accepting,
                NearCapacityCount = nearCap,
                FullCount = full,
                OperationalCount = operational,
                LastUpdated = DateTimeOffset.UtcNow
            };
            return Task.FromResult(metrics);
        }
    }

    public Task ResetToDefaultAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _shelters = CreateDefaultShelters();
            _bookings.Clear();
            _bookingCounter = 1001;
            return Task.CompletedTask;
        }
    }

    public Task<(IEnumerable<Shelter> Items, int TotalCount)> GetPagedAsync(
        ShelterQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var query = _shelters.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(parameters.Search))
            {
                var s = parameters.Search.Trim().ToLowerInvariant();
                query = query.Where(item =>
                    item.Name.ToLowerInvariant().Contains(s) ||
                    item.Location.ToLowerInvariant().Contains(s) ||
                    item.District.ToLowerInvariant().Contains(s) ||
                    item.Area.ToLowerInvariant().Contains(s) ||
                    item.Address.ToLowerInvariant().Contains(s) ||
                    item.Services.Any(svc => svc.ToLowerInvariant().Contains(s)));
            }

            if (!string.IsNullOrWhiteSpace(parameters.District) &&
                !parameters.District.Equals("All districts", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(item => item.District.Equals(parameters.District.Trim(), StringComparison.OrdinalIgnoreCase) ||
                                            item.Location.Equals(parameters.District.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(parameters.Type) &&
                !parameters.Type.Equals("All shelter types", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(item => item.Type.Equals(parameters.Type.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(parameters.Status) &&
                !parameters.Status.Equals("All statuses", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(item => item.Status.Equals(parameters.Status.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (parameters.MinCapacity.HasValue)
            {
                query = query.Where(item => item.Capacity >= parameters.MinCapacity.Value);
            }

            if (parameters.MaxCapacity.HasValue)
            {
                query = query.Where(item => item.Capacity <= parameters.MaxCapacity.Value);
            }

            var list = query.ToList();
            var totalCount = list.Count;
            var page = parameters.Page.GetValueOrDefault(1) > 0 ? parameters.Page.GetValueOrDefault(1) : 1;
            var pageSize = parameters.PageSize.GetValueOrDefault(10) > 0 ? parameters.PageSize.GetValueOrDefault(10) : 10;

            var paged = list
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(CloneShelter)
                .ToList();

            return Task.FromResult(((IEnumerable<Shelter>)paged, totalCount));
        }
    }

    public Task<(int Registered, int Operational, int NearCapacity, int Maintenance)> GetSummaryMetricsAsync(
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            int reg = _shelters.Count;
            int op = _shelters.Count(s => s.Status.Equals("Operational", StringComparison.OrdinalIgnoreCase) || s.Status.Equals("Active", StringComparison.OrdinalIgnoreCase));
            int nc = _shelters.Count(s => s.Status.Equals("Near Capacity", StringComparison.OrdinalIgnoreCase));
            int maint = _shelters.Count(s => s.Status.Equals("Maintenance", StringComparison.OrdinalIgnoreCase));
            return Task.FromResult((reg, op, nc, maint));
        }
    }

    #endregion

    #region Citizen & Extended Shelter Methods (Oshadi)

    public Task<IEnumerable<Shelter>> GetSheltersAsync(
        string? search = null,
        string? area = null,
        double? maxDistance = null,
        string? availability = null,
        bool? accessibleOnly = null,
        string? service = null)
    {
        lock (_lock)
        {
            var query = _shelters.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(s =>
                    s.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    s.Area.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    s.Address.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    s.Location.Contains(term, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(area) && !area.Equals("All", StringComparison.OrdinalIgnoreCase) && !area.Equals("All areas", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(s => s.Area.Equals(area.Trim(), StringComparison.OrdinalIgnoreCase) ||
                                         s.Location.Equals(area.Trim(), StringComparison.OrdinalIgnoreCase) ||
                                         s.District.Equals(area.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (maxDistance.HasValue)
            {
                query = query.Where(s => s.Distance <= maxDistance.Value);
            }

            if (!string.IsNullOrWhiteSpace(availability) && !availability.Equals("All statuses", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(s => s.Status.Equals(availability.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (accessibleOnly == true)
            {
                query = query.Where(s => s.Accessible);
            }

            if (!string.IsNullOrWhiteSpace(service) && !service.Equals("All services", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(s => s.Services.Any(sv => sv.Contains(service.Trim(), StringComparison.OrdinalIgnoreCase)));
            }

            var results = query.OrderBy(s => s.Distance).Select(CloneShelter).ToList();
            return Task.FromResult<IEnumerable<Shelter>>(results);
        }
    }

    public Task<Shelter> CreateAsync(Shelter shelter)
    {
        return AddAsync(shelter);
    }

    public Task<Shelter?> UpdateAsync(string id, Shelter shelter)
    {
        shelter.Id = id;
        return UpdateAsync(shelter);
    }

    public Task<ShelterBooking> CreateBookingAsync(ShelterBooking booking)
    {
        lock (_lock)
        {
            if (string.IsNullOrWhiteSpace(booking.Id))
            {
                booking.Id = $"SH-RES-{_bookingCounter++}";
            }
            booking.BookedAt = DateTime.UtcNow;
            _bookings.Add(booking);

            var shelter = _shelters.FirstOrDefault(s => s.Id.Equals(booking.ShelterId, StringComparison.OrdinalIgnoreCase));
            if (shelter == null && int.TryParse(booking.ShelterId, out var num))
            {
                var code = $"SH-{num:D3}";
                shelter = _shelters.FirstOrDefault(s => s.Id.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            if (shelter != null)
            {
                shelter.CurrentOccupancy = Math.Min(shelter.Capacity, shelter.CurrentOccupancy + booking.NumberOfPeople);
                shelter.Status = shelter.Remaining > 0 ? "Space available" : "Full";
                shelter.UpdatedAt = DateTimeOffset.UtcNow;
            }

            return Task.FromResult(booking);
        }
    }

    public Task<IEnumerable<ShelterBooking>> GetBookingsAsync(string? citizenName = null)
    {
        lock (_lock)
        {
            var query = _bookings.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(citizenName))
            {
                query = query.Where(b => b.CitizenName.Equals(citizenName.Trim(), StringComparison.OrdinalIgnoreCase));
            }
            var results = query.OrderByDescending(b => b.BookedAt).ToList();
            return Task.FromResult<IEnumerable<ShelterBooking>>(results);
        }
    }

    Task<Shelter?> CareLink.Application.Interfaces.IShelterRepository.GetByIdAsync(string id) =>
        GetByIdAsync(id, CancellationToken.None);

    Task<bool> CareLink.Application.Interfaces.IShelterRepository.DeleteAsync(string id) =>
        DeleteAsync(id, CancellationToken.None);

    #endregion

    #region Helpers & Seed Data

    private static string ComputeStatus(int capacity, int occupied)
    {
        if (capacity <= 0 || occupied >= capacity)
        {
            return "Full";
        }
        if ((double)occupied / capacity >= 0.85)
        {
            return "Near Capacity";
        }
        return "Operational";
    }

    private static Shelter CloneShelter(Shelter s)
    {
        return new Shelter
        {
            Id = s.Id,
            Name = s.Name,
            Location = s.Location,
            District = s.District,
            Type = s.Type,
            Manager = s.Manager,
            Capacity = s.Capacity,
            CurrentOccupancy = s.CurrentOccupancy,
            Reserved = s.Reserved,
            Services = new List<string>(s.Services),
            Status = s.Status,
            ContactPhone = s.ContactPhone,
            Notes = s.Notes,
            Address = s.Address,
            Distance = s.Distance,
            Accessible = s.Accessible,
            OperatingHours = s.OperatingHours,
            IsEvacuationHub = s.IsEvacuationHub,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt
        };
    }

    private static List<Shelter> CreateDefaultShelters()
    {
        return new List<Shelter>
        {
            new()
            {
                Id = "SH-001",
                Name = "Riverton High School",
                Location = "Central Ward",
                District = "Central Ward",
                Address = "45 College Road, Central Ward, Greater Riverton",
                Distance = 1.4,
                Type = "Public School",
                Manager = "Brenda Silva",
                Capacity = 660,
                CurrentOccupancy = 612,
                Services = new List<string> { "Accessible", "Medical", "Families", "Food" },
                Status = "Near Capacity",
                Accessible = true,
                ContactPhone = "+94 77 412 8041",
                OperatingHours = "24 Hours · Evacuation Centre",
                IsEvacuationHub = true,
                Notes = "Primary central ward emergency reception facility with wheelchair ramps and backup power."
            },
            new()
            {
                Id = "SH-002",
                Name = "North Community Hall",
                Location = "North Ward",
                District = "North Ward",
                Address = "12 Market Road, North Ward, Greater Riverton",
                Distance = 2.2,
                Type = "Community Centre",
                Manager = "Marcus Vance",
                Capacity = 500,
                CurrentOccupancy = 284,
                Services = new List<string> { "Accessible", "Food", "Pets", "Childcare", "Blankets" },
                Status = "Operational",
                Accessible = true,
                ContactPhone = "+94 77 412 8022",
                OperatingHours = "24 Hours · Open Intake",
                IsEvacuationHub = true,
                Notes = "Active hot meal kitchen on site. Direct access for transport vans."
            },
            new()
            {
                Id = "SH-003",
                Name = "St. Mary's Civic Centre",
                Location = "Eastbank",
                District = "Eastbank",
                Address = "14 Church Street, Eastbank",
                Distance = 2.8,
                Type = "Civic Centre",
                Manager = "Sister Theresa",
                Capacity = 450,
                CurrentOccupancy = 450,
                Services = new List<string> { "Medical", "Childcare", "Psychosocial support" },
                Status = "Full",
                Accessible = false,
                ContactPhone = "+94 77 412 8033",
                OperatingHours = "Capacity Reached (Referrals to North Hall)",
                IsEvacuationHub = false,
                Notes = "Currently at 100% capacity. Redirecting newly displaced families to North Community Hall."
            },
            new()
            {
                Id = "SH-004",
                Name = "Lakeside Sports Hall",
                Location = "West District",
                District = "West District",
                Address = "88 Lake Circular Road, West District",
                Distance = 3.8,
                Type = "Sports Complex",
                Manager = "David Perera",
                Capacity = 400,
                CurrentOccupancy = 196,
                Services = new List<string> { "Food", "Family rooms", "Shower facilities" },
                Status = "Operational",
                Accessible = false,
                ContactPhone = "+94 77 412 8044",
                OperatingHours = "06:00 - 22:00 (Security on site 24/7)",
                IsEvacuationHub = false,
                Notes = "Equipped with family partitions, laundry, and clean shower blocks."
            },
            new()
            {
                Id = "SH-005",
                Name = "Central Ward Stadium Pavilion",
                Location = "Central Ward",
                District = "Central Ward",
                Address = "10 Stadium Avenue, Central Ward",
                Distance = 2.0,
                Type = "Sports Pavilion",
                Manager = "Capt. Rohan Sen",
                Capacity = 1200,
                CurrentOccupancy = 820,
                Services = new List<string> { "Accessible", "Food", "Medical", "Family rooms" },
                Status = "Operational",
                Accessible = true,
                ContactPhone = "+94 77 412 8055",
                OperatingHours = "24/7 during crisis operations",
                IsEvacuationHub = true,
                Notes = "Main high-capacity staging dome equipped with field medical tent and triage."
            },
            new()
            {
                Id = "SH-006",
                Name = "West District Youth Centre",
                Location = "West District",
                District = "West District",
                Address = "5 Youth Lane, West District",
                Distance = 4.2,
                Type = "Community Centre",
                Manager = "Kamal Hettiarachchi",
                Capacity = 350,
                CurrentOccupancy = 120,
                Services = new List<string> { "Food", "Mental Health Support" },
                Status = "Operational",
                Accessible = true,
                ContactPhone = "+94 77 412 8066",
                OperatingHours = "24/7 during crisis operations",
                IsEvacuationHub = false,
                Notes = "Offers specialized youth trauma counselors and quiet study rooms."
            },
            new()
            {
                Id = "SH-007",
                Name = "Riverton Port Authority Warehouse 3",
                Location = "Eastbank",
                District = "Eastbank",
                Address = "Dock 3, Riverton Port, Eastbank",
                Distance = 5.1,
                Type = "Temporary Shelter",
                Manager = "Lt. Dinesh Jayawardena",
                Capacity = 800,
                CurrentOccupancy = 780,
                Services = new List<string> { "Food", "Medical", "Logistics Hub" },
                Status = "Near Capacity",
                Accessible = false,
                ContactPhone = "+94 77 412 8077",
                OperatingHours = "24/7 Logistics & Shelter",
                IsEvacuationHub = true,
                Notes = "Emergency intake shelter directly connected to the river supply barge."
            },
            new()
            {
                Id = "SH-008",
                Name = "North Ward Primary School",
                Location = "North Ward",
                District = "North Ward",
                Address = "25 School Road, North Ward",
                Distance = 3.1,
                Type = "Public School",
                Manager = "Anoma Fonseka",
                Capacity = 450,
                CurrentOccupancy = 210,
                Services = new List<string> { "Accessible", "Food", "Childcare" },
                Status = "Operational",
                Accessible = true,
                ContactPhone = "+94 77 412 8088",
                OperatingHours = "24/7 during crisis operations",
                IsEvacuationHub = false,
                Notes = "Primary evacuation site for displaced school-age families."
            },
            new()
            {
                Id = "SH-009",
                Name = "Eastbank Community Clinic Auxiliary Hall",
                Location = "Eastbank",
                District = "Eastbank",
                Address = "9 Hospital Road, Eastbank",
                Distance = 2.9,
                Type = "Medical Shelter",
                Manager = "Dr. Priyantha Silva",
                Capacity = 200,
                CurrentOccupancy = 195,
                Services = new List<string> { "Accessible", "Medical", "Special Needs" },
                Status = "Near Capacity",
                Accessible = true,
                ContactPhone = "+94 77 412 8099",
                OperatingHours = "24/7 Medical Clinic & Shelter",
                IsEvacuationHub = false,
                Notes = "Specialized for elderly, infirm, and post-operative patients."
            },
            new()
            {
                Id = "SH-010",
                Name = "Greenfield Community Hall",
                Location = "West District",
                District = "West District",
                Address = "72 Greenfield Road, West District",
                Distance = 4.8,
                Type = "Community Shelter",
                Manager = "Nisha Weerasinghe",
                Capacity = 300,
                CurrentOccupancy = 150,
                Services = new List<string> { "Food", "Childcare" },
                Status = "Operational",
                Accessible = true,
                ContactPhone = "+94 77 412 8077",
                OperatingHours = "24/7 during crisis operations",
                IsEvacuationHub = false,
                Notes = "Dedicated mother-and-child support hub with pediatric supplies."
            }
        };
    }

    #endregion
}
