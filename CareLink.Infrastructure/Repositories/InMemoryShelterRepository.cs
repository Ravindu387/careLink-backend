namespace CareLink.Infrastructure.Repositories;

using System.Collections.Concurrent;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;

public class InMemoryShelterRepository : IShelterRepository
{
    private readonly ConcurrentDictionary<string, Shelter> _shelters = new();
    private readonly ConcurrentDictionary<string, ShelterBooking> _bookings = new();
    private readonly Lock _lock = new();
    private int _idCounter = 5;
    private int _bookingCounter = 1001;

    public InMemoryShelterRepository()
    {
        SeedDefaultShelters();
    }

    private void SeedDefaultShelters()
    {
        var seedList = new List<Shelter>
        {
            new()
            {
                Id = "1",
                Name = "Riverton High School",
                Area = "Central Ward",
                Address = "45 College Road, Central Ward, Greater Riverton",
                Distance = 1.4,
                Capacity = 300,
                Availability = 48,
                Occupied = 252,
                Services = new List<string> { "Accessible", "Medical", "Families", "Food" },
                Status = "Space available",
                Accessible = true,
                ContactPhone = "+94 11 234 5678",
                ManagerName = "Major Sarath Wickramasinghe",
                OperatingHours = "24 Hours · Evacuation Centre",
                IsEvacuationHub = true,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-10)
            },
            new()
            {
                Id = "2",
                Name = "North Community Hall",
                Area = "North Ward",
                Address = "12 Market Road, North Ward, Greater Riverton",
                Distance = 2.2,
                Capacity = 400,
                Availability = 216,
                Occupied = 184,
                Services = new List<string> { "Accessible", "Food", "Pets", "Childcare", "Blankets" },
                Status = "Space available",
                Accessible = true,
                ContactPhone = "+94 11 234 8912",
                ManagerName = "Mrs. Sunethra Perera",
                OperatingHours = "24 Hours · Open Intake",
                IsEvacuationHub = true,
                CreatedAt = DateTime.UtcNow.AddDays(-6),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-25)
            },
            new()
            {
                Id = "3",
                Name = "Lakeside Sports Hall",
                Area = "West District",
                Address = "88 Lake Circular Road, West District",
                Distance = 3.8,
                Capacity = 350,
                Availability = 204,
                Occupied = 146,
                Services = new List<string> { "Food", "Family rooms", "Shower facilities" },
                Status = "Space available",
                Accessible = false,
                ContactPhone = "+94 11 234 9901",
                ManagerName = "Officer K. Fernando",
                OperatingHours = "06:00 - 22:00 (Security on site 24/7)",
                IsEvacuationHub = false,
                CreatedAt = DateTime.UtcNow.AddDays(-4),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-40)
            },
            new()
            {
                Id = "4",
                Name = "St. Mary Civic Centre",
                Area = "Eastbank",
                Address = "14 Church Street, Eastbank",
                Distance = 2.8,
                Capacity = 150,
                Availability = 0,
                Occupied = 150,
                Services = new List<string> { "Medical", "Childcare", "Psychosocial support" },
                Status = "Full",
                Accessible = false,
                ContactPhone = "+94 11 234 4455",
                ManagerName = "Fr. Anthony Silva",
                OperatingHours = "Capacity Reached (Referrals to North Hall)",
                IsEvacuationHub = false,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddHours(-1)
            }
        };

        foreach (var shelter in seedList)
        {
            _shelters[shelter.Id] = shelter;
        }
    }

    public Task<IEnumerable<Shelter>> GetSheltersAsync(
        string? search = null,
        string? area = null,
        double? maxDistance = null,
        string? availability = null,
        bool? accessibleOnly = null,
        string? service = null)
    {
        var query = _shelters.Values.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(s =>
                s.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                s.Area.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                s.Address.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(area) && !area.Equals("All", StringComparison.OrdinalIgnoreCase) && !area.Equals("All areas", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(s => s.Area.Equals(area.Trim(), StringComparison.OrdinalIgnoreCase));
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

        var results = query.OrderBy(s => s.Distance).ToList();
        return Task.FromResult<IEnumerable<Shelter>>(results);
    }

    public Task<Shelter?> GetByIdAsync(string id)
    {
        _shelters.TryGetValue(id.Trim(), out var shelter);
        return Task.FromResult(shelter);
    }

    public Task<Shelter> CreateAsync(Shelter shelter)
    {
        lock (_lock)
        {
            if (string.IsNullOrWhiteSpace(shelter.Id))
            {
                shelter.Id = (_idCounter++).ToString();
            }
            shelter.CreatedAt = DateTime.UtcNow;
            shelter.UpdatedAt = DateTime.UtcNow;
            _shelters[shelter.Id] = shelter;
            return Task.FromResult(shelter);
        }
    }

    public Task<Shelter?> UpdateAsync(string id, Shelter shelter)
    {
        lock (_lock)
        {
            if (_shelters.TryGetValue(id.Trim(), out var existing))
            {
                existing.Name = shelter.Name;
                existing.Area = shelter.Area;
                existing.Address = shelter.Address;
                existing.Distance = shelter.Distance;
                existing.Capacity = shelter.Capacity;
                existing.Availability = shelter.Availability;
                existing.Occupied = shelter.Capacity - shelter.Availability;
                existing.Services = shelter.Services;
                existing.Status = existing.Availability > 0 ? "Space available" : "Full";
                existing.Accessible = shelter.Accessible;
                existing.ContactPhone = shelter.ContactPhone;
                existing.ManagerName = shelter.ManagerName;
                existing.OperatingHours = shelter.OperatingHours;
                existing.IsEvacuationHub = shelter.IsEvacuationHub;
                existing.UpdatedAt = DateTime.UtcNow;
                return Task.FromResult<Shelter?>(existing);
            }
            return Task.FromResult<Shelter?>(null);
        }
    }

    public Task<bool> DeleteAsync(string id)
    {
        return Task.FromResult(_shelters.TryRemove(id.Trim(), out _));
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
            _bookings[booking.Id] = booking;

            // Decrement availability in shelter if spaces remain
            if (_shelters.TryGetValue(booking.ShelterId, out var shelter))
            {
                shelter.Availability = Math.Max(0, shelter.Availability - booking.NumberOfPeople);
                shelter.Occupied = Math.Min(shelter.Capacity, shelter.Occupied + booking.NumberOfPeople);
                shelter.Status = shelter.Availability > 0 ? "Space available" : "Full";
                shelter.UpdatedAt = DateTime.UtcNow;
            }

            return Task.FromResult(booking);
        }
    }

    public Task<IEnumerable<ShelterBooking>> GetBookingsAsync(string? citizenName = null)
    {
        var query = _bookings.Values.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(citizenName))
        {
            query = query.Where(b => b.CitizenName.Equals(citizenName.Trim(), StringComparison.OrdinalIgnoreCase));
        }
        var results = query.OrderByDescending(b => b.BookedAt).ToList();
        return Task.FromResult<IEnumerable<ShelterBooking>>(results);
    }
}
