using System.Collections.Concurrent;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;

namespace CareLink.Infrastructure.Repositories;

public class InMemoryAssistanceReferralRepository : IAssistanceReferralRepository
{
    private readonly ConcurrentDictionary<string, AssistanceReferral> _referrals = new();

    public InMemoryAssistanceReferralRepository()
    {
        SeedDefaultData();
    }

    public Task<IReadOnlyList<AssistanceReferral>> GetAllAsync(
        AssistanceReferralFilterParams? filter = null,
        CancellationToken cancellationToken = default)
    {
        var query = _referrals.Values.AsEnumerable();

        if (filter != null)
        {
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var s = filter.Search.Trim();
                query = query.Where(r =>
                    r.Id.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    r.Assistance.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    r.Location.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    r.Assigned.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    r.Crisis.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    r.Sector.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    (r.Description != null && r.Description.Contains(s, StringComparison.OrdinalIgnoreCase)));
            }

            if (!string.IsNullOrWhiteSpace(filter.Tab) &&
                !filter.Tab.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                var tab = filter.Tab.Trim();
                if (tab.Equals("Needs action", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(r => r.Status.Equals("Awaiting Acceptance", StringComparison.OrdinalIgnoreCase));
                }
                else if (tab.Equals("Active", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(r =>
                        r.Status.Equals("Accepted", StringComparison.OrdinalIgnoreCase) ||
                        r.Status.Equals("In Progress", StringComparison.OrdinalIgnoreCase));
                }
                else if (tab.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(r => r.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase));
                }
                else if (tab.Equals("Archived", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(r => r.Status.Equals("Archived", StringComparison.OrdinalIgnoreCase));
                }
            }

            if (!string.IsNullOrWhiteSpace(filter.Crisis) &&
                !filter.Crisis.Equals("All crisis events", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(r => r.Crisis.Equals(filter.Crisis.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.Priority) &&
                !filter.Priority.Equals("All priorities", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(r => r.Priority.Equals(filter.Priority.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.Sector) &&
                !filter.Sector.Equals("All sectors", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(r => r.Sector.Equals(filter.Sector.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.Status) &&
                !filter.Status.Equals("All statuses", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(r => r.Status.Equals(filter.Status.Trim(), StringComparison.OrdinalIgnoreCase));
            }
        }

        var list = query
            .OrderByDescending(r => r.CreatedAt)
            .ToList();

        return Task.FromResult<IReadOnlyList<AssistanceReferral>>(list);
    }

    public Task<AssistanceReferral?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        _referrals.TryGetValue(id, out var item);
        return Task.FromResult(item);
    }

    public Task<AssistanceReferral> AddAsync(
        AssistanceReferral referral,
        CancellationToken cancellationToken = default)
    {
        _referrals[referral.Id] = referral;
        return Task.FromResult(referral);
    }

    public Task<AssistanceReferral?> UpdateAsync(
        AssistanceReferral referral,
        CancellationToken cancellationToken = default)
    {
        if (!_referrals.ContainsKey(referral.Id))
        {
            return Task.FromResult<AssistanceReferral?>(null);
        }

        referral.UpdatedAt = DateTimeOffset.UtcNow;
        _referrals[referral.Id] = referral;
        return Task.FromResult<AssistanceReferral?>(referral);
    }

    public Task<AssistanceReferral?> UpdateStatusAsync(
        string id,
        string status,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        if (!_referrals.TryGetValue(id, out var item))
        {
            return Task.FromResult<AssistanceReferral?>(null);
        }

        item.Status = status.Trim();
        item.UpdatedAt = DateTimeOffset.UtcNow;

        if (status.Equals("Accepted", StringComparison.OrdinalIgnoreCase))
        {
            item.AcceptedAt = DateTimeOffset.UtcNow;
            if (item.Assigned.Equals("Unassigned", StringComparison.OrdinalIgnoreCase))
            {
                item.Assigned = "Case Support Team";
            }
        }
        else if (status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
        {
            item.CompletedAt = DateTimeOffset.UtcNow;
        }

        if (!string.IsNullOrWhiteSpace(reason))
        {
            item.Description = string.IsNullOrWhiteSpace(item.Description)
                ? $"Status update: {reason}"
                : $"{item.Description} | Status update: {reason}";
        }

        _referrals[id] = item;
        return Task.FromResult<AssistanceReferral?>(item);
    }

    public Task<AssistanceReferral?> AssignTeamAsync(
        string id,
        string assignedTeam,
        CancellationToken cancellationToken = default)
    {
        if (!_referrals.TryGetValue(id, out var item))
        {
            return Task.FromResult<AssistanceReferral?>(null);
        }

        item.Assigned = assignedTeam.Trim();
        item.UpdatedAt = DateTimeOffset.UtcNow;
        _referrals[id] = item;
        return Task.FromResult<AssistanceReferral?>(item);
    }

    public Task<bool> DeleteAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_referrals.TryRemove(id, out _));
    }

    public Task<AssistanceReferralMetrics> GetMetricsAsync(
        CancellationToken cancellationToken = default)
    {
        var all = _referrals.Values.ToList();

        var openReferrals = all.Count(r =>
            !r.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase) &&
            !r.Status.Equals("Archived", StringComparison.OrdinalIgnoreCase));

        var awaitingAction = all.Count(r =>
            r.Status.Equals("Awaiting Acceptance", StringComparison.OrdinalIgnoreCase));

        var criticalPriority = all.Count(r =>
            r.Priority.Equals("Critical", StringComparison.OrdinalIgnoreCase));

        var acceptedToday = all.Count(r =>
            r.Status.Equals("Accepted", StringComparison.OrdinalIgnoreCase) ||
            r.Status.Equals("In Progress", StringComparison.OrdinalIgnoreCase) ||
            r.AcceptedAt.HasValue);

        var peopleSupported = all.Sum(r => r.Affected);

        var metrics = new AssistanceReferralMetrics
        {
            OpenReferrals = openReferrals,
            AwaitingAction = awaitingAction,
            CriticalPriority = criticalPriority,
            AcceptedToday = acceptedToday,
            PeopleSupported = peopleSupported,
            AllCount = all.Count,
            NeedsActionCount = awaitingAction,
            ActiveCount = all.Count(r =>
                r.Status.Equals("Accepted", StringComparison.OrdinalIgnoreCase) ||
                r.Status.Equals("In Progress", StringComparison.OrdinalIgnoreCase)),
            CompletedCount = all.Count(r =>
                r.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase)),
            ArchivedCount = all.Count(r =>
                r.Status.Equals("Archived", StringComparison.OrdinalIgnoreCase)),
            LastUpdated = DateTimeOffset.UtcNow
        };

        return Task.FromResult(metrics);
    }

    public Task ResetToDefaultAsync(CancellationToken cancellationToken = default)
    {
        _referrals.Clear();
        SeedDefaultData();
        return Task.CompletedTask;
    }

    private void SeedDefaultData()
    {
        var seedList = new List<AssistanceReferral>
        {
            new()
            {
                Id = "REF-2841",
                Assistance = "Emergency shelter",
                Location = "Riverside - North Ward",
                Affected = 6,
                Priority = "Critical",
                Received = "22 Jun - 13:02",
                Assigned = "Case Support Team",
                Status = "Awaiting Acceptance",
                Crisis = "Monsoon Floods 2025",
                Sector = "Shelter",
                Description = "Displaced family needing immediate emergency shelter due to rapid river swelling.",
                Organisation = "CareLink Partner Office",
                ContactName = "Sunil Perera",
                ContactPhone = "+94 77 123 4567",
                CreatedAt = DateTimeOffset.UtcNow.AddHours(-2)
            },
            new()
            {
                Id = "REF-2838",
                Assistance = "Food assistance",
                Location = "Central District",
                Affected = 12,
                Priority = "High",
                Received = "22 Jun - 11:45",
                Assigned = "Food Distribution Team",
                Status = "Accepted",
                Crisis = "Monsoon Floods 2025",
                Sector = "Food",
                Description = "Essential dry rations and canned goods for displaced families at community center.",
                Organisation = "Red Cross Society",
                ContactName = "Anoma Jayasinghe",
                ContactPhone = "+94 71 987 6543",
                CreatedAt = DateTimeOffset.UtcNow.AddHours(-4),
                AcceptedAt = DateTimeOffset.UtcNow.AddHours(-3)
            },
            new()
            {
                Id = "REF-2835",
                Assistance = "Medical supplies",
                Location = "Eastbank",
                Affected = 8,
                Priority = "Critical",
                Received = "21 Jun - 16:20",
                Assigned = "Medical Response Team",
                Status = "In Progress",
                Crisis = "Dengue Outbreak",
                Sector = "Health",
                Description = "Urgent diagnostic rapid test kits, paracetamol and IV saline drip supplies.",
                Organisation = "CareLink Partner Office",
                ContactName = "Dr. K. Ramanathan",
                ContactPhone = "+94 75 444 3322",
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-1),
                AcceptedAt = DateTimeOffset.UtcNow.AddDays(-1).AddHours(1)
            },
            new()
            {
                Id = "REF-2830",
                Assistance = "Clean drinking water",
                Location = "North District",
                Affected = 20,
                Priority = "High",
                Received = "21 Jun - 10:15",
                Assigned = "WASH Team",
                Status = "Accepted",
                Crisis = "Monsoon Floods 2025",
                Sector = "WASH",
                Description = "Drinking water bowsers and 20L jerrycans for contaminated well zone.",
                Organisation = "Oxfam Sri Lanka",
                ContactName = "Nimal Bandara",
                ContactPhone = "+94 72 333 8899",
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-1).AddHours(-4),
                AcceptedAt = DateTimeOffset.UtcNow.AddDays(-1).AddHours(-2)
            },
            new()
            {
                Id = "REF-2825",
                Assistance = "Temporary housing",
                Location = "West Coast",
                Affected = 5,
                Priority = "Medium",
                Received = "20 Jun - 09:30",
                Assigned = "Shelter Team",
                Status = "Completed",
                Crisis = "Coastal Storm Response",
                Sector = "Shelter",
                Description = "All-weather tarpaulins and timber reinforcement kits provided to fishing families.",
                Organisation = "CareLink Partner Office",
                ContactName = "Gamini Silva",
                ContactPhone = "+94 76 555 1212",
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-2),
                AcceptedAt = DateTimeOffset.UtcNow.AddDays(-2).AddHours(2),
                CompletedAt = DateTimeOffset.UtcNow.AddDays(-1)
            },
            new()
            {
                Id = "REF-2820",
                Assistance = "Emergency food packs",
                Location = "South District",
                Affected = 15,
                Priority = "Low",
                Received = "19 Jun - 14:10",
                Assigned = "Food Distribution Team",
                Status = "Archived",
                Crisis = "Monsoon Floods 2025",
                Sector = "Food",
                Description = "Standard dry food packs delivered. Follow-up verification finished.",
                Organisation = "World Vision",
                ContactName = "Dilani Fernando",
                ContactPhone = "+94 77 999 0011",
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-3),
                AcceptedAt = DateTimeOffset.UtcNow.AddDays(-3).AddHours(1),
                CompletedAt = DateTimeOffset.UtcNow.AddDays(-2)
            },
            new()
            {
                Id = "REF-2815",
                Assistance = "Mobile clinic deployment",
                Location = "Highland Ward",
                Affected = 25,
                Priority = "High",
                Received = "22 Jun - 08:30",
                Assigned = "Unassigned",
                Status = "Awaiting Acceptance",
                Crisis = "Monsoon Floods 2025",
                Sector = "Health",
                Description = "Secondary bridge blocked; mobile medical van required for senior residents and children.",
                Organisation = "CareLink Partner Office",
                ContactName = "Sister Mary",
                ContactPhone = "+94 71 222 3456",
                CreatedAt = DateTimeOffset.UtcNow.AddHours(-6)
            },
            new()
            {
                Id = "REF-2810",
                Assistance = "Sanitation & hygiene kits",
                Location = "Riverside Camp",
                Affected = 35,
                Priority = "Medium",
                Received = "21 Jun - 14:00",
                Assigned = "WASH Team",
                Status = "In Progress",
                Crisis = "Monsoon Floods 2025",
                Sector = "WASH",
                Description = "Hygiene packs including soaps, sanitary products, and bleach for evacuation point.",
                Organisation = "UNICEF Partner",
                ContactName = "Priyantha Kumara",
                ContactPhone = "+94 78 111 2233",
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-1).AddHours(2),
                AcceptedAt = DateTimeOffset.UtcNow.AddDays(-1).AddHours(4)
            }
        };

        foreach (var item in seedList)
        {
            _referrals[item.Id] = item;
        }
    }
}
