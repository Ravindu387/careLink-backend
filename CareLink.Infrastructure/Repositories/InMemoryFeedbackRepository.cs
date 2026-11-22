namespace CareLink.Infrastructure.Repositories;

using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;

public class InMemoryFeedbackRepository : IFeedbackRepository
{
    private readonly ConcurrentDictionary<string, CitizenFeedback> _feedbacks = new();
    private readonly Lock _lock = new();

    public InMemoryFeedbackRepository()
    {
        SeedInitialData();
    }

    public Task<IEnumerable<CitizenFeedback>> GetAllAsync()
    {
        var list = _feedbacks.Values.OrderByDescending(f => f.CreatedAt).AsEnumerable();
        return Task.FromResult(list);
    }

    public Task<CitizenFeedback?> GetByIdAsync(string id)
    {
        _feedbacks.TryGetValue(id, out var feedback);
        return Task.FromResult(feedback);
    }

    public Task<CitizenFeedback> CreateAsync(CitizenFeedback feedback)
    {
        lock (_lock)
        {
            if (string.IsNullOrWhiteSpace(feedback.Id))
            {
                feedback.Id = GenerateNextId();
            }

            if (feedback.CreatedAt == default)
            {
                feedback.CreatedAt = DateTime.UtcNow;
            }

            _feedbacks[feedback.Id] = feedback;
            return Task.FromResult(feedback);
        }
    }

    public Task<CitizenFeedback?> UpdateAsync(CitizenFeedback feedback)
    {
        lock (_lock)
        {
            if (!_feedbacks.ContainsKey(feedback.Id))
            {
                return Task.FromResult<CitizenFeedback?>(null);
            }

            feedback.UpdatedAt = DateTime.UtcNow;
            _feedbacks[feedback.Id] = feedback;
            return Task.FromResult<CitizenFeedback?>(feedback);
        }
    }

    public Task<bool> DeleteAsync(string id)
    {
        return Task.FromResult(_feedbacks.TryRemove(id, out _));
    }

    private string GenerateNextId()
    {
        var maxNumber = 1005;
        foreach (var key in _feedbacks.Keys)
        {
            var match = Regex.Match(key, @"\d+");
            if (match.Success && int.TryParse(match.Value, out var num))
            {
                if (num > maxNumber) maxNumber = num;
            }
        }

        return $"FB-{(maxNumber + 1):D4}";
    }

    private void SeedInitialData()
    {
        var seeds = new List<CitizenFeedback>
        {
            new()
            {
                Id = "FB-1001",
                Category = "Using the citizen portal",
                Rating = 5,
                Comment = "The portal made requesting flood assistance quick and straightforward. Interface is clear and very responsive.",
                Anonymous = false,
                SubmittedBy = "Samira Khan",
                Status = "Acknowledged",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new()
            {
                Id = "FB-1002",
                Category = "Shelter services",
                Rating = 4,
                Comment = "Shelter staff at North Ward were very accommodating. Clean drinking water and warm blankets were readily available.",
                Anonymous = true,
                SubmittedBy = "Anonymous Citizen",
                Status = "Under Review",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new()
            {
                Id = "FB-1003",
                Category = "Requesting help",
                Rating = 5,
                Comment = "Received relief supplies within 3 hours of submitting the emergency form. Huge thanks to the dispatch team!",
                Anonymous = false,
                SubmittedBy = "David Silva",
                Status = "Resolved",
                CreatedAt = DateTime.UtcNow.AddHours(-18)
            },
            new()
            {
                Id = "FB-1004",
                Category = "Safety updates",
                Rating = 4,
                Comment = "Alerts arrived on time regarding Riverton water levels. Kept our neighborhood well informed.",
                Anonymous = false,
                SubmittedBy = "Amara Patel",
                Status = "Acknowledged",
                CreatedAt = DateTime.UtcNow.AddHours(-10)
            },
            new()
            {
                Id = "FB-1005",
                Category = "Accessibility",
                Rating = 3,
                Comment = "Please add larger font size toggle or high contrast options for elderly and vision-impaired users.",
                Anonymous = true,
                SubmittedBy = "Anonymous Citizen",
                Status = "Under Review",
                CreatedAt = DateTime.UtcNow.AddHours(-4)
            }
        };

        foreach (var item in seeds)
        {
            _feedbacks[item.Id] = item;
        }
    }
}
