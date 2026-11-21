using System.Text;
using CareLink.Backend.Models;

namespace CareLink.Backend.Services;

public class PriorityQueueService : IPriorityQueueService
{
    private readonly List<PriorityQueueItem> _cases = [];
    private readonly object _lock = new();

    public PriorityQueueService()
    {
        InitializeSeedData();
    }

    private void InitializeSeedData()
    {
        var items = new List<PriorityQueueItem>
        {
            new()
            {
                Id = "REQ-2841",
                Category = "Emergency shelter",
                Applicant = "D. Jayawardena - 6 people",
                Location = "Riverside, North Ward",
                Priority = "Critical",
                AiPriority = "Critical",
                Confidence = "94% confidence",
                Verification = "Evidence pending",
                Assigned = "You",
                Age = "18 min",
                Status = "Under Review",
                Event = "Flood Response",
                Minutes = 18,
                CreatedAt = DateTime.UtcNow.AddMinutes(-18)
            },
            new()
            {
                Id = "REQ-2839",
                Category = "Medical supplies",
                Applicant = "Community Clinic - 48 people",
                Location = "Old Town, Sector 4",
                Priority = "High",
                AiPriority = "High",
                Confidence = "91% confidence",
                Verification = "Verified",
                Assigned = "You",
                Age = "34 min",
                Status = "Assigned",
                Event = "Flood Response",
                Minutes = 34,
                CreatedAt = DateTime.UtcNow.AddMinutes(-34)
            },
            new()
            {
                Id = "REQ-2836",
                Category = "Food & water",
                Applicant = "M. Rasheed - 3 people",
                Location = "Eastbank, Sector 2",
                Priority = "High",
                AiPriority = "High",
                Confidence = "87% confidence",
                Verification = "Contact verified",
                Assigned = "Unassigned",
                Age = "49 min",
                Status = "Submitted",
                Event = "Flood Response",
                Minutes = 49,
                CreatedAt = DateTime.UtcNow.AddMinutes(-49)
            },
            new()
            {
                Id = "REQ-2821",
                Category = "Rescue assistance",
                Applicant = "P. Nadarajah - 5 people",
                Location = "North Ward",
                Priority = "Critical",
                AiPriority = "Critical",
                Confidence = "96% confidence",
                Verification = "Verified",
                Assigned = "You",
                Age = "1h 34m",
                Status = "Escalated",
                Event = "Storm Response",
                Minutes = 94,
                CreatedAt = DateTime.UtcNow.AddMinutes(-94)
            },
            new()
            {
                Id = "REQ-2814",
                Category = "Medicine",
                Applicant = "R. Perera - 1 people",
                Location = "West District",
                Priority = "High",
                AiPriority = "Medium",
                Confidence = "74% confidence",
                Verification = "Needs information",
                Assigned = "Unassigned",
                Age = "2h 08m",
                Status = "Pending Info",
                Event = "Flood Response",
                Minutes = 128,
                CreatedAt = DateTime.UtcNow.AddMinutes(-128)
            },
            new()
            {
                Id = "REQ-2802",
                Category = "Clean water",
                Applicant = "Community Center - 60 people",
                Location = "South Ward, Sector 3",
                Priority = "Critical",
                AiPriority = "Critical",
                Confidence = "93% confidence",
                Verification = "Verified",
                Assigned = "Unassigned",
                Age = "45 min",
                Status = "Submitted",
                Event = "Flood Response",
                Minutes = 45,
                CreatedAt = DateTime.UtcNow.AddMinutes(-45)
            },
            new()
            {
                Id = "REQ-2795",
                Category = "Emergency shelter",
                Applicant = "A. Silva - 4 people",
                Location = "Lowland Ward",
                Priority = "Critical",
                AiPriority = "High",
                Confidence = "89% confidence",
                Verification = "Evidence pending",
                Assigned = "Unassigned",
                Age = "52 min",
                Status = "Under Review",
                Event = "Storm Response",
                Minutes = 52,
                CreatedAt = DateTime.UtcNow.AddMinutes(-52)
            }
        };

        _cases.AddRange(items);
    }

    public Task<PriorityQueueStatsDto> GetStatsAsync()
    {
        lock (_lock)
        {
            var stats = new PriorityQueueStatsDto
            {
                TotalCases = _cases.Count,
                CriticalCount = _cases.Count(c => c.Priority.Equals("Critical", StringComparison.OrdinalIgnoreCase)),
                DueSoonCount = _cases.Count(c => c.Minutes <= 60),
                PendingInfoCount = _cases.Count(c => c.Status.Equals("Pending Info", StringComparison.OrdinalIgnoreCase)),
                RecentlyUpdatedCount = _cases.Count(c => c.Status.Equals("Under Review", StringComparison.OrdinalIgnoreCase) || c.Status.Equals("Assigned", StringComparison.OrdinalIgnoreCase)),
                OverdueCount = _cases.Count(c => c.Priority.Equals("Critical", StringComparison.OrdinalIgnoreCase) && c.Minutes > 30)
            };

            return Task.FromResult(stats);
        }
    }

    public Task<PaginatedResult<PriorityQueueItem>> GetCasesAsync(PriorityQueueQueryDto query)
    {
        lock (_lock)
        {
            var queryable = _cases.AsEnumerable();

            // Tab filtering
            if (!string.IsNullOrWhiteSpace(query.Tab) && !query.Tab.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                if (query.Tab.Equals("Critical", StringComparison.OrdinalIgnoreCase))
                {
                    queryable = queryable.Where(c => c.Priority.Equals("Critical", StringComparison.OrdinalIgnoreCase));
                }
                else if (query.Tab.Equals("Due soon", StringComparison.OrdinalIgnoreCase))
                {
                    queryable = queryable.Where(c => c.Minutes <= 60);
                }
                else if (query.Tab.Equals("Pending information", StringComparison.OrdinalIgnoreCase))
                {
                    queryable = queryable.Where(c => c.Status.Equals("Pending Info", StringComparison.OrdinalIgnoreCase));
                }
                else if (query.Tab.Equals("Recently updated", StringComparison.OrdinalIgnoreCase))
                {
                    queryable = queryable.Where(c => c.Status.Equals("Under Review", StringComparison.OrdinalIgnoreCase) || c.Status.Equals("Assigned", StringComparison.OrdinalIgnoreCase));
                }
            }

            // Keyword Search
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var s = query.Search.Trim().ToLower();
                queryable = queryable.Where(c =>
                    c.Id.ToLower().Contains(s) ||
                    c.Applicant.ToLower().Contains(s) ||
                    c.Location.ToLower().Contains(s) ||
                    c.Category.ToLower().Contains(s));
            }

            // Crisis Event Filter
            if (!string.IsNullOrWhiteSpace(query.Event) && !query.Event.Equals("All crisis events", StringComparison.OrdinalIgnoreCase))
            {
                queryable = queryable.Where(c => c.Event.Equals(query.Event, StringComparison.OrdinalIgnoreCase));
            }

            // Priority Filter
            if (!string.IsNullOrWhiteSpace(query.Priority) && !query.Priority.Equals("All priorities", StringComparison.OrdinalIgnoreCase))
            {
                queryable = queryable.Where(c => c.Priority.Equals(query.Priority, StringComparison.OrdinalIgnoreCase));
            }

            // Category Filter
            if (!string.IsNullOrWhiteSpace(query.Category) && !query.Category.Equals("All categories", StringComparison.OrdinalIgnoreCase))
            {
                queryable = queryable.Where(c => c.Category.Equals(query.Category, StringComparison.OrdinalIgnoreCase));
            }

            // Status Filter
            if (!string.IsNullOrWhiteSpace(query.Status) && !query.Status.Equals("All statuses", StringComparison.OrdinalIgnoreCase))
            {
                queryable = queryable.Where(c => c.Status.Equals(query.Status, StringComparison.OrdinalIgnoreCase));
            }

            // Date Range
            if (!string.IsNullOrWhiteSpace(query.DateRange) && !query.DateRange.Equals("Date range", StringComparison.OrdinalIgnoreCase))
            {
                if (query.DateRange.Equals("Last 1 hour", StringComparison.OrdinalIgnoreCase))
                    queryable = queryable.Where(c => c.Minutes <= 60);
                else if (query.DateRange.Equals("Last 24 hours", StringComparison.OrdinalIgnoreCase))
                    queryable = queryable.Where(c => c.Minutes <= 1440);
            }

            var list = queryable.OrderBy(c => c.Priority == "Critical" ? 0 : c.Priority == "High" ? 1 : 2)
                                .ThenBy(c => c.Minutes)
                                .ToList();

            var totalCount = list.Count;
            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize < 1 ? 5 : query.PageSize;

            var items = list.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return Task.FromResult(new PaginatedResult<PriorityQueueItem>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            });
        }
    }

    public Task<PriorityQueueItem?> GetByIdAsync(string id)
    {
        lock (_lock)
        {
            var item = _cases.FirstOrDefault(c => c.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(item);
        }
    }

    public Task<int> ClaimCasesAsync(ClaimPriorityCasesRequest request)
    {
        lock (_lock)
        {
            var count = 0;
            foreach (var id in request.CaseIds)
            {
                var item = _cases.FirstOrDefault(c => c.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
                if (item != null)
                {
                    item.Assigned = string.IsNullOrWhiteSpace(request.AssignTo) ? "You" : request.AssignTo;
                    item.Status = "Assigned";
                    count++;
                }
            }
            return Task.FromResult(count);
        }
    }

    public Task<PriorityQueueItem?> UpdateCaseAsync(string id, UpdatePriorityCaseRequest request)
    {
        lock (_lock)
        {
            var item = _cases.FirstOrDefault(c => c.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (item != null)
            {
                if (!string.IsNullOrWhiteSpace(request.Priority)) item.Priority = request.Priority;
                if (!string.IsNullOrWhiteSpace(request.Status)) item.Status = request.Status;
                if (!string.IsNullOrWhiteSpace(request.Assigned)) item.Assigned = request.Assigned;
                if (!string.IsNullOrWhiteSpace(request.Notes)) item.Notes = request.Notes;
                return Task.FromResult<PriorityQueueItem?>(item);
            }
            return Task.FromResult<PriorityQueueItem?>(null);
        }
    }

    public Task<byte[]> ExportCsvAsync(PriorityQueueQueryDto query)
    {
        lock (_lock)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Case ID,Category,Applicant,Location,Priority,AI Priority,Confidence,Verification,Assigned,Age,Status,Event");

            foreach (var item in _cases)
            {
                sb.AppendLine($"\"{item.Id}\",\"{item.Category}\",\"{item.Applicant}\",\"{item.Location}\",\"{item.Priority}\",\"{item.AiPriority}\",\"{item.Confidence}\",\"{item.Verification}\",\"{item.Assigned}\",\"{item.Age}\",\"{item.Status}\",\"{item.Event}\"");
            }

            return Task.FromResult(Encoding.UTF8.GetBytes(sb.ToString()));
        }
    }
}
