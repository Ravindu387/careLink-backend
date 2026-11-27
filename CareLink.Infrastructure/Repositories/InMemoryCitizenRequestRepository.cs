namespace CareLink.Infrastructure.Repositories;

using System.Collections.Concurrent;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;

public class InMemoryCitizenRequestRepository : ICitizenRequestRepository
{
    private readonly ConcurrentDictionary<string, CitizenRequest> _requests = new();
    private readonly Lock _lock = new();
    private int _nextIdCounter = 2842;

    public InMemoryCitizenRequestRepository()
    {
        SeedDefaultRequests();
    }

    private void SeedDefaultRequests()
    {
        var seedList = new List<CitizenRequest>
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
                Address = "54 Riverside Lane, North Ward, Greater Riverton",
                District = "North Ward",
                Color = "amber",
                ImmediateThreat = false,
                PeopleCount = 3,
                Progress = 2,
                CreatedAt = DateTime.UtcNow.AddMinutes(-38),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-8)
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
                Address = "54 Riverside Lane, North Ward, Greater Riverton",
                District = "North Ward",
                Color = "blue",
                ImmediateThreat = false,
                PeopleCount = 4,
                Progress = 3,
                DispatchTrackingId = "DSP-1092",
                CreatedAt = DateTime.UtcNow.AddHours(-26),
                UpdatedAt = DateTime.UtcNow.AddHours(-4)
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
                Address = "54 Riverside Lane, North Ward, Greater Riverton",
                District = "North Ward",
                Color = "green",
                ImmediateThreat = false,
                PeopleCount = 1,
                Progress = 4,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            }
        };

        foreach (var req in seedList)
        {
            _requests[req.Id] = req;
        }
    }

    public Task<IEnumerable<CitizenRequest>> GetRequestsAsync(string? citizenName = null, string? search = null, string? status = null, string? category = null)
    {
        var query = _requests.Values.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(citizenName))
        {
            var trimmedName = citizenName.Trim();
            if (trimmedName.Equals("Citizen User", StringComparison.OrdinalIgnoreCase) ||
                trimmedName.Equals("Samira Khan", StringComparison.OrdinalIgnoreCase) ||
                trimmedName.Equals("citizen@carelink.org", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(r =>
                    r.CitizenName.Equals("Citizen User", StringComparison.OrdinalIgnoreCase) ||
                    r.CitizenName.Equals("Samira Khan", StringComparison.OrdinalIgnoreCase) ||
                    r.CitizenName.Equals("citizen@carelink.org", StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                query = query.Where(r => r.CitizenName.Equals(trimmedName, StringComparison.OrdinalIgnoreCase));
            }
        }

        if (!string.IsNullOrWhiteSpace(status) && !status.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(r => r.Status.Equals(status.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(category) && !category.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(r => r.Category.Equals(category.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(r =>
                r.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                r.Description.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                r.Category.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                r.Address.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                r.Id.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        var results = query.OrderByDescending(r => r.CreatedAt).ToList();
        return Task.FromResult<IEnumerable<CitizenRequest>>(results);
    }

    public Task<CitizenRequest?> GetByIdAsync(string id)
    {
        _requests.TryGetValue(id.Trim(), out var req);
        return Task.FromResult(req);
    }

    public Task<CitizenRequest> CreateRequestAsync(CitizenRequest request)
    {
        lock (_lock)
        {
            if (string.IsNullOrWhiteSpace(request.Id))
            {
                request.Id = $"REQ-{_nextIdCounter++}";
            }

            request.CreatedAt = DateTime.UtcNow;
            request.UpdatedAt = DateTime.UtcNow;
            _requests[request.Id] = request;
            return Task.FromResult(request);
        }
    }

    public Task<CitizenRequest?> UpdateStatusAsync(string id, string status, int? progress = null)
    {
        lock (_lock)
        {
            if (_requests.TryGetValue(id.Trim(), out var req))
            {
                req.Status = status.Trim();
                if (progress.HasValue)
                {
                    req.Progress = progress.Value;
                }
                else
                {
                    req.Progress = status.ToLowerInvariant() switch
                    {
                        "sent" => 1,
                        "under review" => 2,
                        "help arranged" or "help on the way" => 3,
                        "completed" => 4,
                        _ => req.Progress
                    };
                }
                req.UpdatedAt = DateTime.UtcNow;
                return Task.FromResult<CitizenRequest?>(req);
            }
            return Task.FromResult<CitizenRequest?>(null);
        }
    }

    public Task<bool> DeleteRequestAsync(string id)
    {
        return Task.FromResult(_requests.TryRemove(id.Trim(), out _));
    }

    public Task<CitizenRequestsSummaryDto> GetSummaryAsync(string? citizenName = null)
    {
        var query = _requests.Values.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(citizenName))
        {
            var trimmedName = citizenName.Trim();
            if (trimmedName.Equals("Citizen User", StringComparison.OrdinalIgnoreCase) ||
                trimmedName.Equals("Samira Khan", StringComparison.OrdinalIgnoreCase) ||
                trimmedName.Equals("citizen@carelink.org", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(r =>
                    r.CitizenName.Equals("Citizen User", StringComparison.OrdinalIgnoreCase) ||
                    r.CitizenName.Equals("Samira Khan", StringComparison.OrdinalIgnoreCase) ||
                    r.CitizenName.Equals("citizen@carelink.org", StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                query = query.Where(r => r.CitizenName.Equals(trimmedName, StringComparison.OrdinalIgnoreCase));
            }
        }

        var list = query.ToList();
        var summary = new CitizenRequestsSummaryDto
        {
            TotalRequests = list.Count,
            ActiveCount = list.Count(r => r.Progress < 4 && !r.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase)),
            CompletedCount = list.Count(r => r.Progress == 4 || r.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase)),
            DraftsCount = 0
        };

        return Task.FromResult(summary);
    }
}
