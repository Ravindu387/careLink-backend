using System.Text;
using CareLink.Backend.Models;

namespace CareLink.Backend.Services;

public class CaseManagementService : ICaseManagementService
{
    private readonly object _lock = new();
    private readonly List<CaseItem> _cases = [];
    private readonly List<ActivityItem> _activities = [];
    private int _nextId = 2842;

    public CaseManagementService()
    {
        SeedInitialData();
    }

    private void SeedInitialData()
    {
        _cases.AddRange([
            new CaseItem
            {
                Id = "ECO-2841",
                Category = "Emergency shelter",
                Applicant = "M. Fernando - 3 people",
                Location = "Riverview, North Ward",
                Priority = "Critical",
                AiPriority = "Critical",
                Confidence = "94% confidence",
                Verification = "Evidence pending",
                Assigned = "You",
                Age = "18 min",
                Status = "Under Review",
                Description = "Displaced due to rising flood waters in Riverview.",
                CreatedAt = DateTime.UtcNow.AddMinutes(-18)
            },
            new CaseItem
            {
                Id = "ECO-2829",
                Category = "Medical supplies",
                Applicant = "D. Silva - 2 people",
                Location = "Old Town, Sector 4",
                Priority = "High",
                AiPriority = "High",
                Confidence = "87% confidence",
                Verification = "Verified",
                Assigned = "You",
                Age = "34 min",
                Status = "Assigned",
                Description = "Insulin and clean dressing supplies urgently required.",
                CreatedAt = DateTime.UtcNow.AddMinutes(-34)
            },
            new CaseItem
            {
                Id = "ECO-2836",
                Category = "Food & water",
                Applicant = "K. Mendis - 5 people",
                Location = "Eastbank, Sector 2",
                Priority = "High",
                AiPriority = "High",
                Confidence = "82% confidence",
                Verification = "Contact verified",
                Assigned = "Unassigned",
                Age = "49 min",
                Status = "Submitted",
                Description = "Drinking water and dry ration kit requested for family.",
                CreatedAt = DateTime.UtcNow.AddMinutes(-49)
            },
            new CaseItem
            {
                Id = "ECO-2818",
                Category = "Rescue assistance",
                Applicant = "T. Jayasinghe - 4 people",
                Location = "Marshland, Sector 1",
                Priority = "Critical",
                AiPriority = "Critical",
                Confidence = "91% confidence",
                Verification = "Evidence pending",
                Assigned = "Unassigned",
                Age = "1 hr",
                Status = "Submitted",
                Description = "Stranded on roof level; boat access required.",
                CreatedAt = DateTime.UtcNow.AddHours(-1)
            },
            new CaseItem
            {
                Id = "ECO-2812",
                Category = "Elderly care",
                Applicant = "S. Perera - 1 person",
                Location = "Highland Point, Sector 5",
                Priority = "High",
                AiPriority = "High",
                Confidence = "89% confidence",
                Verification = "Verified",
                Assigned = "You",
                Age = "2 hrs",
                Status = "In Progress",
                Description = "Elderly citizen requires transport to designated care shelter.",
                CreatedAt = DateTime.UtcNow.AddHours(-2)
            }
        ]);

        _activities.AddRange([
            new ActivityItem
            {
                Title = "Case escalated",
                Description = "ECO-2841 - Needs urgent assistance",
                Time = "4 min",
                IconKey = "ArrowUpToLine",
                Color = "blue",
                Timestamp = DateTime.UtcNow.AddMinutes(-4)
            },
            new ActivityItem
            {
                Title = "AI review completed",
                Description = "ECO-2836 - Critical approved",
                Time = "12 min",
                IconKey = "Activity",
                Color = "green",
                Timestamp = DateTime.UtcNow.AddMinutes(-12)
            },
            new ActivityItem
            {
                Title = "Citizen information received",
                Description = "ECO-2829 - Additional details",
                Time = "19 min",
                IconKey = "MessageSquare",
                Color = "blue",
                Timestamp = DateTime.UtcNow.AddMinutes(-19)
            },
            new ActivityItem
            {
                Title = "Duplicate pair detected",
                Description = "ECO-2818 - Similarity match",
                Time = "28 min",
                IconKey = "Copy",
                Color = "purple",
                Timestamp = DateTime.UtcNow.AddMinutes(-28)
            }
        ]);
    }

    public Task<CaseDashboardResponse> GetDashboardDataAsync()
    {
        lock (_lock)
        {
            var myActiveCount = _cases.Count(c => c.Assigned == "You" && c.Status != "Resolved");
            var unassignedCount = _cases.Count(c => c.Assigned == "Unassigned" && c.Status != "Resolved");
            var awaitingVerifCount = _cases.Count(c => c.Verification == "Evidence pending" && c.Status != "Resolved");
            var criticalCount = _cases.Count(c => c.Priority == "Critical" && c.Status != "Resolved");
            var escalatedCount = _cases.Count(c => c.Status == "Escalated");

            var stats = new List<StatCardItem>
            {
                new() { Title = "My active cases", Value = myActiveCount > 0 ? myActiveCount.ToString() : "28", Description = "4 new within 1 hour", IconKey = "BriefcaseBusiness", Color = "blue" },
                new() { Title = "Unassigned cases", Value = unassignedCount > 0 ? unassignedCount.ToString() : "63", Description = "6 flagged overdue", IconKey = "FileText", Color = "orange" },
                new() { Title = "Awaiting verification", Value = awaitingVerifCount > 0 ? awaitingVerifCount.ToString() : "14", Description = "Evidence to review", IconKey = "ShieldCheck", Color = "blue" },
                new() { Title = "Critical queue", Value = criticalCount > 0 ? criticalCount.ToString() : "12", Description = "3 over 1 hour", IconKey = "AlertTriangle", Color = "red" },
                new() { Title = "Potential duplicates", Value = "9", Description = "Human review required", IconKey = "Copy", Color = "purple" },
                new() { Title = "Escalated cases", Value = escalatedCount > 0 ? escalatedCount.ToString() : "7", Description = "2 awaiting coordinator", IconKey = "ArrowUpToLine", Color = "red" },
                new() { Title = "Resolved today", Value = "84", Description = "Median 2.1 hours", IconKey = "CheckCircle2", Color = "green" },
                new() { Title = "SLA performance", Value = "91.4%", Description = "+2.7% this week", IconKey = "TrendingUp", Color = "green" },
            };

            var statusOverview = new List<StatusBreakdownItem>
            {
                new() { Name = "Submitted", Value = 284 + _cases.Count(c => c.Status == "Submitted") - 2 },
                new() { Name = "Under Review", Value = 218 + _cases.Count(c => c.Status == "Under Review") - 1 },
                new() { Name = "Verified", Value = 194 },
                new() { Name = "Assigned", Value = 246 + _cases.Count(c => c.Status == "Assigned") - 1 },
                new() { Name = "In Progress", Value = 182 + _cases.Count(c => c.Status == "In Progress") - 1 },
                new() { Name = "Pending Info", Value = 96 },
                new() { Name = "Escalated", Value = 64 + escalatedCount }
            };

            var totalOpen = statusOverview.Sum(s => s.Value);

            var chartData = new List<ChartDataPoint>
            {
                new() { Day = "Mon", Received = 85, Resolved = 65 },
                new() { Day = "Tue", Received = 105, Resolved = 80 },
                new() { Day = "Wed", Received = 95, Resolved = 92 },
                new() { Day = "Thu", Received = 125, Resolved = 100 },
                new() { Day = "Fri", Received = 115, Resolved = 108 },
                new() { Day = "Sat", Received = 145, Resolved = 130 },
                new() { Day = "Today", Received = 100 + _cases.Count - 5, Resolved = 85 }
            };

            var priorityCases = _cases
                .OrderByDescending(c => c.Priority == "Critical")
                .ThenByDescending(c => c.CreatedAt)
                .Take(10)
                .ToList();

            var activities = _activities
                .OrderByDescending(a => a.Timestamp)
                .Take(10)
                .ToList();

            var response = new CaseDashboardResponse
            {
                Statistics = stats,
                StatusOverview = statusOverview,
                TotalOpenCases = totalOpen,
                ChartData = chartData,
                ResolutionRate = "+84.7%",
                PriorityCases = priorityCases,
                RecentActivities = activities,
                LastUpdated = DateTime.UtcNow
            };

            return Task.FromResult(response);
        }
    }

    public Task<List<CaseItem>> GetCasesAsync(string? search = null, string? status = null, string? priority = null, string? assigned = null)
    {
        lock (_lock)
        {
            var query = _cases.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLowerInvariant();
                query = query.Where(c =>
                    c.Id.ToLowerInvariant().Contains(s) ||
                    c.Category.ToLowerInvariant().Contains(s) ||
                    c.Applicant.ToLowerInvariant().Contains(s) ||
                    c.Location.ToLowerInvariant().Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                query = query.Where(c => c.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(priority) && priority != "All")
            {
                query = query.Where(c => c.Priority.Equals(priority, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(assigned) && assigned != "All")
            {
                query = query.Where(c => c.Assigned.Equals(assigned, StringComparison.OrdinalIgnoreCase));
            }

            return Task.FromResult(query.OrderByDescending(c => c.CreatedAt).ToList());
        }
    }

    public Task<CaseItem?> GetCaseByIdAsync(string id)
    {
        lock (_lock)
        {
            var item = _cases.FirstOrDefault(c => c.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(item);
        }
    }

    public Task<CaseItem> CreateCaseAsync(CreateCaseRequest request)
    {
        lock (_lock)
        {
            var id = $"ECO-{_nextId++}";
            var newCase = new CaseItem
            {
                Id = id,
                Category = request.Category,
                Applicant = $"{request.ApplicantName} - {request.PeopleCount} people",
                Location = request.Location,
                Priority = request.Priority,
                AiPriority = request.Priority,
                Confidence = "88% confidence",
                Verification = "Evidence pending",
                Assigned = "Unassigned",
                Age = "Just now",
                Status = "Submitted",
                Description = request.Description,
                CreatedAt = DateTime.UtcNow
            };

            _cases.Insert(0, newCase);

            _activities.Insert(0, new ActivityItem
            {
                Title = "New case created",
                Description = $"{id} - {newCase.Category} ({newCase.Applicant})",
                Time = "Just now",
                IconKey = "BriefcaseBusiness",
                Color = "blue",
                Timestamp = DateTime.UtcNow
            });

            return Task.FromResult(newCase);
        }
    }

    public void UpsertExternalCase(CaseItem item)
    {
        lock (_lock)
        {
            var existingIdx = _cases.FindIndex(c => c.Id.Equals(item.Id, StringComparison.OrdinalIgnoreCase));
            if (existingIdx >= 0)
            {
                _cases[existingIdx] = item;
            }
            else
            {
                _cases.Insert(0, item);
            }
        }
    }

    public Task<CaseItem?> ClaimCaseAsync(string id, string? assignedTo = null)
    {
        lock (_lock)
        {
            var item = _cases.FirstOrDefault(c => c.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (item == null) return Task.FromResult<CaseItem?>(null);

            item.Assigned = string.IsNullOrWhiteSpace(assignedTo) ? "You" : assignedTo;
            if (item.Status == "Submitted")
            {
                item.Status = "Assigned";
            }

            _activities.Insert(0, new ActivityItem
            {
                Title = "Case claimed",
                Description = $"{item.Id} assigned to {item.Assigned}",
                Time = "Just now",
                IconKey = "ClipboardCheck",
                Color = "green",
                Timestamp = DateTime.UtcNow
            });

            return Task.FromResult<CaseItem?>(item);
        }
    }

    public Task<CaseItem?> UpdateStatusAsync(string id, string status, string? notes = null)
    {
        lock (_lock)
        {
            var item = _cases.FirstOrDefault(c => c.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (item == null) return Task.FromResult<CaseItem?>(null);

            var oldStatus = item.Status;
            item.Status = status;

            _activities.Insert(0, new ActivityItem
            {
                Title = $"Status updated to {status}",
                Description = $"{item.Id} changed from {oldStatus} to {status}" + (!string.IsNullOrWhiteSpace(notes) ? $" ({notes})" : ""),
                Time = "Just now",
                IconKey = status == "Escalated" ? "ArrowUpToLine" : "Activity",
                Color = status == "Escalated" ? "red" : "blue",
                Timestamp = DateTime.UtcNow
            });

            return Task.FromResult<CaseItem?>(item);
        }
    }

    public Task<byte[]> ExportSummaryCsvAsync()
    {
        lock (_lock)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Case ID,Category,Applicant,Location,Priority,AI Priority,Verification,Assigned,Status,Created At");

            foreach (var c in _cases)
            {
                sb.AppendLine($"\"{c.Id}\",\"{c.Category}\",\"{c.Applicant}\",\"{c.Location}\",\"{c.Priority}\",\"{c.AiPriority}\",\"{c.Verification}\",\"{c.Assigned}\",\"{c.Status}\",\"{c.CreatedAt:yyyy-MM-dd HH:mm:ss}\"");
            }

            return Task.FromResult(Encoding.UTF8.GetBytes(sb.ToString()));
        }
    }
}
