using System.Text;
using CareLink.Backend.Models;

namespace CareLink.Backend.Services;

public class VolunteerDirectoryService : IVolunteerDirectoryService
{
    private readonly object _lock = new();
    private readonly List<VolunteerDirectoryItemDto> _volunteers = [];
    private int _volCounter = 1500;

    public VolunteerDirectoryService()
    {
        InitializeData();
    }

    private void InitializeData()
    {
        _volunteers.AddRange([
            new VolunteerDirectoryItemDto
            {
                Id = "VOL-1048",
                Name = "Elena Rossi",
                Skills = ["First Aid", "Logistics"],
                Location = "Central Ward",
                Availability = "Available now",
                Verification = "Verified",
                Assignment = "Evacuation Support",
                Rating = "4.9",
                Phone = "+94 77 123 4567",
                Email = "elena.rossi@carelink.org",
                JoinedDate = "2026-01-12",
                HoursContributed = 48,
                CreatedAt = DateTime.UtcNow.AddMonths(-5)
            },
            new VolunteerDirectoryItemDto
            {
                Id = "VOL-1172",
                Name = "Musa Bello",
                Skills = ["Driving", "Rescue"],
                Location = "North Ward",
                Availability = "Available 14:00",
                Verification = "Verified",
                Assignment = "Unassigned",
                Rating = "4.8",
                Phone = "+94 77 234 5678",
                Email = "musa.bello@carelink.org",
                JoinedDate = "2026-02-04",
                HoursContributed = 36,
                CreatedAt = DateTime.UtcNow.AddMonths(-4)
            },
            new VolunteerDirectoryItemDto
            {
                Id = "VOL-0934",
                Name = "Lin Wei",
                Skills = ["Translation", "Medical"],
                Location = "Eastbank",
                Availability = "Available now",
                Verification = "Verified",
                Assignment = "Clinic Support",
                Rating = "4.9",
                Phone = "+94 77 345 6789",
                Email = "lin.wei@carelink.org",
                JoinedDate = "2025-11-20",
                HoursContributed = 62,
                CreatedAt = DateTime.UtcNow.AddMonths(-6)
            },
            new VolunteerDirectoryItemDto
            {
                Id = "VOL-1288",
                Name = "Sara Khan",
                Skills = ["Shelter Support", "Childcare"],
                Location = "Central Ward",
                Availability = "Available 16:00",
                Verification = "Pending",
                Assignment = "Unassigned",
                Rating = "—",
                Phone = "+94 77 456 7890",
                Email = "sara.khan@carelink.org",
                JoinedDate = "2026-03-01",
                HoursContributed = 12,
                CreatedAt = DateTime.UtcNow.AddMonths(-2)
            },
            new VolunteerDirectoryItemDto
            {
                Id = "VOL-0812",
                Name = "Daniel Okafor",
                Skills = ["Logistics", "Warehousing"],
                Location = "West District",
                Availability = "Unavailable",
                Verification = "Verified",
                Assignment = "Supply Dispatch",
                Rating = "4.7",
                Phone = "+94 77 567 8901",
                Email = "daniel.okafor@carelink.org",
                JoinedDate = "2025-10-15",
                HoursContributed = 84,
                CreatedAt = DateTime.UtcNow.AddMonths(-7)
            },
            new VolunteerDirectoryItemDto
            {
                Id = "VOL-1350",
                Name = "Amara Silva",
                Skills = ["First Aid", "Medical"],
                Location = "South Ward",
                Availability = "Available now",
                Verification = "Verified",
                Assignment = "Unassigned",
                Rating = "4.8",
                Phone = "+94 77 678 9012",
                Email = "amara.silva@carelink.org",
                JoinedDate = "2026-02-18",
                HoursContributed = 28,
                CreatedAt = DateTime.UtcNow.AddMonths(-3)
            },
            new VolunteerDirectoryItemDto
            {
                Id = "VOL-1420",
                Name = "James Carter",
                Skills = ["Driving", "Logistics"],
                Location = "North Ward",
                Availability = "Unavailable",
                Verification = "Pending",
                Assignment = "Unassigned",
                Rating = "—",
                Phone = "+94 77 789 0123",
                Email = "james.carter@carelink.org",
                JoinedDate = "2026-03-10",
                HoursContributed = 8,
                CreatedAt = DateTime.UtcNow.AddMonths(-1)
            }
        ]);
    }

    public Task<VolunteerDirectoryOverviewDto> GetOverviewAsync(VolunteerDirectoryQueryDto query)
    {
        lock (_lock)
        {
            var filtered = ApplyFilter(_volunteers, query);
            var totalCount = filtered.Count;
            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize < 1 ? 10 : query.PageSize;
            var totalPages = Math.Max(1, (int)Math.Ceiling((double)totalCount / pageSize));

            var paged = filtered
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(CloneVolunteer)
                .ToList();

            var locations = _volunteers.Select(v => v.Location).Distinct().OrderBy(l => l).ToList();
            var skills = _volunteers.SelectMany(v => v.Skills).Distinct().OrderBy(s => s).ToList();

            var overview = new VolunteerDirectoryOverviewDto
            {
                Volunteers = paged,
                Stats = CalculateStats(),
                Locations = locations,
                Skills = skills,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages
            };

            return Task.FromResult(overview);
        }
    }

    public Task<VolunteerDirectoryStatsDto> GetStatsAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(CalculateStats());
        }
    }

    public Task<VolunteerDirectoryItemDto?> GetByIdAsync(string id)
    {
        lock (_lock)
        {
            var v = _volunteers.FirstOrDefault(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(v != null ? CloneVolunteer(v) : null);
        }
    }

    public Task<VolunteerDirectoryItemDto> CreateAsync(CreateVolunteerDirectoryRequest request)
    {
        lock (_lock)
        {
            var newId = $"VOL-{_volCounter++}";
            var volunteer = new VolunteerDirectoryItemDto
            {
                Id = newId,
                Name = request.Name.Trim(),
                Location = request.Location.Trim(),
                Skills = request.Skills != null && request.Skills.Count > 0 ? request.Skills : ["General Support"],
                Availability = string.IsNullOrWhiteSpace(request.Availability) ? "Available now" : request.Availability.Trim(),
                Verification = "Pending",
                Assignment = string.IsNullOrWhiteSpace(request.Assignment) ? "Unassigned" : request.Assignment.Trim(),
                Rating = "—",
                Phone = request.Phone?.Trim() ?? string.Empty,
                Email = request.Email?.Trim() ?? string.Empty,
                JoinedDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                HoursContributed = 0,
                CreatedAt = DateTime.UtcNow
            };

            _volunteers.Insert(0, volunteer);
            return Task.FromResult(CloneVolunteer(volunteer));
        }
    }

    public Task<VolunteerDirectoryItemDto?> UpdateAsync(string id, UpdateVolunteerDirectoryRequest request)
    {
        lock (_lock)
        {
            var v = _volunteers.FirstOrDefault(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (v == null) return Task.FromResult<VolunteerDirectoryItemDto?>(null);

            if (!string.IsNullOrWhiteSpace(request.Name)) v.Name = request.Name.Trim();
            if (!string.IsNullOrWhiteSpace(request.Location)) v.Location = request.Location.Trim();
            if (request.Skills != null && request.Skills.Count > 0) v.Skills = [.. request.Skills];
            if (!string.IsNullOrWhiteSpace(request.Availability)) v.Availability = request.Availability.Trim();
            if (!string.IsNullOrWhiteSpace(request.Verification)) v.Verification = request.Verification.Trim();
            if (!string.IsNullOrWhiteSpace(request.Assignment)) v.Assignment = request.Assignment.Trim();
            if (!string.IsNullOrWhiteSpace(request.Rating)) v.Rating = request.Rating.Trim();
            if (!string.IsNullOrWhiteSpace(request.Phone)) v.Phone = request.Phone.Trim();
            if (!string.IsNullOrWhiteSpace(request.Email)) v.Email = request.Email.Trim();

            return Task.FromResult<VolunteerDirectoryItemDto?>(CloneVolunteer(v));
        }
    }

    public Task<VolunteerDirectoryItemDto?> UpdateVerificationAsync(string id, string verification)
    {
        lock (_lock)
        {
            var v = _volunteers.FirstOrDefault(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (v == null) return Task.FromResult<VolunteerDirectoryItemDto?>(null);

            v.Verification = verification;
            return Task.FromResult<VolunteerDirectoryItemDto?>(CloneVolunteer(v));
        }
    }

    public Task<VolunteerDirectoryItemDto?> UpdateAvailabilityAsync(string id, string availability)
    {
        lock (_lock)
        {
            var v = _volunteers.FirstOrDefault(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (v == null) return Task.FromResult<VolunteerDirectoryItemDto?>(null);

            v.Availability = availability;
            return Task.FromResult<VolunteerDirectoryItemDto?>(CloneVolunteer(v));
        }
    }

    public Task<bool> DeleteAsync(string id)
    {
        lock (_lock)
        {
            var v = _volunteers.FirstOrDefault(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (v == null) return Task.FromResult(false);

            _volunteers.Remove(v);
            return Task.FromResult(true);
        }
    }

    public Task<byte[]> ExportCsvAsync(VolunteerDirectoryQueryDto query)
    {
        lock (_lock)
        {
            var filtered = ApplyFilter(_volunteers, query);
            var sb = new StringBuilder();
            sb.AppendLine("Volunteer ID,Name,Skills,Location,Availability,Verification,Assignment,Rating,Phone,Email");

            foreach (var v in filtered)
            {
                var skills = string.Join(" / ", v.Skills);
                sb.AppendLine($"\"{EscapeCsv(v.Id)}\",\"{EscapeCsv(v.Name)}\",\"{EscapeCsv(skills)}\",\"{EscapeCsv(v.Location)}\",\"{EscapeCsv(v.Availability)}\",\"{EscapeCsv(v.Verification)}\",\"{EscapeCsv(v.Assignment)}\",\"{EscapeCsv(v.Rating)}\",\"{EscapeCsv(v.Phone)}\",\"{EscapeCsv(v.Email)}\"");
            }

            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            return Task.FromResult(bytes);
        }
    }

    private static List<VolunteerDirectoryItemDto> ApplyFilter(List<VolunteerDirectoryItemDto> list, VolunteerDirectoryQueryDto query)
    {
        var result = list.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim().ToLowerInvariant();
            result = result.Where(v =>
                v.Id.ToLowerInvariant().Contains(s) ||
                v.Name.ToLowerInvariant().Contains(s) ||
                v.Location.ToLowerInvariant().Contains(s) ||
                v.Skills.Any(sk => sk.ToLowerInvariant().Contains(s)));
        }

        if (!string.IsNullOrWhiteSpace(query.Skill) && !query.Skill.Equals("All skills", StringComparison.OrdinalIgnoreCase))
        {
            result = result.Where(v => v.Skills.Any(sk => sk.Equals(query.Skill, StringComparison.OrdinalIgnoreCase)));
        }

        if (!string.IsNullOrWhiteSpace(query.Location) && !query.Location.Equals("All locations", StringComparison.OrdinalIgnoreCase))
        {
            result = result.Where(v => v.Location.Equals(query.Location, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.Availability) && !query.Availability.Equals("Any availability", StringComparison.OrdinalIgnoreCase))
        {
            if (query.Availability.Equals("Available now", StringComparison.OrdinalIgnoreCase))
            {
                result = result.Where(v => v.Availability.StartsWith("Available", StringComparison.OrdinalIgnoreCase));
            }
            else if (query.Availability.Equals("Unavailable", StringComparison.OrdinalIgnoreCase))
            {
                result = result.Where(v => v.Availability.Equals("Unavailable", StringComparison.OrdinalIgnoreCase));
            }
        }

        if (!string.IsNullOrWhiteSpace(query.Verification) && !query.Verification.Equals("All verification states", StringComparison.OrdinalIgnoreCase))
        {
            result = result.Where(v => v.Verification.Equals(query.Verification, StringComparison.OrdinalIgnoreCase));
        }

        return result.ToList();
    }

    private VolunteerDirectoryStatsDto CalculateStats()
    {
        var verifiedCount = _volunteers.Count(v => v.Verification.Equals("Verified", StringComparison.OrdinalIgnoreCase));
        var availableCount = _volunteers.Count(v => v.Availability.StartsWith("Available", StringComparison.OrdinalIgnoreCase));
        var assignedCount = _volunteers.Count(v => !v.Assignment.Equals("Unassigned", StringComparison.OrdinalIgnoreCase));

        return new VolunteerDirectoryStatsDto
        {
            TotalRegistered = (2410 + _volunteers.Count).ToString("N0"),
            Verified = (2180 + verifiedCount).ToString("N0"),
            AvailableNow = (640 + availableCount).ToString("N0"),
            Assigned = (380 + assignedCount).ToString("N0")
        };
    }

    private static VolunteerDirectoryItemDto CloneVolunteer(VolunteerDirectoryItemDto v) => new()
    {
        Id = v.Id,
        Name = v.Name,
        Skills = [.. v.Skills],
        Location = v.Location,
        Availability = v.Availability,
        Verification = v.Verification,
        Assignment = v.Assignment,
        Rating = v.Rating,
        Phone = v.Phone,
        Email = v.Email,
        JoinedDate = v.JoinedDate,
        HoursContributed = v.HoursContributed,
        CreatedAt = v.CreatedAt
    };

    private static string EscapeCsv(string val) => (val ?? "").Replace("\"", "\"\"");
}
