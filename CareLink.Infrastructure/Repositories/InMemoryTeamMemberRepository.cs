using System.Collections.Concurrent;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;

namespace CareLink.Infrastructure.Repositories;

public class InMemoryTeamMemberRepository : ITeamMemberRepository
{
    private readonly ConcurrentDictionary<string, TeamMember> _members = new(StringComparer.OrdinalIgnoreCase);
    private int _idSequence = 400;

    public InMemoryTeamMemberRepository()
    {
        SeedDefaults();
    }

    private void SeedDefaults()
    {
        _members.Clear();

        var seeds = new List<TeamMember>
        {
            new()
            {
                Id = "USR-0142",
                Name = "Sara Khan",
                Role = "Partner Officer",
                Sector = "Coordination",
                Area = "Greater Riverton",
                Contact = "+94 77 412 8014",
                Email = "sara.khan@example.org",
                LastActive = "Online",
                Status = "Active",
                Description = "Senior partner officer overseeing general coordination across Greater Riverton.",
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-120),
                UpdatedAt = DateTimeOffset.UtcNow.AddMinutes(-5)
            },
            new()
            {
                Id = "USR-0188",
                Name = "Michael Torres",
                Role = "Programme Manager",
                Sector = "Food Security",
                Area = "Central · North",
                Contact = "+94 77 412 8042",
                Email = "michael.torres@example.org",
                LastActive = "18 min ago",
                Status = "Active",
                Description = "Programme manager leading food security distribution and cluster coordination.",
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-90),
                UpdatedAt = DateTimeOffset.UtcNow.AddMinutes(-18)
            },
            new()
            {
                Id = "USR-0241",
                Name = "Nafisa Silva",
                Role = "Field Team Lead",
                Sector = "WASH",
                Area = "North Ward",
                Contact = "+94 77 412 8098",
                Email = "nafisa.silva@example.org",
                LastActive = "6 min ago",
                Status = "Active",
                Description = "Field response team lead supervising water sanitation projects in North Ward.",
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-60),
                UpdatedAt = DateTimeOffset.UtcNow.AddMinutes(-6)
            },
            new()
            {
                Id = "USR-0284",
                Name = "Ayo Mensah",
                Role = "Protection Officer",
                Sector = "Protection",
                Area = "All districts",
                Contact = "+94 77 412 8063",
                Email = "ayo.mensah@example.org",
                LastActive = "42 min ago",
                Status = "Active",
                Description = "Protection officer coordinating vulnerable population safeguarding.",
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-45),
                UpdatedAt = DateTimeOffset.UtcNow.AddMinutes(-42)
            },
            new()
            {
                Id = "USR-0301",
                Name = "Daniel Perera",
                Role = "Logistics Officer",
                Sector = "Logistics",
                Area = "Eastbank",
                Contact = "+94 77 412 8011",
                Email = "daniel.perera@example.org",
                LastActive = "Yesterday",
                Status = "Needs Review",
                Description = "Logistics officer managing warehouse intake and dispatch along Eastbank corridor.",
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-30),
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(-1)
            },
            new()
            {
                Id = "USR-0315",
                Name = "Amara Fernando",
                Role = "Health Coordinator",
                Sector = "Health",
                Area = "South Ward",
                Contact = "+94 77 412 8055",
                Email = "amara.fernando@example.org",
                LastActive = "2 hours ago",
                Status = "Active",
                Description = "Health coordinator liaising with primary clinics and mobile health vans.",
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-20),
                UpdatedAt = DateTimeOffset.UtcNow.AddHours(-2)
            }
        };

        foreach (var member in seeds)
        {
            _members[member.Id] = member;
        }
    }

    public Task<IReadOnlyList<TeamMember>> GetAllAsync(TeamMemberQuery? query = null, CancellationToken cancellationToken = default)
    {
        IEnumerable<TeamMember> items = _members.Values.OrderByDescending(m => m.UpdatedAt);

        if (query != null)
        {
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var term = query.Search.Trim();
                items = items.Where(m =>
                    m.Id.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    m.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    m.Role.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    m.Sector.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    m.Area.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    m.Contact.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    m.Email.Contains(term, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(query.Tab) && !string.Equals(query.Tab, "All", StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(query.Tab, "Needs action", StringComparison.OrdinalIgnoreCase))
                {
                    items = items.Where(m => string.Equals(m.Status, "Needs Review", StringComparison.OrdinalIgnoreCase));
                }
                else
                {
                    items = items.Where(m => string.Equals(m.Status, query.Tab, StringComparison.OrdinalIgnoreCase));
                }
            }

            if (!string.IsNullOrWhiteSpace(query.Role) && !string.Equals(query.Role, "All roles", StringComparison.OrdinalIgnoreCase))
            {
                items = items.Where(m => string.Equals(m.Role, query.Role, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(query.Sector) && !string.Equals(query.Sector, "All sectors", StringComparison.OrdinalIgnoreCase))
            {
                items = items.Where(m => string.Equals(m.Sector, query.Sector, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(query.Location) && !string.Equals(query.Location, "All locations", StringComparison.OrdinalIgnoreCase))
            {
                items = items.Where(m => m.Area.Contains(query.Location, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(query.Status) && !string.Equals(query.Status, "All statuses", StringComparison.OrdinalIgnoreCase))
            {
                items = items.Where(m => string.Equals(m.Status, query.Status, StringComparison.OrdinalIgnoreCase));
            }
        }

        return Task.FromResult<IReadOnlyList<TeamMember>>(items.ToList());
    }

    public Task<TeamMember?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        _members.TryGetValue(id, out var member);
        return Task.FromResult(member);
    }

    public Task<TeamContactsMetrics> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        var all = _members.Values.ToList();
        var activeCount = all.Count(m => string.Equals(m.Status, "Active", StringComparison.OrdinalIgnoreCase));
        var needsReviewCount = all.Count(m => string.Equals(m.Status, "Needs Review", StringComparison.OrdinalIgnoreCase));
        var completedCount = all.Count(m => string.Equals(m.Status, "Completed", StringComparison.OrdinalIgnoreCase));
        var archivedCount = all.Count(m => string.Equals(m.Status, "Archived", StringComparison.OrdinalIgnoreCase));

        // Use dynamic counts if items exceed initial count or dynamically calculated from list
        var metrics = new TeamContactsMetrics
        {
            ActiveRecords = Math.Max(activeCount, 48 + (activeCount - 5)),
            ActiveRecordsSubtitle = "Organisation-scoped",
            NeedsReview = Math.Max(needsReviewCount, 3 + (needsReviewCount - 1)),
            NeedsReviewSubtitle = "Assigned to your team",
            Verified = "96%",
            VerifiedSubtitle = "Current assurance",
            DueSoon = 2,
            DueSoonSubtitle = "Within 30 days",

            TotalCount = Math.Max(all.Count, 48 + (all.Count - 6)),
            NeedsActionCount = Math.Max(needsReviewCount, 7 + (needsReviewCount - 1)),
            ActiveCount = Math.Max(activeCount, 48 + (activeCount - 5)),
            CompletedCount = completedCount,
            ArchivedCount = archivedCount,
            LastUpdated = DateTimeOffset.UtcNow
        };

        return Task.FromResult(metrics);
    }

    public Task<TeamMember> CreateAsync(TeamMember member, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(member.Id))
        {
            var nextId = Interlocked.Increment(ref _idSequence);
            member.Id = $"USR-{nextId:D4}";
        }

        member.CreatedAt = DateTimeOffset.UtcNow;
        member.UpdatedAt = DateTimeOffset.UtcNow;
        _members[member.Id] = member;
        return Task.FromResult(member);
    }

    public Task<TeamMember?> UpdateAsync(string id, TeamMember member, CancellationToken cancellationToken = default)
    {
        if (!_members.TryGetValue(id, out var existing))
        {
            return Task.FromResult<TeamMember?>(null);
        }

        member.Id = id;
        member.CreatedAt = existing.CreatedAt;
        member.UpdatedAt = DateTimeOffset.UtcNow;
        _members[id] = member;
        return Task.FromResult<TeamMember?>(member);
    }

    public Task<TeamMember?> UpdateStatusAsync(string id, string status, CancellationToken cancellationToken = default)
    {
        if (!_members.TryGetValue(id, out var existing))
        {
            return Task.FromResult<TeamMember?>(null);
        }

        existing.Status = status;
        existing.UpdatedAt = DateTimeOffset.UtcNow;
        return Task.FromResult<TeamMember?>(existing);
    }

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_members.TryRemove(id, out _));
    }

    public Task ResetToDefaultsAsync(CancellationToken cancellationToken = default)
    {
        SeedDefaults();
        return Task.CompletedTask;
    }
}
