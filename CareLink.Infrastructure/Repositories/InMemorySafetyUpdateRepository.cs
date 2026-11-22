namespace CareLink.Infrastructure.Repositories;

using System.Collections.Concurrent;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;

public class InMemorySafetyUpdateRepository : ISafetyUpdateRepository
{
    private readonly ConcurrentDictionary<string, SafetyUpdate> _updates = new();
    private readonly Lock _lock = new();
    private int _idCounter = 106;

    public InMemorySafetyUpdateRepository()
    {
        SeedDefaultUpdates();
    }

    private void SeedDefaultUpdates()
    {
        var seedList = new List<SafetyUpdate>
        {
            new()
            {
                Id = "SAFE-101",
                Type = "Emergency alert",
                Level = "Emergency alert",
                Severity = "Critical",
                Title = "Evacuate Riverside blocks A–D",
                Description = "Flood water is rising rapidly across the lower river basin. Move immediately to North Community Hall using Market Road. Do not use Riverside Bridge.",
                Location = "Riverside · North Ward",
                Area = "North Ward",
                Source = "Disaster Management Centre (DMC)",
                ActionInstructions = "Immediately evacuate ground-floor homes. Assemble essential survival bags and move north along Market Road to the community refuge hall.",
                SafeRoutes = new List<string> { "Market Road towards North Hill bypass", "Highland Link pathway" },
                HazardAreas = new List<string> { "Riverside Bridge (submerged)", "Lower Block C Embankment" },
                Helpline = "112",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMinutes(-12),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-12)
            },
            new()
            {
                Id = "SAFE-102",
                Type = "Warning",
                Level = "Warning",
                Severity = "High",
                Title = "Heavy rainfall expected until 18:00",
                Description = "Intense monsoon downpours forecast across Greater Riverton. Low-lying roads at severe risk of flash floods. Keep essential medicines and documents ready.",
                Location = "Greater Riverton",
                Area = "Greater Riverton",
                Source = "Department of Meteorology",
                ActionInstructions = "Stay indoors on elevated floors. Avoid unnecessary vehicular travel through underpasses and canal banks.",
                SafeRoutes = new List<string> { "Central Elevated Highway", "Airport Expressway" },
                HazardAreas = new List<string> { "Railway underpass at South Crossing", "Old Mill Road culvert" },
                Helpline = "112",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMinutes(-38),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-38)
            },
            new()
            {
                Id = "SAFE-103",
                Type = "Service update",
                Level = "Service update",
                Severity = "Medium",
                Title = "Drinking water distribution point opened",
                Description = "Municipal water bowsers providing certified safe drinking water at North Community Support Centre. Clean containers provided on site if needed.",
                Location = "North Ward",
                Area = "North Ward",
                Source = "Municipal Relief Division",
                ActionInstructions = "Bring clean 5L to 10L containers if possible. Dedicated fast-track lines for elders and families with infants.",
                SafeRoutes = new List<string> { "School Avenue", "Community Centre Gate 2" },
                HazardAreas = new List<string>(),
                Helpline = "112",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddHours(-1),
                UpdatedAt = DateTime.UtcNow.AddHours(-1)
            },
            new()
            {
                Id = "SAFE-104",
                Type = "Warning",
                Level = "Warning",
                Severity = "High",
                Title = "East Ward reservoir sluice discharge",
                Description = "Controlled excess water release from upstream reservoir underway. River embankment settlements in East Ward should remain vigilant.",
                Location = "East Ward · River Basin",
                Area = "East Ward",
                Source = "Irrigation & Water Management Authority",
                ActionInstructions = "Secure cattle, boats, and riverbank structures. Do not enter riverbanks for washing or fishing.",
                SafeRoutes = new List<string> { "East Ridge Road" },
                HazardAreas = new List<string> { "Kelani River Bank Road", "Fishermen Pier 4" },
                Helpline = "112",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddHours(-2),
                UpdatedAt = DateTime.UtcNow.AddHours(-2)
            },
            new()
            {
                Id = "SAFE-105",
                Type = "Service update",
                Level = "Service update",
                Severity = "Low",
                Title = "Mobile emergency clinic deployed",
                Description = "Red Cross mobile medical clinic stationed at Riverton High School Shelter offering wound care, insulin refrigeration, and emergency triage.",
                Location = "Riverton High School · Central Ward",
                Area = "Greater Riverton",
                Source = "Sri Lanka Red Cross Society",
                ActionInstructions = "Free basic medicines, blood pressure monitoring, and tetanus shots available between 08:00 and 20:00.",
                SafeRoutes = new List<string> { "High School Lane", "Sports Complex Entrance" },
                HazardAreas = new List<string>(),
                Helpline = "112",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddHours(-3),
                UpdatedAt = DateTime.UtcNow.AddHours(-3)
            }
        };

        foreach (var update in seedList)
        {
            _updates[update.Id] = update;
        }
    }

    public Task<IEnumerable<SafetyUpdate>> GetUpdatesAsync(string? area = null, string? level = null, string? search = null)
    {
        var query = _updates.Values.Where(u => u.IsActive).AsEnumerable();

        if (!string.IsNullOrWhiteSpace(area) && !area.Equals("All", StringComparison.OrdinalIgnoreCase) && !area.Equals("All Areas", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(u =>
                u.Area.Equals(area.Trim(), StringComparison.OrdinalIgnoreCase) ||
                u.Location.Contains(area.Trim(), StringComparison.OrdinalIgnoreCase) ||
                (area.Equals("Greater Riverton", StringComparison.OrdinalIgnoreCase) && u.Area.Contains("Ward", StringComparison.OrdinalIgnoreCase)));
        }

        if (!string.IsNullOrWhiteSpace(level) && !level.Equals("All", StringComparison.OrdinalIgnoreCase) && !level.Equals("All Updates", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(u => u.Level.Equals(level.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(u =>
                u.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                u.Description.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                u.Location.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                u.Source.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                (u.ActionInstructions != null && u.ActionInstructions.Contains(term, StringComparison.OrdinalIgnoreCase)));
        }

        var results = query.OrderByDescending(u => u.CreatedAt).ToList();
        return Task.FromResult<IEnumerable<SafetyUpdate>>(results);
    }

    public Task<SafetyUpdate?> GetByIdAsync(string id)
    {
        _updates.TryGetValue(id.Trim(), out var update);
        return Task.FromResult(update);
    }

    public Task<SafetyUpdate> CreateAsync(SafetyUpdate update)
    {
        lock (_lock)
        {
            if (string.IsNullOrWhiteSpace(update.Id))
            {
                update.Id = $"SAFE-{_idCounter++}";
            }
            update.CreatedAt = DateTime.UtcNow;
            update.UpdatedAt = DateTime.UtcNow;
            _updates[update.Id] = update;
            return Task.FromResult(update);
        }
    }

    public Task<SafetyUpdate?> UpdateAsync(string id, SafetyUpdate update)
    {
        lock (_lock)
        {
            if (_updates.TryGetValue(id.Trim(), out var existing))
            {
                existing.Title = update.Title;
                existing.Description = update.Description;
                existing.Level = update.Level;
                existing.Severity = update.Severity;
                existing.Type = update.Type;
                existing.Location = update.Location;
                existing.Area = update.Area;
                existing.Source = update.Source;
                existing.ActionInstructions = update.ActionInstructions;
                existing.SafeRoutes = update.SafeRoutes;
                existing.HazardAreas = update.HazardAreas;
                existing.IsActive = update.IsActive;
                existing.UpdatedAt = DateTime.UtcNow;
                return Task.FromResult<SafetyUpdate?>(existing);
            }
            return Task.FromResult<SafetyUpdate?>(null);
        }
    }

    public Task<bool> DeleteAsync(string id)
    {
        return Task.FromResult(_updates.TryRemove(id.Trim(), out _));
    }

    public Task<IEnumerable<string>> GetMonitoredAreasAsync()
    {
        var areas = _updates.Values
            .Select(u => u.Area)
            .Distinct()
            .Union(new[] { "North Ward", "Greater Riverton", "Riverside", "East Ward" })
            .OrderBy(a => a)
            .ToList();
        return Task.FromResult<IEnumerable<string>>(areas);
    }
}
