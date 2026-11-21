using System.Text;
using CareLink.Backend.Models;

namespace CareLink.Backend.Services;

public class MyCasesService : IMyCasesService
{
    private readonly object _lock = new();
    private readonly List<MyCaseItem> _cases = [];
    private int _nextId = 2842;

    public MyCasesService()
    {
        SeedInitialData();
    }

    private void SeedInitialData()
    {
        _cases.AddRange([
            new MyCaseItem
            {
                Id = "REQ-2841",
                Category = "Emergency shelter",
                Applicant = "D. Jayawardena - 6 people",
                Location = "Riverside, North Ward",
                Priority = "Critical",
                Confidence = "94% confidence",
                Verification = "Evidence pending",
                Assigned = "You",
                Age = "18 min",
                Status = "Under Review",
                Event = "Flood Response",
                Description = "Ground floor flooded; temporary shelter placement required.",
                CreatedAt = DateTime.UtcNow.AddMinutes(-18)
            },
            new MyCaseItem
            {
                Id = "REQ-2839",
                Category = "Medical supplies",
                Applicant = "Community Clinic - 48 people",
                Location = "Old Town, Sector 4",
                Priority = "High",
                Confidence = "91% confidence",
                Verification = "Verified",
                Assigned = "You",
                Age = "34 min",
                Status = "Assigned",
                Event = "Flood Response",
                Description = "Clinic urgent replenishment for clean bandages and antibiotics.",
                CreatedAt = DateTime.UtcNow.AddMinutes(-34)
            },
            new MyCaseItem
            {
                Id = "REQ-2821",
                Category = "Rescue assistance",
                Applicant = "P. Nadarajah - 5 people",
                Location = "North Ward",
                Priority = "Critical",
                Confidence = "96% confidence",
                Verification = "Verified",
                Assigned = "You",
                Age = "1h 34m",
                Status = "Escalated",
                Event = "Flood Response",
                Description = "Stranded family; coordinating rescue dispatch.",
                CreatedAt = DateTime.UtcNow.AddMinutes(-94)
            },
            new MyCaseItem
            {
                Id = "REQ-2818",
                Category = "Food & water",
                Applicant = "S. Fernando - 4 people",
                Location = "Eastbank, Sector 2",
                Priority = "High",
                Confidence = "88% confidence",
                Verification = "Verified",
                Assigned = "You",
                Age = "2h 15m",
                Status = "Assigned",
                Event = "Storm Response",
                Description = "Drinking water and dry rations delivery scheduled.",
                CreatedAt = DateTime.UtcNow.AddMinutes(-135)
            },
            new MyCaseItem
            {
                Id = "REQ-2805",
                Category = "Emergency shelter",
                Applicant = "R. Silva - 3 people",
                Location = "West Ward",
                Priority = "Medium",
                Confidence = "85% confidence",
                Verification = "Evidence pending",
                Assigned = "You",
                Age = "3h 20m",
                Status = "Submitted",
                Event = "Flood Response",
                Description = "Assessing evacuation shelter availability.",
                CreatedAt = DateTime.UtcNow.AddMinutes(-200)
            },
            new MyCaseItem
            {
                Id = "REQ-2798",
                Category = "Medical supplies",
                Applicant = "K. Perera - 2 people",
                Location = "South Ward",
                Priority = "High",
                Confidence = "90% confidence",
                Verification = "Verified",
                Assigned = "You",
                Age = "4h 10m",
                Status = "Under Review",
                Event = "Flood Response",
                Description = "Senior family member requires daily insulin therapy delivery.",
                CreatedAt = DateTime.UtcNow.AddMinutes(-250)
            },
            new MyCaseItem
            {
                Id = "REQ-2790",
                Category = "Food & water",
                Applicant = "A. Gunasekara - 5 people",
                Location = "Marshland, Sector 2",
                Priority = "Medium",
                Confidence = "84% confidence",
                Verification = "Verified",
                Assigned = "You",
                Age = "5h 05m",
                Status = "Assigned",
                Event = "Storm Response",
                Description = "Clean water jerry cans and sanitation kit.",
                CreatedAt = DateTime.UtcNow.AddMinutes(-305)
            },
            new MyCaseItem
            {
                Id = "REQ-2782",
                Category = "Medicine",
                Applicant = "M. Wickramasinghe - 1 person",
                Location = "Old Town",
                Priority = "Critical",
                Confidence = "93% confidence",
                Verification = "Evidence pending",
                Assigned = "You",
                Age = "1h 10m",
                Status = "Under Review",
                Event = "Flood Response",
                Description = "Cardiac prescription medication urgently required.",
                CreatedAt = DateTime.UtcNow.AddMinutes(-70)
            }
        ]);
    }

    public Task<MyCasesStatsDto> GetStatsAsync()
    {
        lock (_lock)
        {
            var active = _cases.Where(c => c.Assigned == "You").ToList();
            var stats = new MyCasesStatsDto
            {
                TotalCases = active.Count,
                CriticalCount = active.Count(c => c.Priority == "Critical"),
                DueSoonCount = active.Count(c => c.Priority == "Critical" || c.Priority == "High"),
                PendingInfoCount = active.Count(c => c.Verification == "Evidence pending"),
                RecentlyUpdatedCount = active.Count(c => c.Status == "Under Review" || c.Status == "Assigned")
            };

            return Task.FromResult(stats);
        }
    }

    public Task<PaginatedResult<MyCaseItem>> GetCasesAsync(MyCasesQueryDto query)
    {
        lock (_lock)
        {
            var filtered = _cases.Where(c => c.Assigned == "You").AsEnumerable();

            // Tab Filter
            var tab = query.Tab?.Trim() ?? "All";
            if (tab.Equals("Critical", StringComparison.OrdinalIgnoreCase))
            {
                filtered = filtered.Where(c => c.Priority == "Critical");
            }
            else if (tab.Equals("Due soon", StringComparison.OrdinalIgnoreCase))
            {
                filtered = filtered.Where(c => c.Priority == "Critical" || c.Priority == "High");
            }
            else if (tab.Equals("Pending information", StringComparison.OrdinalIgnoreCase))
            {
                filtered = filtered.Where(c => c.Verification == "Evidence pending");
            }
            else if (tab.Equals("Recently updated", StringComparison.OrdinalIgnoreCase))
            {
                filtered = filtered.Where(c => c.Status == "Under Review" || c.Status == "Assigned");
            }

            // Keyword Search
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var s = query.Search.Trim().ToLowerInvariant();
                filtered = filtered.Where(c =>
                    c.Id.ToLowerInvariant().Contains(s) ||
                    c.Applicant.ToLowerInvariant().Contains(s) ||
                    c.Location.ToLowerInvariant().Contains(s) ||
                    c.Category.ToLowerInvariant().Contains(s));
            }

            // Event Filter
            if (!string.IsNullOrWhiteSpace(query.Event) && query.Event != "All crisis events")
            {
                filtered = filtered.Where(c => c.Event.Equals(query.Event, StringComparison.OrdinalIgnoreCase));
            }

            // Priority Filter
            if (!string.IsNullOrWhiteSpace(query.Priority) && query.Priority != "All priorities")
            {
                filtered = filtered.Where(c => c.Priority.Equals(query.Priority, StringComparison.OrdinalIgnoreCase));
            }

            // Category Filter
            if (!string.IsNullOrWhiteSpace(query.Category) && query.Category != "All categories")
            {
                filtered = filtered.Where(c => c.Category.Equals(query.Category, StringComparison.OrdinalIgnoreCase));
            }

            // Status Filter
            if (!string.IsNullOrWhiteSpace(query.Status) && query.Status != "All statuses")
            {
                filtered = filtered.Where(c => c.Status.Equals(query.Status, StringComparison.OrdinalIgnoreCase));
            }

            // Date Range Filter
            if (!string.IsNullOrWhiteSpace(query.DateRange) && query.DateRange != "Date range")
            {
                if (query.DateRange.Equals("Today", StringComparison.OrdinalIgnoreCase))
                {
                    filtered = filtered.Where(c => c.CreatedAt >= DateTime.UtcNow.Date);
                }
                else if (query.DateRange.Contains("7 days", StringComparison.OrdinalIgnoreCase))
                {
                    filtered = filtered.Where(c => c.CreatedAt >= DateTime.UtcNow.AddDays(-7));
                }
            }

            var totalCount = filtered.Count();
            var page = Math.Max(1, query.PageNumber);
            var pageSize = Math.Max(1, query.PageSize);
            var items = filtered
                .OrderByDescending(c => c.Priority == "Critical")
                .ThenByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var result = new PaginatedResult<MyCaseItem>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };

            return Task.FromResult(result);
        }
    }

    public Task<MyCaseItem?> GetCaseByIdAsync(string id)
    {
        lock (_lock)
        {
            var item = _cases.FirstOrDefault(c => c.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(item);
        }
    }

    public Task<MyCaseItem> CreateCaseAsync(CreateMyCaseRequest request)
    {
        lock (_lock)
        {
            var id = $"REQ-{_nextId++}";
            var newCase = new MyCaseItem
            {
                Id = id,
                Category = request.Category,
                Applicant = $"{request.ApplicantName} - {request.PeopleCount} people",
                Location = request.Location,
                Priority = request.Priority,
                Confidence = "89% confidence",
                Verification = "Evidence pending",
                Assigned = "You",
                Age = "Just now",
                Status = "Assigned",
                Event = request.Event,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow
            };

            _cases.Insert(0, newCase);
            return Task.FromResult(newCase);
        }
    }

    public Task<MyCaseItem?> UpdateStatusAsync(string id, UpdateMyCaseStatusRequest request)
    {
        lock (_lock)
        {
            var item = _cases.FirstOrDefault(c => c.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (item == null) return Task.FromResult<MyCaseItem?>(null);

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                item.Status = request.Status;
            }

            if (!string.IsNullOrWhiteSpace(request.Priority))
            {
                item.Priority = request.Priority;
            }

            if (!string.IsNullOrWhiteSpace(request.Notes))
            {
                item.Notes = request.Notes;
            }

            return Task.FromResult<MyCaseItem?>(item);
        }
    }

    public Task<int> BulkActionAsync(BulkMyCasesActionRequest request)
    {
        lock (_lock)
        {
            var count = 0;
            var action = request.Action.Trim().ToLowerInvariant();

            foreach (var id in request.CaseIds)
            {
                var item = _cases.FirstOrDefault(c => c.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
                if (item == null) continue;

                if (action == "resolve")
                {
                    item.Status = "Resolved";
                }
                else if (action == "escalate")
                {
                    item.Status = "Escalated";
                    item.Priority = "Critical";
                }
                else if (action == "underreview")
                {
                    item.Status = "Under Review";
                }
                else if (action == "reassign" && !string.IsNullOrWhiteSpace(request.AssignTo))
                {
                    item.Assigned = request.AssignTo;
                }

                if (!string.IsNullOrWhiteSpace(request.Notes))
                {
                    item.Notes = request.Notes;
                }

                count++;
            }

            return Task.FromResult(count);
        }
    }

    public async Task<byte[]> ExportCsvAsync(MyCasesQueryDto query)
    {
        var result = await GetCasesAsync(new MyCasesQueryDto
        {
            Tab = query.Tab,
            Search = query.Search,
            Priority = query.Priority,
            Category = query.Category,
            Status = query.Status,
            Event = query.Event,
            DateRange = query.DateRange,
            PageNumber = 1,
            PageSize = 1000
        });

        var sb = new StringBuilder();
        sb.AppendLine("Case ID,Category,Applicant,Location,Priority,Verification,Assigned,Age,Status,Crisis Event,Description,Notes");

        foreach (var c in result.Items)
        {
            sb.AppendLine($"\"{c.Id}\",\"{c.Category}\",\"{c.Applicant}\",\"{c.Location}\",\"{c.Priority}\",\"{c.Verification}\",\"{c.Assigned}\",\"{c.Age}\",\"{c.Status}\",\"{c.Event}\",\"{c.Description}\",\"{c.Notes}\"");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }
}
