using System.Text;
using CareLink.Backend.Models;

namespace CareLink.Backend.Services;

public class RequestHistoryService : IRequestHistoryService
{
    private readonly List<HistoryEventItem> _events = [];
    private readonly object _lock = new();

    public RequestHistoryService()
    {
        InitializeSeedData();
    }

    private void InitializeSeedData()
    {
        var items = new List<HistoryEventItem>
        {
            new()
            {
                Id = "EVT-4821",
                CaseId = "REQ-2841",
                Action = "Priority recommendation reviewed",
                Actor = "Amina Mensah",
                Change = "Critical → Critical",
                Rationale = "AI rationale approved",
                Timestamp = "22 Jun · 13:42",
                EventType = "Decision",
                CreatedAt = DateTime.UtcNow.AddMinutes(-18)
            },
            new()
            {
                Id = "EVT-4818",
                CaseId = "REQ-2839",
                Action = "Case assigned",
                Actor = "System Queue",
                Change = "Unassigned → Medical Team 3",
                Rationale = "Skills and proximity",
                Timestamp = "22 Jun · 13:28",
                EventType = "Assignment",
                CreatedAt = DateTime.UtcNow.AddMinutes(-34)
            },
            new()
            {
                Id = "EVT-4811",
                CaseId = "REQ-2828",
                Action = "Additional information received",
                Actor = "Citizen Portal",
                Change = "Pending → Verified",
                Rationale = "Contact confirmed",
                Timestamp = "22 Jun · 12:55",
                EventType = "Verification",
                CreatedAt = DateTime.UtcNow.AddMinutes(-65)
            },
            new()
            {
                Id = "EVT-4804",
                CaseId = "REQ-2821",
                Action = "Case escalated",
                Actor = "Nadia Okafor",
                Change = "High → Critical",
                Rationale = "Immediate life risk",
                Timestamp = "22 Jun · 12:31",
                EventType = "Escalation",
                CreatedAt = DateTime.UtcNow.AddMinutes(-90)
            },
            new()
            {
                Id = "EVT-4798",
                CaseId = "REQ-2814",
                Action = "Information requested",
                Actor = "You",
                Change = "Under Review → Pending Info",
                Rationale = "Prescription required",
                Timestamp = "22 Jun · 11:58",
                EventType = "Information",
                CreatedAt = DateTime.UtcNow.AddMinutes(-122)
            },
            new()
            {
                Id = "EVT-4792",
                CaseId = "REQ-2809",
                Action = "Evidence verified",
                Actor = "Amina Mensah",
                Change = "Pending → Verified",
                Rationale = "Documents approved",
                Timestamp = "22 Jun · 11:30",
                EventType = "Verification",
                CreatedAt = DateTime.UtcNow.AddMinutes(-150)
            },
            new()
            {
                Id = "EVT-4786",
                CaseId = "REQ-2798",
                Action = "Case created",
                Actor = "Citizen Portal",
                Change = "New → Submitted",
                Rationale = "Citizen assistance request",
                Timestamp = "22 Jun · 10:45",
                EventType = "Creation",
                CreatedAt = DateTime.UtcNow.AddMinutes(-195)
            },
            new()
            {
                Id = "EVT-4779",
                CaseId = "REQ-2789",
                Action = "Case status updated",
                Actor = "Case Manager",
                Change = "Submitted → Under Review",
                Rationale = "Initial assessment completed",
                Timestamp = "22 Jun · 10:12",
                EventType = "Status Change",
                CreatedAt = DateTime.UtcNow.AddMinutes(-230)
            }
        };

        _events.AddRange(items);
    }

    public Task<RequestHistoryStatsDto> GetStatsAsync()
    {
        lock (_lock)
        {
            var stats = new RequestHistoryStatsDto
            {
                TotalEvents = 48214, // Historical aggregate baseline
                DecisionsCount = _events.Count(e => e.EventType.Equals("Decision", StringComparison.OrdinalIgnoreCase)),
                AssignmentsCount = _events.Count(e => e.EventType.Equals("Assignment", StringComparison.OrdinalIgnoreCase)),
                VerificationsCount = _events.Count(e => e.EventType.Equals("Verification", StringComparison.OrdinalIgnoreCase)),
                EscalationsCount = _events.Count(e => e.EventType.Equals("Escalation", StringComparison.OrdinalIgnoreCase))
            };

            return Task.FromResult(stats);
        }
    }

    public Task<PaginatedResult<HistoryEventItem>> GetEventsAsync(RequestHistoryQueryDto query)
    {
        lock (_lock)
        {
            var queryable = _events.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var s = query.Search.Trim().ToLower();
                queryable = queryable.Where(e =>
                    e.Id.ToLower().Contains(s) ||
                    e.CaseId.ToLower().Contains(s) ||
                    e.Action.ToLower().Contains(s) ||
                    e.Actor.ToLower().Contains(s) ||
                    e.Rationale.ToLower().Contains(s) ||
                    e.Change.ToLower().Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(query.EventType) && !query.EventType.Equals("All event types", StringComparison.OrdinalIgnoreCase))
            {
                queryable = queryable.Where(e => e.EventType.Equals(query.EventType, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(query.Actor) && !query.Actor.Equals("All actors", StringComparison.OrdinalIgnoreCase))
            {
                queryable = queryable.Where(e => e.Actor.Equals(query.Actor, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(query.CaseId) && !query.CaseId.Equals("All cases", StringComparison.OrdinalIgnoreCase))
            {
                queryable = queryable.Where(e => e.CaseId.Equals(query.CaseId, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(query.DateRange) && !query.DateRange.Equals("Date range", StringComparison.OrdinalIgnoreCase))
            {
                if (query.DateRange.Equals("Today", StringComparison.OrdinalIgnoreCase))
                {
                    queryable = queryable.Where(e => e.Timestamp.Contains("22 Jun"));
                }
            }

            var list = queryable.OrderByDescending(e => e.CreatedAt).ToList();
            var totalCount = list.Count;
            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize < 1 ? 5 : query.PageSize;

            var items = list.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return Task.FromResult(new PaginatedResult<HistoryEventItem>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            });
        }
    }

    public Task<HistoryEventItem?> GetByIdAsync(string id)
    {
        lock (_lock)
        {
            var item = _events.FirstOrDefault(e => e.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(item);
        }
    }

    public Task<HistoryEventItem> LogEventAsync(LogHistoryEventRequest request)
    {
        lock (_lock)
        {
            var nextNum = 4822 + _events.Count;
            var newEvent = new HistoryEventItem
            {
                Id = $"EVT-{nextNum}",
                CaseId = request.CaseId,
                Action = request.Action,
                Actor = request.Actor,
                Change = request.Change,
                Rationale = request.Rationale,
                EventType = request.EventType,
                Details = request.Details,
                Timestamp = $"{DateTime.UtcNow:dd MMM · HH:mm}",
                CreatedAt = DateTime.UtcNow
            };

            _events.Insert(0, newEvent);
            return Task.FromResult(newEvent);
        }
    }

    public Task<List<string>> GetUniqueActorsAsync()
    {
        lock (_lock)
        {
            var actors = _events.Select(e => e.Actor).Distinct().OrderBy(a => a).ToList();
            return Task.FromResult(actors);
        }
    }

    public Task<List<string>> GetUniqueCaseIdsAsync()
    {
        lock (_lock)
        {
            var cases = _events.Select(e => e.CaseId).Distinct().OrderBy(c => c).ToList();
            return Task.FromResult(cases);
        }
    }

    public Task<byte[]> ExportCsvAsync(RequestHistoryQueryDto query)
    {
        lock (_lock)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Event ID,Case ID,Action,Actor,Change,Rationale,Timestamp,Event Type");

            foreach (var e in _events)
            {
                sb.AppendLine($"\"{e.Id}\",\"{e.CaseId}\",\"{e.Action}\",\"{e.Actor}\",\"{e.Change}\",\"{e.Rationale}\",\"{e.Timestamp}\",\"{e.EventType}\"");
            }

            return Task.FromResult(Encoding.UTF8.GetBytes(sb.ToString()));
        }
    }
}
