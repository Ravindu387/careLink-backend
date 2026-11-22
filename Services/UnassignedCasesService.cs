using System.Text;
using CareLink.Backend.Models;

namespace CareLink.Backend.Services;

public class UnassignedCasesService : IUnassignedCasesService
{
    private readonly List<UnassignedCase> _cases = [];
    private readonly object _lock = new();

    public UnassignedCasesService()
    {
        InitializeDemonstrationData();
    }

    private void InitializeDemonstrationData()
    {
        _cases.AddRange([
            new UnassignedCase
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
                ContactNumber = "+94 77 412 8041",
                HouseholdSize = 3,
                Description = "Clean drinking water and emergency rations requested for family stranded on upper level.",
                CreatedAt = DateTime.UtcNow.AddMinutes(-49)
            },
            new UnassignedCase
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
                ContactNumber = "+94 71 890 2311",
                HouseholdSize = 1,
                Description = "Urgent insulin refill needed; pharmacy prescription slip image blurry and requires re-verification.",
                CreatedAt = DateTime.UtcNow.AddMinutes(-128)
            },
            new UnassignedCase
            {
                Id = "REQ-2848",
                Category = "Emergency shelter",
                Applicant = "K. Gunaratne - 5 people",
                Location = "Lower Riverside",
                Priority = "Critical",
                AiPriority = "Critical",
                Confidence = "95% confidence",
                Verification = "Pending document",
                Assigned = "Unassigned",
                Age = "25 min",
                Status = "Submitted",
                Event = "Flood Response",
                Minutes = 25,
                ContactNumber = "+94 70 334 5678",
                HouseholdSize = 5,
                Description = "Roof collapsed due to torrential flash rain. 2 elderly persons and 1 infant require shelter kit immediately.",
                CreatedAt = DateTime.UtcNow.AddMinutes(-25)
            },
            new UnassignedCase
            {
                Id = "REQ-2852",
                Category = "Rescue assistance",
                Applicant = "S. Fernando - 4 people",
                Location = "Old Canal Zone",
                Priority = "Critical",
                AiPriority = "Critical",
                Confidence = "98% confidence",
                Verification = "Contact verified",
                Assigned = "Unassigned",
                Age = "12 min",
                Status = "Submitted",
                Event = "Flood Response",
                Minutes = 12,
                ContactNumber = "+94 77 912 3450",
                HouseholdSize = 4,
                Description = "Surging water reaching first floor balcony. Boat extraction required for 4 individuals.",
                CreatedAt = DateTime.UtcNow.AddMinutes(-12)
            },
            new UnassignedCase
            {
                Id = "REQ-2860",
                Category = "Medicine",
                Applicant = "A. De Silva - 2 people",
                Location = "Highland Sector 4",
                Priority = "Medium",
                AiPriority = "Medium",
                Confidence = "82% confidence",
                Verification = "Verified",
                Assigned = "Unassigned",
                Age = "1h 45m",
                Status = "Under Review",
                Event = "Storm Response",
                Minutes = 105,
                ContactNumber = "+94 72 455 6789",
                HouseholdSize = 2,
                Description = "Chronic asthma inhalers and pediatric fever medication requested following power outages.",
                CreatedAt = DateTime.UtcNow.AddMinutes(-105)
            },
            new UnassignedCase
            {
                Id = "REQ-2865",
                Category = "Food & water",
                Applicant = "P. Wijesinghe - 6 people",
                Location = "Central Valley",
                Priority = "Medium",
                AiPriority = "Low",
                Confidence = "79% confidence",
                Verification = "Needs information",
                Assigned = "Unassigned",
                Age = "3h 10m",
                Status = "Pending Info",
                Event = "Storm Response",
                Minutes = 190,
                ContactNumber = "+94 75 889 0123",
                HouseholdSize = 6,
                Description = "Dry rations pack requested; secondary contact person missing from submission form.",
                CreatedAt = DateTime.UtcNow.AddMinutes(-190)
            },
            new UnassignedCase
            {
                Id = "REQ-2871",
                Category = "Financial Aid",
                Applicant = "T. Jayasuriya - 2 people",
                Location = "North Outskirts",
                Priority = "Low",
                AiPriority = "Low",
                Confidence = "89% confidence",
                Verification = "Contact verified",
                Assigned = "Unassigned",
                Age = "4h 30m",
                Status = "Submitted",
                Event = "Drought Relief",
                Minutes = 270,
                ContactNumber = "+94 78 223 9012",
                HouseholdSize = 2,
                Description = "Emergency subsistence voucher request due to seasonal agricultural water shortages.",
                CreatedAt = DateTime.UtcNow.AddMinutes(-270)
            },
            new UnassignedCase
            {
                Id = "REQ-2880",
                Category = "Emergency shelter",
                Applicant = "N. Bandara - 7 people",
                Location = "Eastbank, Sector 5",
                Priority = "Critical",
                AiPriority = "Critical",
                Confidence = "93% confidence",
                Verification = "Contact verified",
                Assigned = "Unassigned",
                Age = "35 min",
                Status = "Submitted",
                Event = "Urban Fire Response",
                Minutes = 35,
                ContactNumber = "+94 71 556 7812",
                HouseholdSize = 7,
                Description = "Displaced by perimeter residential fire. Direct relocation and emergency bedding required.",
                CreatedAt = DateTime.UtcNow.AddMinutes(-35)
            }
        ]);
    }

    public Task<UnassignedCasesStatsDto> GetStatsAsync()
    {
        lock (_lock)
        {
            var unassignedOnly = _cases.Where(c => c.Assigned == "Unassigned").ToList();

            var stats = new UnassignedCasesStatsDto
            {
                TotalCases = unassignedOnly.Count,
                CriticalCount = unassignedOnly.Count(c => c.Priority == "Critical"),
                DueSoonCount = unassignedOnly.Count(c => c.Priority == "Critical" || c.Minutes <= 60),
                PendingInfoCount = unassignedOnly.Count(c => c.Verification == "Needs information" || c.Status == "Pending Info"),
                RecentlyUpdatedCount = unassignedOnly.Count(c => c.Status == "Submitted")
            };

            return Task.FromResult(stats);
        }
    }

    public Task<PaginatedResult<UnassignedCase>> GetCasesAsync(UnassignedCasesQueryDto query)
    {
        lock (_lock)
        {
            var items = _cases.AsEnumerable();

            // Default behavior: show Unassigned cases unless assigned filter is specifically requested
            items = items.Where(c => c.Assigned == "Unassigned");

            // Tab filter
            if (!string.IsNullOrWhiteSpace(query.Tab) && !query.Tab.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                var tab = query.Tab.Trim().ToLowerInvariant();
                items = tab switch
                {
                    "critical" => items.Where(c => c.Priority.Equals("Critical", StringComparison.OrdinalIgnoreCase)),
                    "due soon" => items.Where(c => c.Priority.Equals("Critical", StringComparison.OrdinalIgnoreCase) || c.Priority.Equals("High", StringComparison.OrdinalIgnoreCase) || c.Minutes <= 60),
                    "pending information" => items.Where(c => c.Verification.Equals("Needs information", StringComparison.OrdinalIgnoreCase) || c.Status.Equals("Pending Info", StringComparison.OrdinalIgnoreCase)),
                    "recently updated" => items.Where(c => c.Status.Equals("Submitted", StringComparison.OrdinalIgnoreCase)),
                    _ => items
                };
            }

            // Search query
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var s = query.Search.Trim().ToLowerInvariant();
                items = items.Where(c =>
                    c.Id.ToLowerInvariant().Contains(s) ||
                    c.Applicant.ToLowerInvariant().Contains(s) ||
                    c.Location.ToLowerInvariant().Contains(s) ||
                    c.Category.ToLowerInvariant().Contains(s) ||
                    c.Event.ToLowerInvariant().Contains(s));
            }

            // Priority filter
            if (!string.IsNullOrWhiteSpace(query.Priority) && !query.Priority.Equals("All priorities", StringComparison.OrdinalIgnoreCase))
            {
                items = items.Where(c => c.Priority.Equals(query.Priority, StringComparison.OrdinalIgnoreCase));
            }

            // Category filter
            if (!string.IsNullOrWhiteSpace(query.Category) && !query.Category.Equals("All categories", StringComparison.OrdinalIgnoreCase))
            {
                items = items.Where(c => c.Category.Equals(query.Category, StringComparison.OrdinalIgnoreCase));
            }

            // Status filter
            if (!string.IsNullOrWhiteSpace(query.Status) && !query.Status.Equals("All statuses", StringComparison.OrdinalIgnoreCase))
            {
                items = items.Where(c => c.Status.Equals(query.Status, StringComparison.OrdinalIgnoreCase));
            }

            // Event filter
            if (!string.IsNullOrWhiteSpace(query.Event) && !query.Event.Equals("All crisis events", StringComparison.OrdinalIgnoreCase))
            {
                items = items.Where(c => c.Event.Equals(query.Event, StringComparison.OrdinalIgnoreCase));
            }

            var list = items.OrderBy(c => c.Priority == "Critical" ? 0 : c.Priority == "High" ? 1 : 2)
                            .ThenBy(c => c.Minutes)
                            .ToList();

            var totalCount = list.Count;
            var page = Math.Max(1, query.PageNumber);
            var size = query.PageSize > 0 ? query.PageSize : 10;
            var pagedItems = list.Skip((page - 1) * size).Take(size).ToList();

            var result = new PaginatedResult<UnassignedCase>
            {
                Items = pagedItems,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = size
            };

            return Task.FromResult(result);
        }
    }

    public Task<UnassignedCase?> GetByIdAsync(string id)
    {
        lock (_lock)
        {
            var item = _cases.FirstOrDefault(c => c.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(item);
        }
    }

    public Task<int> ClaimCasesAsync(ClaimUnassignedCasesRequest request)
    {
        lock (_lock)
        {
            var claimed = 0;
            var assignTo = string.IsNullOrWhiteSpace(request.AssignTo) ? "You" : request.AssignTo;

            foreach (var id in request.CaseIds)
            {
                var target = _cases.FirstOrDefault(c => c.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
                if (target != null)
                {
                    target.Assigned = assignTo;
                    target.Status = "Assigned";
                    claimed++;
                }
            }

            return Task.FromResult(claimed);
        }
    }

    public Task<UnassignedCase?> AssignCaseAsync(AssignUnassignedCaseRequest request)
    {
        lock (_lock)
        {
            var target = _cases.FirstOrDefault(c => c.Id.Equals(request.CaseId, StringComparison.OrdinalIgnoreCase));
            if (target != null)
            {
                target.Assigned = string.IsNullOrWhiteSpace(request.AssignedTo) ? "You" : request.AssignedTo;
                target.Status = "Assigned";
                if (!string.IsNullOrWhiteSpace(request.Notes))
                {
                    target.Notes = request.Notes;
                }
            }

            return Task.FromResult(target);
        }
    }

    public Task<UnassignedCase?> UpdateCaseAsync(string id, UpdateUnassignedCaseRequest request)
    {
        lock (_lock)
        {
            var target = _cases.FirstOrDefault(c => c.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (target != null)
            {
                if (!string.IsNullOrWhiteSpace(request.Priority)) target.Priority = request.Priority;
                if (!string.IsNullOrWhiteSpace(request.Status)) target.Status = request.Status;
                if (!string.IsNullOrWhiteSpace(request.Assigned)) target.Assigned = request.Assigned;
                if (!string.IsNullOrWhiteSpace(request.Notes)) target.Notes = request.Notes;
            }

            return Task.FromResult(target);
        }
    }

    public Task<byte[]> ExportCsvAsync(UnassignedCasesQueryDto query)
    {
        lock (_lock)
        {
            var paged = GetCasesAsync(new UnassignedCasesQueryDto
            {
                Tab = query.Tab,
                Search = query.Search,
                Event = query.Event,
                Priority = query.Priority,
                Category = query.Category,
                Status = query.Status,
                DateRange = query.DateRange,
                PageNumber = 1,
                PageSize = 10000
            }).Result;

            var sb = new StringBuilder();
            sb.AppendLine("Case ID,Category,Applicant,Location,Priority,AI Priority,Verification,Assigned,Age,Status,Event,Contact Number,Household Size");

            foreach (var c in paged.Items)
            {
                sb.AppendLine($"\"{EscapeCsv(c.Id)}\",\"{EscapeCsv(c.Category)}\",\"{EscapeCsv(c.Applicant)}\",\"{EscapeCsv(c.Location)}\",\"{EscapeCsv(c.Priority)}\",\"{EscapeCsv(c.AiPriority)}\",\"{EscapeCsv(c.Verification)}\",\"{EscapeCsv(c.Assigned)}\",\"{EscapeCsv(c.Age)}\",\"{EscapeCsv(c.Status)}\",\"{EscapeCsv(c.Event)}\",\"{EscapeCsv(c.ContactNumber)}\",{c.HouseholdSize}");
            }

            return Task.FromResult(Encoding.UTF8.GetBytes(sb.ToString()));
        }
    }

    private static string EscapeCsv(string? value)
    {
        return (value ?? string.Empty).Replace("\"", "\"\"");
    }
}
