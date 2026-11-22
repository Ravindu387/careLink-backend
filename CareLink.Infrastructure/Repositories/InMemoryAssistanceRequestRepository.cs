using System.Collections.Concurrent;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;

namespace CareLink.Infrastructure.Repositories;

public class InMemoryAssistanceRequestRepository : IAssistanceRequestRepository
{
    private readonly ConcurrentDictionary<string, AssistanceRequest> _requests = new();
    private const int BaselineTotal = 1276;
    private const int BaselineUnassigned = 60;
    private const int BaselineOverdue = 11;
    private const int BaselineDuplicates = 8;

    public InMemoryAssistanceRequestRepository()
    {
        SeedDefaultData();
    }

    public Task<IReadOnlyList<AssistanceRequest>> GetAllAsync(
        AssistanceRequestFilterParams? filter = null,
        CancellationToken cancellationToken = default)
    {
        var query = _requests.Values.AsEnumerable();

        if (filter != null)
        {
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var s = filter.Search.Trim();
                query = query.Where(r =>
                    r.Id.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    r.Category.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    r.Location.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    r.Assigned.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    r.Organisation.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    (r.Description != null && r.Description.Contains(s, StringComparison.OrdinalIgnoreCase)));
            }

            if (!string.IsNullOrWhiteSpace(filter.Tab) && !filter.Tab.Equals("All requests", StringComparison.OrdinalIgnoreCase))
            {
                var tab = filter.Tab.Trim();
                if (tab.Equals("Unassigned", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(r => r.Assigned.Equals("Unassigned", StringComparison.OrdinalIgnoreCase));
                }
                else if (tab.Equals("Overdue", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(r => r.IsOverdue || r.Status.Equals("Escalated", StringComparison.OrdinalIgnoreCase));
                }
                else if (tab.Equals("Potential duplicates", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(r => r.IsPotentialDuplicate || r.Id == "REQ-2836");
                }
            }

            if (!string.IsNullOrWhiteSpace(filter.CrisisEvent) && !filter.CrisisEvent.Equals("All crisis events", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(r => r.CrisisEvent.Equals(filter.CrisisEvent, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.Priority) && !filter.Priority.Equals("All priorities", StringComparison.OrdinalIgnoreCase))
            {
                var p = filter.Priority.Trim();
                query = query.Where(r => r.PriorityLevel.Equals(p, StringComparison.OrdinalIgnoreCase) ||
                                         r.PriorityDisplay.StartsWith(p, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.Category) && !filter.Category.Equals("All categories", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(r => r.Category.Equals(filter.Category, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.Status) && !filter.Status.Equals("All statuses", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(r => r.Status.Equals(filter.Status, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.Organisation) && !filter.Organisation.Equals("All organisations", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(r => r.Organisation.Equals(filter.Organisation, StringComparison.OrdinalIgnoreCase));
            }

            if (filter.VerifiedOnly == true)
            {
                query = query.Where(r => r.Verification.Equals("Verified", StringComparison.OrdinalIgnoreCase) ||
                                         r.Verification.Equals("Contact verified", StringComparison.OrdinalIgnoreCase));
            }

            if (filter.UnassignedOnly == true)
            {
                query = query.Where(r => r.Assigned.Equals("Unassigned", StringComparison.OrdinalIgnoreCase));
            }
        }

        var list = query
            .OrderByDescending(r => r.CreatedAt)
            .ToList();

        return Task.FromResult<IReadOnlyList<AssistanceRequest>>(list);
    }

    public Task<AssistanceRequest?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        _requests.TryGetValue(id, out var req);
        return Task.FromResult(req);
    }

    public Task<AssistanceRequest> AddAsync(AssistanceRequest request, CancellationToken cancellationToken = default)
    {
        _requests[request.Id] = request;
        return Task.FromResult(request);
    }

    public Task<AssistanceRequest?> UpdateAsync(AssistanceRequest request, CancellationToken cancellationToken = default)
    {
        if (!_requests.ContainsKey(request.Id))
        {
            return Task.FromResult<AssistanceRequest?>(null);
        }

        _requests[request.Id] = request;
        return Task.FromResult<AssistanceRequest?>(request);
    }

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var removed = _requests.TryRemove(id, out _);
        return Task.FromResult(removed);
    }

    public Task<AssistanceRequestMetrics> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        var items = _requests.Values.ToList();

        var liveUnassigned = items.Count(r => r.Assigned.Equals("Unassigned", StringComparison.OrdinalIgnoreCase));
        var liveOverdue = items.Count(r => r.IsOverdue || r.Status.Equals("Escalated", StringComparison.OrdinalIgnoreCase));
        var liveDuplicates = items.Count(r => r.IsPotentialDuplicate || r.Id == "REQ-2836");
        var liveUnderReview = items.Count(r => r.Status.Equals("Under Review", StringComparison.OrdinalIgnoreCase));
        var liveInProgress = items.Count(r => r.Status.Equals("In Progress", StringComparison.OrdinalIgnoreCase));
        var liveCompleted = items.Count(r => r.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase));
        var liveEscalated = items.Count(r => r.Status.Equals("Escalated", StringComparison.OrdinalIgnoreCase));

        var metrics = new AssistanceRequestMetrics
        {
            TotalRequests = BaselineTotal + items.Count,
            Unassigned = BaselineUnassigned + liveUnassigned,
            Overdue = BaselineOverdue + liveOverdue,
            PotentialDuplicates = BaselineDuplicates + liveDuplicates,
            UnderReview = liveUnderReview,
            InProgress = liveInProgress,
            Completed = liveCompleted,
            Escalated = liveEscalated
        };

        return Task.FromResult(metrics);
    }

    public Task ResetToDefaultAsync(CancellationToken cancellationToken = default)
    {
        _requests.Clear();
        SeedDefaultData();
        return Task.CompletedTask;
    }

    private void SeedDefaultData()
    {
        var baseTime = DateTimeOffset.UtcNow;

        var items = new List<AssistanceRequest>
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
                IsOverdue = false,
                IsPotentialDuplicate = false,
                CreatedAt = baseTime.AddMinutes(-18)
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
                CreatedAt = baseTime.AddMinutes(-34),
                AssignedAt = baseTime.AddMinutes(-20)
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
                CreatedAt = baseTime.AddMinutes(-49)
            },
            new()
            {
                Id = "REQ-2828",
                Category = "Accessible transport",
                Location = "Central Ward",
                CrisisEvent = "Monsoon Floods 2025",
                PriorityLevel = "High",
                Confidence = 82,
                PriorityDisplay = "High · 82%",
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
                CreatedAt = baseTime.AddHours(-1).AddMinutes(-12),
                AssignedAt = baseTime.AddMinutes(-45)
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
                HasVulnerableMembers = false,
                IsLifeThreatening = true,
                IsOverdue = true,
                IsPotentialDuplicate = false,
                CreatedAt = baseTime.AddHours(-1).AddMinutes(-34),
                AssignedAt = baseTime.AddHours(-1)
            },
            new()
            {
                Id = "REQ-2819",
                Category = "Medical supplies",
                Location = "West District",
                CrisisEvent = "Dengue Outbreak",
                PriorityLevel = "Medium",
                Confidence = 76,
                PriorityDisplay = "Medium · 76%",
                Verification = "Verified",
                Assigned = "Medical Team 2",
                Status = "Assigned",
                Organisation = "Health Services",
                Description = "IV fluid and antipyretic delivery for field triage tent.",
                AffectedPeopleCount = 8,
                HasVulnerableMembers = true,
                IsLifeThreatening = false,
                IsOverdue = false,
                IsPotentialDuplicate = false,
                CreatedAt = baseTime.AddHours(-2).AddMinutes(-10),
                AssignedAt = baseTime.AddHours(-1).AddMinutes(-30)
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
                Description = "Community center shelter ran out of drinking water bottles.",
                AffectedPeopleCount = 35,
                HasVulnerableMembers = true,
                IsLifeThreatening = false,
                IsOverdue = false,
                IsPotentialDuplicate = false,
                CreatedAt = baseTime.AddHours(-3).AddMinutes(-20)
            },
            new()
            {
                Id = "REQ-2808",
                Category = "Emergency shelter",
                Location = "Central Ward",
                CrisisEvent = "North Ridge Landslide",
                PriorityLevel = "Medium",
                Confidence = 72,
                PriorityDisplay = "Medium · 72%",
                Verification = "Verified",
                Assigned = "Shelter Team 1",
                Status = "Completed",
                Organisation = "National Disaster Authority",
                Description = "Displaced family checked into Community Hall Shelter Pod 3.",
                AffectedPeopleCount = 4,
                HasVulnerableMembers = false,
                IsLifeThreatening = false,
                IsOverdue = false,
                IsPotentialDuplicate = false,
                CreatedAt = baseTime.AddHours(-4).AddMinutes(-15),
                AssignedAt = baseTime.AddHours(-3).AddMinutes(-30)
            }
        };

        foreach (var item in items)
        {
            _requests[item.Id] = item;
        }
    }
}
