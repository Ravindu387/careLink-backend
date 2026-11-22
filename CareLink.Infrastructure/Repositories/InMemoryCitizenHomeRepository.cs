namespace CareLink.Infrastructure.Repositories;

using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;

public class InMemoryCitizenHomeRepository : ICitizenHomeRepository
{
    private readonly ConcurrentDictionary<string, CitizenRequest> _requests = new();
    private readonly ConcurrentDictionary<string, NearbyFacility> _facilities = new();
    private CrisisOperation _operation = new();
    private readonly Lock _lock = new();

    public InMemoryCitizenHomeRepository()
    {
        SeedInitialData();
    }

    public Task<CrisisOperation> GetActiveOperationAsync()
    {
        return Task.FromResult(_operation);
    }

    public Task<IEnumerable<CitizenRequest>> GetCitizenRequestsAsync(string? citizenName = null, string? search = null, string? status = null)
    {
        var list = _requests.Values.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(citizenName))
        {
            list = list.Where(r => string.Equals(r.CitizenName, citizenName.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(status) && !status.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            list = list.Where(r => string.Equals(r.Status, status.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLowerInvariant();
            list = list.Where(r =>
                (r.Id?.ToLowerInvariant().Contains(s) ?? false) ||
                (r.Title?.ToLowerInvariant().Contains(s) ?? false) ||
                (r.Category?.ToLowerInvariant().Contains(s) ?? false) ||
                (r.Description?.ToLowerInvariant().Contains(s) ?? false) ||
                (r.Status?.ToLowerInvariant().Contains(s) ?? false)
            );
        }

        var ordered = list.OrderByDescending(r => r.CreatedAt).AsEnumerable();
        return Task.FromResult(ordered);
    }

    public Task<CitizenRequest?> GetCitizenRequestByIdAsync(string id)
    {
        _requests.TryGetValue(id, out var req);
        return Task.FromResult(req);
    }

    public Task<CitizenRequest> CreateCitizenRequestAsync(CitizenRequest request)
    {
        lock (_lock)
        {
            if (string.IsNullOrWhiteSpace(request.Id))
            {
                request.Id = GenerateNextRequestId();
            }

            if (request.CreatedAt == default)
            {
                request.CreatedAt = DateTime.UtcNow;
            }

            _requests[request.Id] = request;
            return Task.FromResult(request);
        }
    }

    public Task<CitizenRequest?> UpdateCitizenRequestStatusAsync(string id, string status)
    {
        lock (_lock)
        {
            if (!_requests.TryGetValue(id, out var req))
            {
                return Task.FromResult<CitizenRequest?>(null);
            }

            req.Status = status;
            req.UpdatedAt = DateTime.UtcNow;
            _requests[id] = req;
            return Task.FromResult<CitizenRequest?>(req);
        }
    }

    public Task<bool> DeleteCitizenRequestAsync(string id)
    {
        return Task.FromResult(_requests.TryRemove(id, out _));
    }

    public Task<IEnumerable<NearbyFacility>> GetNearbyFacilitiesAsync(string? search = null)
    {
        var list = _facilities.Values.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLowerInvariant();
            list = list.Where(f =>
                (f.Name?.ToLowerInvariant().Contains(s) ?? false) ||
                (f.Category?.ToLowerInvariant().Contains(s) ?? false) ||
                (f.Status?.ToLowerInvariant().Contains(s) ?? false) ||
                (f.Address?.ToLowerInvariant().Contains(s) ?? false) ||
                f.Services.Any(sv => sv.ToLowerInvariant().Contains(s))
            );
        }

        return Task.FromResult(list);
    }

    public Task<NearbyFacility?> GetFacilityByIdAsync(string id)
    {
        _facilities.TryGetValue(id, out var fac);
        return Task.FromResult(fac);
    }

    public Task<NearbyFacility> CreateFacilityAsync(NearbyFacility facility)
    {
        lock (_lock)
        {
            if (string.IsNullOrWhiteSpace(facility.Id))
            {
                facility.Id = $"FAC-0{_facilities.Count + 1}";
            }
            if (facility.CreatedAt == default)
            {
                facility.CreatedAt = DateTime.UtcNow;
            }
            _facilities[facility.Id] = facility;
            return Task.FromResult(facility);
        }
    }

    public Task<NearbyFacility?> UpdateFacilityStatusAsync(string id, string status)
    {
        lock (_lock)
        {
            if (!_facilities.TryGetValue(id, out var fac))
            {
                return Task.FromResult<NearbyFacility?>(null);
            }
            fac.Status = status;
            _facilities[id] = fac;
            return Task.FromResult<NearbyFacility?>(fac);
        }
    }

    private string GenerateNextRequestId()
    {
        var maxNumber = 2841;
        foreach (var key in _requests.Keys)
        {
            var match = Regex.Match(key, @"\d+");
            if (match.Success && int.TryParse(match.Value, out var num))
            {
                if (num > maxNumber) maxNumber = num;
            }
        }

        return $"REQ-{maxNumber + 1}";
    }

    private void SeedInitialData()
    {
        _operation = new CrisisOperation
        {
            Id = "CRISIS-2026-01",
            Title = "Greater Riverton Floods",
            Subtitle = "Help centres and shelters are open",
            Status = "Active Local Response",
            Region = "Greater Riverton",
            ActiveAlertTitle = "Evacuation notice for Riverside blocks A-D",
            ActiveAlertDescription = "Use Market Road to reach North Community Hall. Do not use Riverside Bridge.",
            ActiveAlertSeverity = "Critical",
            Helpline = "112",
            CreatedAt = DateTime.UtcNow.AddDays(-3)
        };

        var initialRequests = new List<CitizenRequest>
        {
            new()
            {
                Id = "REQ-2841",
                Title = "Emergency shelter",
                Category = "Emergency shelter",
                Description = "A case worker is checking your information.",
                Status = "Under review",
                Priority = "High",
                CitizenName = "Samira Khan",
                ContactPhone = "+94 77 123 4567",
                Address = "North Ward, Greater Riverton",
                Color = "blue",
                CreatedAt = DateTime.UtcNow.AddMinutes(-8)
            },
            new()
            {
                Id = "REQ-2794",
                Title = "Food and drinking water",
                Category = "Food and water",
                Description = "Delivery DSP-1092 is expected by 14:20.",
                Status = "Help on the way",
                Priority = "Normal",
                CitizenName = "Samira Khan",
                ContactPhone = "+94 77 123 4567",
                Address = "North Ward, Greater Riverton",
                Color = "green",
                CreatedAt = DateTime.UtcNow.AddHours(-3).AddMinutes(-40)
            },
            new()
            {
                Id = "REQ-2812",
                Title = "Medical support",
                Category = "Medical support",
                Description = "Support was delivered and confirmed.",
                Status = "Completed",
                Priority = "Normal",
                CitizenName = "Samira Khan",
                ContactPhone = "+94 77 123 4567",
                Address = "North Ward, Greater Riverton",
                Color = "green",
                CreatedAt = DateTime.UtcNow.AddDays(-2).AddHours(-4)
            }
        };

        foreach (var r in initialRequests)
        {
            _requests[r.Id] = r;
        }

        var initialFacilities = new List<NearbyFacility>
        {
            new()
            {
                Id = "1",
                Name = "North Community Support Centre",
                Category = "Support centre",
                Distance = "0.8 km",
                DistanceKm = 0.8,
                Hours = "Open until 20:00",
                Status = "Open",
                Address = "North Ward",
                Phone = "+94 21 222 1001",
                Services = ["Food", "Water", "Charging"],
                Description = "A local support centre providing essential supplies, food, water and charging facilities.",
                Color = "green",
                MapX = 25,
                MapY = 23,
                IconKey = "House"
            },
            new()
            {
                Id = "2",
                Name = "Riverton High School",
                Category = "Emergency shelter",
                Distance = "1.4 km",
                DistanceKm = 1.4,
                Hours = "Open 24 hours",
                Status = "Space available",
                Address = "Riverton",
                Phone = "+94 21 222 1002",
                Services = ["Accessible", "Medical"],
                Description = "Emergency accommodation for families and displaced residents.",
                Color = "green",
                MapX = 66,
                MapY = 26,
                IconKey = "House"
            },
            new()
            {
                Id = "3",
                Name = "Eastbank Mobile Clinic",
                Category = "Medical support",
                Distance = "2.1 km",
                DistanceKm = 2.1,
                Hours = "Open until 18:00",
                Status = "Open",
                Address = "Eastbank",
                Phone = "+94 21 222 1003",
                Services = ["First aid", "Medicines"],
                Description = "Mobile medical assistance, first aid and essential medicines.",
                Color = "green",
                MapX = 31,
                MapY = 72,
                IconKey = "Cross"
            },
            new()
            {
                Id = "4",
                Name = "St. Mary Civic Centre",
                Category = "Emergency shelter",
                Distance = "3.2 km",
                DistanceKm = 3.2,
                Hours = "Currently full",
                Status = "Full",
                Address = "Central Ward",
                Phone = "+94 21 222 1004",
                Services = ["Food", "Medical", "Childcare"],
                Description = "Community emergency shelter. Currently at full capacity.",
                Color = "red",
                MapX = 86,
                MapY = 64,
                IconKey = "House"
            }
        };

        foreach (var f in initialFacilities)
        {
            _facilities[f.Id] = f;
        }
    }
}
