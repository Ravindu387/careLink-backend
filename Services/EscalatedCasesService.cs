using System.Text;
using CareLink.Backend.Models;

namespace CareLink.Backend.Services;

public class EscalatedCasesService : IEscalatedCasesService
{
    private readonly object _lock = new();
    private readonly List<EscalatedCaseItem> _cases = [];

    public EscalatedCasesService()
    {
        SeedInitialData();
    }

    private void SeedInitialData()
    {
        _cases.AddRange([
            new EscalatedCaseItem
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
                EscalationReason = "River embankment breach; rapid water level surge.",
                TargetOrg = "District Disaster Management Center",
                CoordinatorNotes = "Requires immediate multi-family temporary shelter placement.",
                CreatedAt = DateTime.UtcNow.AddMinutes(-18)
            },
            new EscalatedCaseItem
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
                EscalationReason = "High-voltage line fallen near flooded dwelling; naval boat access requested.",
                TargetOrg = "Navy Search & Rescue Unit",
                CoordinatorNotes = "Safety hazard verified by local field observer.",
                CreatedAt = DateTime.UtcNow.AddMinutes(-94)
            },
            new EscalatedCaseItem
            {
                Id = "REQ-2835",
                Category = "Medical supplies",
                Applicant = "H. Silva - 4 people",
                Location = "Old Town, Sector 4",
                Priority = "Critical",
                AiPriority = "Critical",
                Confidence = "91% confidence",
                Verification = "Verified",
                Assigned = "Coordinator Team",
                Age = "32 min",
                Status = "Escalated",
                Event = "Flood Response",
                Minutes = 32,
                EscalationReason = "Power outage affecting clinic oxygen and refrigeration supplies.",
                TargetOrg = "Red Cross Logistics",
                CoordinatorNotes = "Generator backup requested urgently.",
                CreatedAt = DateTime.UtcNow.AddMinutes(-32)
            },
            new EscalatedCaseItem
            {
                Id = "REQ-2819",
                Category = "Emergency shelter",
                Applicant = "N. Fernando - 8 people",
                Location = "Eastbank, Sector 3",
                Priority = "High",
                AiPriority = "High",
                Confidence = "88% confidence",
                Verification = "Contact verified",
                Assigned = "You",
                Age = "55 min",
                Status = "Escalated",
                Event = "Flood Response",
                Minutes = 55,
                EscalationReason = "Two wheelchair-bound elderly family members; requires disability-equipped shelter.",
                TargetOrg = "Social Services Relief Wing",
                CoordinatorNotes = "Accessible van transport mobilized.",
                CreatedAt = DateTime.UtcNow.AddMinutes(-55)
            },
            new EscalatedCaseItem
            {
                Id = "REQ-2807",
                Category = "Rescue assistance",
                Applicant = "B. Weerasinghe - 3 people",
                Location = "Marshland, Sector 1",
                Priority = "Critical",
                AiPriority = "Critical",
                Confidence = "95% confidence",
                Verification = "Verified",
                Assigned = "Coordinator Team",
                Age = "1h 12m",
                Status = "Escalated",
                Event = "Storm Response",
                Minutes = 72,
                EscalationReason = "Road access submerged by 4 feet of floodwater; trapped on upper floor.",
                TargetOrg = "Armed Forces Disaster Relief",
                CoordinatorNotes = "Informed rescue command center.",
                CreatedAt = DateTime.UtcNow.AddMinutes(-72)
            },
            new EscalatedCaseItem
            {
                Id = "REQ-2795",
                Category = "Food & water",
                Applicant = "M. Perera - 12 people",
                Location = "Riverview Community Center",
                Priority = "High",
                AiPriority = "High",
                Confidence = "86% confidence",
                Verification = "Contact verified",
                Assigned = "Unassigned",
                Age = "2h 20m",
                Status = "Under Review",
                Event = "Flood Response",
                Minutes = 140,
                EscalationReason = "Collective community shelter ran out of clean potable water.",
                TargetOrg = "National Water Supply Board",
                CoordinatorNotes = "Water bowser dispatch requested.",
                CreatedAt = DateTime.UtcNow.AddMinutes(-140)
            },
            new EscalatedCaseItem
            {
                Id = "REQ-2788",
                Category = "Medicine",
                Applicant = "S. De Silva - 1 person",
                Location = "West District",
                Priority = "Critical",
                AiPriority = "Critical",
                Confidence = "89% confidence",
                Verification = "Needs information",
                Assigned = "You",
                Age = "45 min",
                Status = "Pending Info",
                Event = "Flood Response",
                Minutes = 45,
                EscalationReason = "Specialized cardiac prescription medication required within 4 hours.",
                TargetOrg = "Ministry of Health Emergency Medical Squad",
                CoordinatorNotes = "Pharmacy courier dispatched.",
                CreatedAt = DateTime.UtcNow.AddMinutes(-45)
            }
        ]);
    }

    public Task<EscalationStatsDto> GetStatsAsync()
    {
        lock (_lock)
        {
            var stats = new EscalationStatsDto
            {
                TotalEscalated = _cases.Count,
                CriticalCount = _cases.Count(c => c.Priority == "Critical"),
                DueSoonCount = _cases.Count(c => c.Minutes <= 60),
                PendingInfoCount = _cases.Count(c => c.Verification == "Needs information" || c.Status == "Pending Info"),
                RecentlyUpdatedCount = _cases.Count(c => c.Status == "Under Review")
            };

            return Task.FromResult(stats);
        }
    }

    public Task<PaginatedResult<EscalatedCaseItem>> GetCasesAsync(EscalatedQueryDto query)
    {
        lock (_lock)
        {
            var filtered = _cases.AsEnumerable();

            // Tab Filter
            var tab = query.Tab?.Trim() ?? "All";
            if (tab.Equals("Critical", StringComparison.OrdinalIgnoreCase))
            {
                filtered = filtered.Where(c => c.Priority == "Critical");
            }
            else if (tab.Equals("Due soon", StringComparison.OrdinalIgnoreCase))
            {
                filtered = filtered.Where(c => c.Minutes <= 60);
            }
            else if (tab.Equals("Pending information", StringComparison.OrdinalIgnoreCase))
            {
                filtered = filtered.Where(c => c.Verification == "Needs information" || c.Status == "Pending Info");
            }
            else if (tab.Equals("Recently updated", StringComparison.OrdinalIgnoreCase))
            {
                filtered = filtered.Where(c => c.Status == "Under Review");
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
            if (!string.IsNullOrWhiteSpace(query.DateRange) && query.DateRange != "Date range" && query.DateRange != "All time")
            {
                if (query.DateRange.Contains("1 hour", StringComparison.OrdinalIgnoreCase))
                {
                    filtered = filtered.Where(c => c.Minutes <= 60);
                }
                else if (query.DateRange.Contains("24 hours", StringComparison.OrdinalIgnoreCase))
                {
                    filtered = filtered.Where(c => c.Minutes <= 1440);
                }
            }

            var totalCount = filtered.Count();
            var page = Math.Max(1, query.PageNumber);
            var pageSize = Math.Max(1, query.PageSize);
            var items = filtered
                .OrderByDescending(c => c.Priority == "Critical")
                .ThenBy(c => c.Minutes)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var result = new PaginatedResult<EscalatedCaseItem>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };

            return Task.FromResult(result);
        }
    }

    public Task<EscalatedCaseItem?> GetCaseByIdAsync(string id)
    {
        lock (_lock)
        {
            var item = _cases.FirstOrDefault(c => c.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(item);
        }
    }

    public Task<int> BulkActionAsync(BulkEscalationActionRequest request)
    {
        lock (_lock)
        {
            var count = 0;
            var action = request.Action.Trim().ToLowerInvariant();

            foreach (var id in request.CaseIds)
            {
                var item = _cases.FirstOrDefault(c => c.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
                if (item == null) continue;

                if (action == "escalate")
                {
                    item.Status = "Escalated";
                    item.Priority = "Critical";
                }
                else if (action == "de-escalate")
                {
                    item.Status = "Under Review";
                    if (item.Priority == "Critical") item.Priority = "High";
                }
                else if (action == "assigncoordinator")
                {
                    item.Assigned = string.IsNullOrWhiteSpace(request.AssignTo) ? "Senior Coordinator" : request.AssignTo;
                    item.Status = "Assigned";
                }
                else if (action == "resolve")
                {
                    item.Status = "Resolved";
                }

                if (!string.IsNullOrWhiteSpace(request.Notes))
                {
                    item.CoordinatorNotes = request.Notes;
                }

                count++;
            }

            return Task.FromResult(count);
        }
    }

    public Task<EscalatedCaseItem?> RecordDecisionAsync(string id, EscalationDecisionRequest request)
    {
        lock (_lock)
        {
            var item = _cases.FirstOrDefault(c => c.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (item == null) return Task.FromResult<EscalatedCaseItem?>(null);

            var action = request.Action.Trim().ToLowerInvariant();

            if (action == "resolve")
            {
                item.Status = "Resolved";
            }
            else if (action == "reassign")
            {
                item.Assigned = string.IsNullOrWhiteSpace(request.AssignTo) ? "You" : request.AssignTo;
                item.Status = "Assigned";
            }
            else if (action == "requestcrossorg")
            {
                item.Status = "Escalated";
                if (!string.IsNullOrWhiteSpace(request.TargetOrg))
                {
                    item.TargetOrg = request.TargetOrg;
                }
            }

            if (!string.IsNullOrWhiteSpace(request.Notes))
            {
                item.CoordinatorNotes = request.Notes;
            }

            return Task.FromResult<EscalatedCaseItem?>(item);
        }
    }

    public async Task<byte[]> ExportCsvAsync(EscalatedQueryDto query)
    {
        var result = await GetCasesAsync(new EscalatedQueryDto
        {
            Tab = query.Tab,
            Search = query.Search,
            Event = query.Event,
            Priority = query.Priority,
            Category = query.Category,
            Status = query.Status,
            DateRange = query.DateRange,
            PageNumber = 1,
            PageSize = 1000
        });

        var sb = new StringBuilder();
        sb.AppendLine("Case ID,Category,Applicant,Location,Reported Priority,AI Priority,Confidence,Verification,Assigned,Age,Status,Event,Escalation Reason,Target Org,Coordinator Notes");

        foreach (var c in result.Items)
        {
            sb.AppendLine($"\"{c.Id}\",\"{c.Category}\",\"{c.Applicant}\",\"{c.Location}\",\"{c.Priority}\",\"{c.AiPriority}\",\"{c.Confidence}\",\"{c.Verification}\",\"{c.Assigned}\",\"{c.Age}\",\"{c.Status}\",\"{c.Event}\",\"{c.EscalationReason}\",\"{c.TargetOrg}\",\"{c.CoordinatorNotes}\"");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }
}
