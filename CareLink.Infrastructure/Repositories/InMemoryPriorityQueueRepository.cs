using System.Collections.Concurrent;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;

namespace CareLink.Infrastructure.Repositories;

public class InMemoryPriorityQueueRepository : IPriorityQueueRepository
{
    private readonly ConcurrentDictionary<string, PriorityQueueItem> _items = new();
    private const int BaselineTotal = 1276;
    private const int BaselineUnassigned = 60;
    private const int BaselineOverdue = 9;
    private const int BaselineDuplicates = 8;

    private static readonly List<SlaPolicy> SlaPolicies = new()
    {
        new()
        {
            Level = "Critical",
            Time = "Immediate response",
            TargetMinutes = 15,
            Color = "bg-red-100 text-red-700",
            Description = "Life-threatening triage or critical structural/rescue emergency requiring urgent dispatch."
        },
        new()
        {
            Level = "High",
            Time = "Within 30 minutes",
            TargetMinutes = 30,
            Color = "bg-amber-100 text-amber-700",
            Description = "Urgent medical supplies, displaced families, or potable water crisis."
        },
        new()
        {
            Level = "Medium",
            Time = "Within 2 hours",
            TargetMinutes = 120,
            Color = "bg-sky-100 text-sky-700",
            Description = "Accessible transport, secondary shelter allocation, and non-emergency field kit delivery."
        },
        new()
        {
            Level = "Low",
            Time = "Within 24 hours",
            TargetMinutes = 1440,
            Color = "bg-slate-100 text-slate-700",
            Description = "Standard humanitarian coordination, welfare checks, and administrative records."
        }
    };

    public InMemoryPriorityQueueRepository()
    {
        SeedDefaultData();
    }

    public Task<IReadOnlyList<PriorityQueueItem>> GetAllAsync(
        PriorityQueueFilterParams? filter = null,
        CancellationToken cancellationToken = default)
    {
        var query = _items.Values.AsEnumerable();

        if (filter != null)
        {
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var s = filter.Search.Trim();
                query = query.Where(i =>
                    i.Id.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    i.Category.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    i.Location.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    i.Assigned.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    (i.Description != null && i.Description.Contains(s, StringComparison.OrdinalIgnoreCase)));
            }

            if (!string.IsNullOrWhiteSpace(filter.Tab) &&
                !filter.Tab.Equals("All requests", StringComparison.OrdinalIgnoreCase))
            {
                var tab = filter.Tab.Trim();
                if (tab.Equals("Unassigned", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(i => i.Assigned.Equals("Unassigned", StringComparison.OrdinalIgnoreCase));
                }
                else if (tab.Equals("Overdue", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(i => i.IsOverdue || i.Status.Equals("Escalated", StringComparison.OrdinalIgnoreCase));
                }
                else if (tab.Equals("Potential duplicates", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(i => i.IsPotentialDuplicate || i.Id == "REQ-2836");
                }
            }

            if (!string.IsNullOrWhiteSpace(filter.CrisisEvent) &&
                !filter.CrisisEvent.Equals("All crisis events", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(i => i.CrisisEvent.Equals(filter.CrisisEvent.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.Priority) &&
                !filter.Priority.Equals("All priorities", StringComparison.OrdinalIgnoreCase))
            {
                var p = filter.Priority.Trim();
                query = query.Where(i =>
                    i.PriorityLevel.Equals(p, StringComparison.OrdinalIgnoreCase) ||
                    i.PriorityDisplay.StartsWith(p, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.Category) &&
                !filter.Category.Equals("All categories", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(i => i.Category.Equals(filter.Category.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.Status) &&
                !filter.Status.Equals("All statuses", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(i => i.Status.Equals(filter.Status.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.Organisation) &&
                !filter.Organisation.Equals("All organisations", StringComparison.OrdinalIgnoreCase))
            {
                var org = filter.Organisation.Trim();
                if (org.Equals("Unassigned", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(i => i.Assigned.Equals("Unassigned", StringComparison.OrdinalIgnoreCase));
                }
                else if (org.Equals("Medical Teams", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(i => i.Assigned.Contains("Medical", StringComparison.OrdinalIgnoreCase));
                }
                else if (org.Equals("Rescue Teams", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(i => i.Assigned.Contains("Rescue", StringComparison.OrdinalIgnoreCase));
                }
                else
                {
                    query = query.Where(i => i.Assigned.Contains(org, StringComparison.OrdinalIgnoreCase) ||
                                             i.Organisation.Contains(org, StringComparison.OrdinalIgnoreCase));
                }
            }

            if (filter.OverdueOnly == true)
            {
                query = query.Where(i => i.IsOverdue || i.Status.Equals("Escalated", StringComparison.OrdinalIgnoreCase));
            }

            if (filter.UnassignedOnly == true)
            {
                query = query.Where(i => i.Assigned.Equals("Unassigned", StringComparison.OrdinalIgnoreCase));
            }
        }

        var list = query
            .OrderByDescending(i => i.PriorityLevel.Equals("Critical", StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(i => i.Confidence)
            .ThenByDescending(i => i.CreatedAt)
            .ToList();

        return Task.FromResult<IReadOnlyList<PriorityQueueItem>>(list);
    }

    public Task<PriorityQueueItem?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        _items.TryGetValue(id, out var item);
        return Task.FromResult(item);
    }

    public Task<PriorityQueueItem> AddAsync(PriorityQueueItem item, CancellationToken cancellationToken = default)
    {
        _items[item.Id] = item;
        return Task.FromResult(item);
    }

    public Task<PriorityQueueItem?> UpdateAsync(PriorityQueueItem item, CancellationToken cancellationToken = default)
    {
        if (!_items.ContainsKey(item.Id))
        {
            return Task.FromResult<PriorityQueueItem?>(null);
        }

        item.UpdatedAt = DateTimeOffset.UtcNow;
        _items[item.Id] = item;
        return Task.FromResult<PriorityQueueItem?>(item);
    }

    public Task<PriorityQueueItem?> AssignTeamAsync(string id, string assignedTeam, CancellationToken cancellationToken = default)
    {
        if (!_items.TryGetValue(id, out var item))
        {
            return Task.FromResult<PriorityQueueItem?>(null);
        }

        item.Assigned = assignedTeam;
        item.AssignedAt = DateTimeOffset.UtcNow;
        item.UpdatedAt = DateTimeOffset.UtcNow;
        if (item.Status == "Submitted" || item.Status == "Under Review")
        {
            item.Status = "Assigned";
        }
        _items[id] = item;
        return Task.FromResult<PriorityQueueItem?>(item);
    }

    public Task<PriorityQueueItem?> UpdateStatusAsync(string id, string status, CancellationToken cancellationToken = default)
    {
        if (!_items.TryGetValue(id, out var item))
        {
            return Task.FromResult<PriorityQueueItem?>(null);
        }

        item.Status = status;
        item.UpdatedAt = DateTimeOffset.UtcNow;
        if (status.Equals("Escalated", StringComparison.OrdinalIgnoreCase))
        {
            item.IsOverdue = true;
        }
        _items[id] = item;
        return Task.FromResult<PriorityQueueItem?>(item);
    }

    public Task<PriorityQueueItem?> UpdatePriorityAsync(
        string id,
        string priorityLevel,
        int confidence,
        CancellationToken cancellationToken = default)
    {
        if (!_items.TryGetValue(id, out var item))
        {
            return Task.FromResult<PriorityQueueItem?>(null);
        }

        item.PriorityLevel = priorityLevel;
        item.Confidence = confidence;
        item.PriorityDisplay = $"{priorityLevel} · {confidence}%";
        item.UpdatedAt = DateTimeOffset.UtcNow;
        _items[id] = item;
        return Task.FromResult<PriorityQueueItem?>(item);
    }

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var removed = _items.TryRemove(id, out _);
        return Task.FromResult(removed);
    }

    public Task<PriorityQueueMetrics> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        var items = _items.Values.ToList();

        var unassignedLive = items.Count(i => i.Assigned.Equals("Unassigned", StringComparison.OrdinalIgnoreCase));
        var overdueLive = items.Count(i => i.IsOverdue || i.Status.Equals("Escalated", StringComparison.OrdinalIgnoreCase));
        var duplicatesLive = items.Count(i => i.IsPotentialDuplicate || i.Id == "REQ-2836");

        var criticalLive = items.Count(i => i.PriorityLevel.Equals("Critical", StringComparison.OrdinalIgnoreCase));
        var highLive = items.Count(i => i.PriorityLevel.Equals("High", StringComparison.OrdinalIgnoreCase));
        var mediumLive = items.Count(i => i.PriorityLevel.Equals("Medium", StringComparison.OrdinalIgnoreCase));
        var lowLive = items.Count(i => i.PriorityLevel.Equals("Low", StringComparison.OrdinalIgnoreCase));

        var overdueTotal = BaselineOverdue + overdueLive;

        var metrics = new PriorityQueueMetrics
        {
            TotalCount = BaselineTotal + items.Count,
            UnassignedCount = BaselineUnassigned + unassignedLive,
            OverdueCount = overdueTotal,
            PotentialDuplicatesCount = BaselineDuplicates + duplicatesLive,
            CriticalOrOverdueRequiringAttentionCount = overdueTotal,
            CriticalCount = criticalLive,
            HighCount = highLive,
            MediumCount = mediumLive,
            LowCount = lowLive,
            LastUpdated = DateTimeOffset.UtcNow,
        };

        return Task.FromResult(metrics);
    }

    public Task<IReadOnlyList<SlaPolicy>> GetSlaPoliciesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<SlaPolicy>>(SlaPolicies);
    }

    public Task ResetToDefaultAsync(CancellationToken cancellationToken = default)
    {
        _items.Clear();
        SeedDefaultData();
        return Task.CompletedTask;
    }

    private void SeedDefaultData()
    {
        var now = DateTimeOffset.UtcNow;

        var list = new List<PriorityQueueItem>
        {
            new()
            {
                Id = "REQ-2841",
                Category = "Emergency shelter",
                Location = "Riverside, North Ward",
                CrisisEvent = "Monsoon Floods 2025",
                PriorityLevel = "Critical",
                Confidence = 94,
                PriorityDisplay = "Critical · 94%",
                Verification = "Evidence pending",
                Assigned = "Unassigned",
                Status = "Under Review",
                Organisation = "National Disaster Authority",
                Description = "Ground floor flooding, 4 adults and 2 children displaced. Requires urgent shelter allocation.",
                AffectedPeopleCount = 6,
                HasVulnerableMembers = true,
                IsLifeThreatening = true,
                IsOverdue = true,
                IsPotentialDuplicate = false,
                CreatedAt = now.AddMinutes(-18)
            },
            new()
            {
                Id = "REQ-2839",
                Category = "Medical supplies",
                Location = "Old Town, Sector 4",
                CrisisEvent = "Monsoon Floods 2025",
                PriorityLevel = "High",
                Confidence = 91,
                PriorityDisplay = "High · 91%",
                Verification = "Verified",
                Assigned = "Medical Team 3",
                Status = "Assigned",
                Organisation = "Health Services",
                Description = "Critical diabetic insulin depleted due to flooding. Cold storage packs required immediately.",
                AffectedPeopleCount = 2,
                HasVulnerableMembers = true,
                IsLifeThreatening = false,
                IsOverdue = false,
                IsPotentialDuplicate = false,
                CreatedAt = now.AddMinutes(-34),
                AssignedAt = now.AddMinutes(-20)
            },
            new()
            {
                Id = "REQ-2836",
                Category = "Food & water",
                Location = "Eastbank, Sector 2",
                CrisisEvent = "Monsoon Floods 2025",
                PriorityLevel = "High",
                Confidence = 87,
                PriorityDisplay = "High · 87%",
                Verification = "Contact verified",
                Assigned = "Unassigned",
                Status = "Submitted",
                Organisation = "Relief Organisation",
                Description = "Family of 5 stranded on upper level with contaminated water supply. Urgent potable water needed.",
                AffectedPeopleCount = 5,
                HasVulnerableMembers = false,
                IsLifeThreatening = false,
                IsOverdue = false,
                IsPotentialDuplicate = true,
                CreatedAt = now.AddMinutes(-49)
            },
            new()
            {
                Id = "REQ-2821",
                Category = "Rescue assistance",
                Location = "North Ward",
                CrisisEvent = "Monsoon Floods 2025",
                PriorityLevel = "Critical",
                Confidence = 96,
                PriorityDisplay = "Critical · 96%",
                Verification = "Verified",
                Assigned = "Rescue Unit 2",
                Status = "Escalated",
                Organisation = "Emergency Services",
                Description = "Rising fast water at canal embankment. 3 citizens trapped on roof.",
                AffectedPeopleCount = 3,
                HasVulnerableMembers = true,
                IsLifeThreatening = true,
                IsOverdue = true,
                IsPotentialDuplicate = false,
                CreatedAt = now.AddMinutes(-94),
                AssignedAt = now.AddMinutes(-60)
            },
            new()
            {
                Id = "REQ-2828",
                Category = "Accessible transport",
                Location = "Central Ward",
                CrisisEvent = "Monsoon Floods 2025",
                PriorityLevel = "Medium",
                Confidence = 82,
                PriorityDisplay = "Medium · 82%",
                Verification = "Verified",
                Assigned = "Mobility Team",
                Status = "In Progress",
                Organisation = "Transport Authority",
                Description = "Wheelchair user evacuation needed from ground floor flat. Specialized vehicle required.",
                AffectedPeopleCount = 1,
                HasVulnerableMembers = true,
                IsLifeThreatening = false,
                IsOverdue = false,
                IsPotentialDuplicate = false,
                CreatedAt = now.AddMinutes(-72),
                AssignedAt = now.AddMinutes(-40)
            },
            new()
            {
                Id = "REQ-2819",
                Category = "Medical supplies",
                Location = "West District",
                CrisisEvent = "Dengue Outbreak",
                PriorityLevel = "High",
                Confidence = 89,
                PriorityDisplay = "High · 89%",
                Verification = "Verified",
                Assigned = "Medical Team 2",
                Status = "Assigned",
                Organisation = "Health Services",
                Description = "Rapid diagnostic test kits and oral rehydration solution request for community center.",
                AffectedPeopleCount = 40,
                HasVulnerableMembers = true,
                IsLifeThreatening = false,
                IsOverdue = false,
                IsPotentialDuplicate = false,
                CreatedAt = now.AddMinutes(-130),
                AssignedAt = now.AddMinutes(-90)
            },
            new()
            {
                Id = "REQ-2815",
                Category = "Food & water",
                Location = "Eastbank",
                CrisisEvent = "Monsoon Floods 2025",
                PriorityLevel = "High",
                Confidence = 85,
                PriorityDisplay = "High · 85%",
                Verification = "Contact verified",
                Assigned = "Unassigned",
                Status = "Submitted",
                Organisation = "Relief Organisation",
                Description = "Community kitchen depleted of dry food rations for 120 flood evacuees.",
                AffectedPeopleCount = 120,
                HasVulnerableMembers = false,
                IsLifeThreatening = false,
                IsOverdue = true,
                IsPotentialDuplicate = false,
                CreatedAt = now.AddMinutes(-200)
            },
            new()
            {
                Id = "REQ-2808",
                Category = "Emergency shelter",
                Location = "Central Ward",
                CrisisEvent = "Monsoon Floods 2025",
                PriorityLevel = "Medium",
                Confidence = 72,
                PriorityDisplay = "Medium · 72%",
                Verification = "Verified",
                Assigned = "Shelter Team 1",
                Status = "Completed",
                Organisation = "Disaster Management Center",
                Description = "Bedding and hygiene kits requested for community school refuge point.",
                AffectedPeopleCount = 35,
                HasVulnerableMembers = false,
                IsLifeThreatening = false,
                IsOverdue = false,
                IsPotentialDuplicate = false,
                CreatedAt = now.AddMinutes(-255),
                AssignedAt = now.AddMinutes(-230)
            }
        };

        foreach (var item in list)
        {
            _items[item.Id] = item;
        }
    }
}
