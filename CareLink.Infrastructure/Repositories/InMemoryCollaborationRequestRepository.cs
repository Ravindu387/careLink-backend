using System.Collections.Concurrent;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;

namespace CareLink.Infrastructure.Repositories;

public class InMemoryCollaborationRequestRepository : ICollaborationRequestRepository
{
    private readonly ConcurrentDictionary<string, CollaborationRequest> _items = new();

    public InMemoryCollaborationRequestRepository()
    {
        SeedDefaultData();
    }

    public Task<IReadOnlyList<CollaborationRequest>> GetAllAsync(
        CollaborationFilterParams? filter = null,
        CancellationToken cancellationToken = default)
    {
        var query = _items.Values.AsEnumerable();

        if (filter != null)
        {
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var s = filter.Search.Trim();
                query = query.Where(item =>
                    item.Id.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    item.Title.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    item.Partner.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    item.Sector.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    item.Area.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    item.Support.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    (item.Description != null && item.Description.Contains(s, StringComparison.OrdinalIgnoreCase)));
            }

            if (!string.IsNullOrWhiteSpace(filter.Tab) &&
                !filter.Tab.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                var tab = filter.Tab.Trim();
                if (tab.Equals("Needs action", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(item => item.Status.Equals("Awaiting Response", StringComparison.OrdinalIgnoreCase));
                }
                else if (tab.Equals("Active", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(item =>
                        item.Status.Equals("Accepted", StringComparison.OrdinalIgnoreCase) ||
                        item.Status.Equals("In Progress", StringComparison.OrdinalIgnoreCase));
                }
                else if (tab.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(item => item.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase));
                }
                else if (tab.Equals("Archived", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(item => item.Status.Equals("Archived", StringComparison.OrdinalIgnoreCase));
                }
            }

            if (!string.IsNullOrWhiteSpace(filter.Sector) &&
                !filter.Sector.Equals("All sectors", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(item => item.Sector.Equals(filter.Sector.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.Partner) &&
                !filter.Partner.Equals("All partners", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(item => item.Partner.Equals(filter.Partner.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.Area) &&
                !filter.Area.Equals("All operational areas", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(item => item.Area.Equals(filter.Area.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.Status) &&
                !filter.Status.Equals("All statuses", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(item => item.Status.Equals(filter.Status.Trim(), StringComparison.OrdinalIgnoreCase));
            }
        }

        var list = query
            .OrderByDescending(item => item.CreatedAt)
            .ToList();

        return Task.FromResult<IReadOnlyList<CollaborationRequest>>(list);
    }

    public Task<CollaborationRequest?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        _items.TryGetValue(id, out var item);
        return Task.FromResult(item);
    }

    public Task<CollaborationRequest> AddAsync(CollaborationRequest collaboration, CancellationToken cancellationToken = default)
    {
        _items[collaboration.Id] = collaboration;
        return Task.FromResult(collaboration);
    }

    public Task<CollaborationRequest?> UpdateAsync(CollaborationRequest collaboration, CancellationToken cancellationToken = default)
    {
        if (!_items.ContainsKey(collaboration.Id))
        {
            return Task.FromResult<CollaborationRequest?>(null);
        }

        collaboration.UpdatedAt = DateTimeOffset.UtcNow;
        _items[collaboration.Id] = collaboration;
        return Task.FromResult<CollaborationRequest?>(collaboration);
    }

    public Task<CollaborationRequest?> UpdateStatusAsync(string id, string status, CancellationToken cancellationToken = default)
    {
        if (!_items.TryGetValue(id, out var item))
        {
            return Task.FromResult<CollaborationRequest?>(null);
        }

        item.Status = status.Trim();
        item.UpdatedAt = DateTimeOffset.UtcNow;

        if (status.Equals("Accepted", StringComparison.OrdinalIgnoreCase))
        {
            item.AcceptedAt = DateTimeOffset.UtcNow;
        }
        else if (status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
        {
            item.CompletedAt = DateTimeOffset.UtcNow;
        }

        _items[id] = item;
        return Task.FromResult<CollaborationRequest?>(item);
    }

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_items.TryRemove(id, out _));
    }

    public Task<CollaborationRequestMetrics> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        var all = _items.Values.ToList();

        var openCollabs = all.Count(i =>
            !i.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase) &&
            !i.Status.Equals("Archived", StringComparison.OrdinalIgnoreCase));

        var awaiting = all.Count(i =>
            i.Status.Equals("Awaiting Response", StringComparison.OrdinalIgnoreCase));

        var active = all.Count(i =>
            i.Status.Equals("Accepted", StringComparison.OrdinalIgnoreCase) ||
            i.Status.Equals("In Progress", StringComparison.OrdinalIgnoreCase));

        var completed = all.Count(i =>
            i.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase));

        var sectors = all.Select(i => i.Sector).Distinct(StringComparer.OrdinalIgnoreCase).Count();
        var partners = all.Select(i => i.Partner).Distinct(StringComparer.OrdinalIgnoreCase).Count();

        var metrics = new CollaborationRequestMetrics
        {
            OpenCollaborations = openCollabs,
            AwaitingResponse = awaiting,
            ActivePartnerships = active,
            CompletedThisMonth = 28 + completed,
            TotalCollaborations = all.Count,
            SectorsCount = Math.Max(sectors, 5),
            PartnersCount = Math.Max(partners, 9),
            NeedsActionCount = awaiting,
            ActiveCount = active,
            CompletedCount = completed,
            ArchivedCount = all.Count(i => i.Status.Equals("Archived", StringComparison.OrdinalIgnoreCase)),
            LastUpdated = DateTimeOffset.UtcNow
        };

        return Task.FromResult(metrics);
    }

    public Task ResetToDefaultAsync(CancellationToken cancellationToken = default)
    {
        _items.Clear();
        SeedDefaultData();
        return Task.CompletedTask;
    }

    private void SeedDefaultData()
    {
        var baseDate = DateTimeOffset.UtcNow;

        var seeds = new List<CollaborationRequest>
        {
            new()
            {
                Id = "COL-0742",
                Title = "Mobile water distribution",
                Partner = "National Disaster Authority",
                Sector = "WASH",
                Area = "North Ward",
                Support = "Vehicles · staff",
                StartDate = "24 Jun",
                Status = "Awaiting Response",
                Description = "Coordination of mobile clean drinking water supply bowsers and distribution crews for high-ground refugee centers.",
                ContactName = "Col. Ravi Jayawardena",
                ContactEmail = "r.jaya@disaster.gov",
                ContactPhone = "+94 11 234 5678",
                CreatedAt = baseDate.AddHours(-1),
                UpdatedAt = baseDate.AddHours(-1)
            },
            new()
            {
                Id = "COL-0739",
                Title = "Shelter food service",
                Partner = "CareLink Shelter Network",
                Sector = "Food Security",
                Area = "Central Ward",
                Support = "1,200 meals/day",
                StartDate = "23 Jun",
                Status = "Accepted",
                Description = "Central kitchen supply chain delivery for three primary community centers hosting displaced residents.",
                ContactName = "Sister Teresa",
                ContactEmail = "shelter.meals@carelink.org",
                ContactPhone = "+94 77 334 1122",
                CreatedAt = baseDate.AddHours(-4),
                UpdatedAt = baseDate.AddHours(-2),
                AcceptedAt = baseDate.AddHours(-2)
            },
            new()
            {
                Id = "COL-0734",
                Title = "Community health outreach",
                Partner = "Eastbank Health Authority",
                Sector = "Health",
                Area = "Eastbank",
                Support = "8 outreach workers",
                StartDate = "25 Jun",
                Status = "In Progress",
                Description = "Door-to-door water-borne disease screenings and distribution of water purification tablets in low-lying settlements.",
                ContactName = "Dr. Asela Gunaratne",
                ContactEmail = "asela.g@eastbankhealth.org",
                ContactPhone = "+94 71 889 4455",
                CreatedAt = baseDate.AddHours(-8),
                UpdatedAt = baseDate.AddHours(-3),
                AcceptedAt = baseDate.AddHours(-6)
            },
            new()
            {
                Id = "COL-0730",
                Title = "Emergency shelter support",
                Partner = "Global Relief Network",
                Sector = "Shelter",
                Area = "North Ward",
                Support = "Temporary shelters",
                StartDate = "26 Jun",
                Status = "Completed",
                Description = "Erection of 50 weather-resistant heavy duty tents and emergency camp sanitation facilities.",
                ContactName = "Mark Henderson",
                ContactEmail = "m.henderson@grn-int.org",
                ContactPhone = "+94 76 112 9988",
                CreatedAt = baseDate.AddDays(-2),
                UpdatedAt = baseDate.AddDays(-1),
                AcceptedAt = baseDate.AddDays(-2).AddHours(4),
                CompletedAt = baseDate.AddDays(-1)
            },
            new()
            {
                Id = "COL-0728",
                Title = "Food package distribution",
                Partner = "CareLink Shelter Network",
                Sector = "Food Security",
                Area = "South Ward",
                Support = "500 food packs",
                StartDate = "27 Jun",
                Status = "Awaiting Response",
                Description = "Weekly dry ration boxes for families in isolated riverside settlements waiting for ferry transport access.",
                ContactName = "Priyantha Senanayake",
                ContactEmail = "p.sena@carelink.org",
                ContactPhone = "+94 77 665 4321",
                CreatedAt = baseDate.AddHours(-12),
                UpdatedAt = baseDate.AddHours(-12)
            },
            new()
            {
                Id = "COL-0725",
                Title = "Medical assistance programme",
                Partner = "Eastbank Health Authority",
                Sector = "Health",
                Area = "Eastbank",
                Support = "Medical team",
                StartDate = "28 Jun",
                Status = "In Progress",
                Description = "Mobile triage and maternal healthcare consultation unit for cutoff villages.",
                ContactName = "Dr. Nilmini Alwis",
                ContactEmail = "n.alwis@eastbankhealth.org",
                ContactPhone = "+94 70 223 8899",
                CreatedAt = baseDate.AddDays(-1),
                UpdatedAt = baseDate.AddHours(-5),
                AcceptedAt = baseDate.AddHours(-18)
            },
            new()
            {
                Id = "COL-0720",
                Title = "Child protection center",
                Partner = "Global Relief Network",
                Sector = "Protection",
                Area = "Central Ward",
                Support = "Child friendly space",
                StartDate = "29 Jun",
                Status = "Accepted",
                Description = "Safe recreational and psychosocial support space setup for 150 affected children.",
                ContactName = "Fathima Rizwan",
                ContactEmail = "f.rizwan@protection.ngo",
                ContactPhone = "+94 75 554 2211",
                CreatedAt = baseDate.AddDays(-1).AddHours(-4),
                UpdatedAt = baseDate.AddHours(-7),
                AcceptedAt = baseDate.AddHours(-7)
            },
            new()
            {
                Id = "COL-0715",
                Title = "Sanitation & waste management",
                Partner = "National Disaster Authority",
                Sector = "WASH",
                Area = "South Ward",
                Support = "Debris removal · toilets",
                StartDate = "30 Jun",
                Status = "Archived",
                Description = "Installation of 20 portable bio-toilets and community perimeter flood cleanup completed last week.",
                ContactName = "Gamini Wijesooriya",
                ContactEmail = "g.wije@disaster.gov",
                ContactPhone = "+94 11 998 7766",
                CreatedAt = baseDate.AddDays(-5),
                UpdatedAt = baseDate.AddDays(-3),
                AcceptedAt = baseDate.AddDays(-5).AddHours(2),
                CompletedAt = baseDate.AddDays(-3)
            }
        };

        foreach (var item in seeds)
        {
            _items[item.Id] = item;
        }
    }
}
