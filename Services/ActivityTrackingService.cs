using System.Text;
using CareLink.Backend.Models;

namespace CareLink.Backend.Services;

public class ActivityTrackingService : IActivityTrackingService
{
    private readonly List<ActivityRecord> _activities = [];
    private readonly object _lock = new();
    private int _idCounter = 10;
    private int _volunteersCheckedIn = 294;
    private readonly Dictionary<string, (int count, string position)> _areaPresence = new(StringComparer.OrdinalIgnoreCase)
    {
        { "North Ward", (42, "top-[26%] left-[30%]") },
        { "Eastbank", (64, "top-[26%] right-[22%]") },
        { "Central", (86, "top-[52%] left-[52%]") },
        { "West District", (31, "bottom-[16%] left-[19%]") }
    };

    public ActivityTrackingService()
    {
        InitializeDemonstrationData();
    }

    private void InitializeDemonstrationData()
    {
        _activities.AddRange([
            new ActivityRecord
            {
                Id = "ACT-001",
                Time = "13:42",
                Volunteer = "Elena Rossi",
                Activity = "Checked in",
                Task = "TASK-782",
                Location = "North Ward",
                Verification = "GPS verified",
                CreatedAt = DateTime.UtcNow.AddMinutes(-18)
            },
            new ActivityRecord
            {
                Id = "ACT-002",
                Time = "13:28",
                Volunteer = "Daniel Okafor",
                Activity = "Progress update",
                Task = "TASK-761",
                Location = "West Warehouse",
                Verification = "76% complete",
                CreatedAt = DateTime.UtcNow.AddMinutes(-32)
            },
            new ActivityRecord
            {
                Id = "ACT-003",
                Time = "13:10",
                Volunteer = "Lin Wei",
                Activity = "Break started",
                Task = "TASK-779",
                Location = "Central Clinic",
                Verification = "Scheduled",
                CreatedAt = DateTime.UtcNow.AddMinutes(-50)
            },
            new ActivityRecord
            {
                Id = "ACT-004",
                Time = "12:55",
                Volunteer = "Musa Bello",
                Activity = "Assignment accepted",
                Task = "TASK-782",
                Location = "North Ward",
                Verification = "On time",
                CreatedAt = DateTime.UtcNow.AddMinutes(-65)
            },
            new ActivityRecord
            {
                Id = "ACT-005",
                Time = "12:40",
                Volunteer = "Sara Khan",
                Activity = "Training completed",
                Task = "TRN-104",
                Location = "Remote",
                Verification = "Passed",
                CreatedAt = DateTime.UtcNow.AddMinutes(-80)
            },
            new ActivityRecord
            {
                Id = "ACT-006",
                Time = "12:25",
                Volunteer = "Theo Martin",
                Activity = "Checked in",
                Task = "TASK-774",
                Location = "Eastbank Hub",
                Verification = "GPS verified",
                CreatedAt = DateTime.UtcNow.AddMinutes(-95)
            },
            new ActivityRecord
            {
                Id = "ACT-007",
                Time = "12:10",
                Volunteer = "Maya Fernando",
                Activity = "Progress update",
                Task = "TASK-768",
                Location = "Riverton School",
                Verification = "50% complete",
                CreatedAt = DateTime.UtcNow.AddMinutes(-110)
            },
            new ActivityRecord
            {
                Id = "ACT-008",
                Time = "11:55",
                Volunteer = "Abdul Rahman",
                Activity = "Checked out",
                Task = "TASK-761",
                Location = "West District",
                Verification = "GPS verified",
                CreatedAt = DateTime.UtcNow.AddMinutes(-125)
            },
            new ActivityRecord
            {
                Id = "ACT-009",
                Time = "11:40",
                Volunteer = "Elena Rossi",
                Activity = "Progress update",
                Task = "TASK-782",
                Location = "North Ward",
                Verification = "82% complete",
                CreatedAt = DateTime.UtcNow.AddMinutes(-140)
            },
            new ActivityRecord
            {
                Id = "ACT-010",
                Time = "11:20",
                Volunteer = "Daniel Okafor",
                Activity = "Assignment accepted",
                Task = "TASK-774",
                Location = "Eastbank Hub",
                Verification = "On time",
                CreatedAt = DateTime.UtcNow.AddMinutes(-160)
            }
        ]);
    }

    public Task<ActivityTrackingStatsDto> GetStatsAsync()
    {
        lock (_lock)
        {
            var areas = _areaPresence
                .Select(kv => new PresenceArea
                {
                    Name = kv.Key,
                    Count = kv.Value.count,
                    Position = kv.Value.position
                })
                .ToList();

            var stats = new ActivityTrackingStatsDto
            {
                VolunteersCheckedIn = _volunteersCheckedIn,
                HoursToday = $"{2184 + (_volunteersCheckedIn - 294) * 4:N0}h",
                TasksOnTrack = "89.4%",
                SafetyAlerts = 3,
                LastSync = "Just now",
                OperationalAreasCount = Math.Max(12, _areaPresence.Count),
                PresenceSummary = new FieldPresenceSummaryDto
                {
                    MedicalTeams = 58,
                    Logistics = 96,
                    ShelterSupport = 74,
                    Areas = areas
                }
            };

            return Task.FromResult(stats);
        }
    }

    public Task<PaginatedResult<ActivityRecord>> GetActivitiesAsync(ActivityTrackingQueryDto query)
    {
        lock (_lock)
        {
            var items = _activities.AsEnumerable();

            // Search query
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var s = query.Search.Trim().ToLowerInvariant();
                items = items.Where(a =>
                    a.Id.ToLowerInvariant().Contains(s) ||
                    a.Volunteer.ToLowerInvariant().Contains(s) ||
                    a.Activity.ToLowerInvariant().Contains(s) ||
                    a.Task.ToLowerInvariant().Contains(s) ||
                    a.Location.ToLowerInvariant().Contains(s) ||
                    a.Verification.ToLowerInvariant().Contains(s));
            }

            // Activity filter
            if (!string.IsNullOrWhiteSpace(query.Activity) && !query.Activity.Equals("All activities", StringComparison.OrdinalIgnoreCase))
            {
                items = items.Where(a => a.Activity.Equals(query.Activity, StringComparison.OrdinalIgnoreCase));
            }

            // Location filter
            if (!string.IsNullOrWhiteSpace(query.Location) && !query.Location.Equals("All locations", StringComparison.OrdinalIgnoreCase))
            {
                items = items.Where(a => a.Location.Equals(query.Location, StringComparison.OrdinalIgnoreCase));
            }

            var list = items.OrderByDescending(a => a.CreatedAt).ToList();
            var totalCount = list.Count;
            var page = Math.Max(1, query.PageNumber);
            var size = query.PageSize > 0 ? query.PageSize : 10;
            var pagedItems = list.Skip((page - 1) * size).Take(size).ToList();

            var result = new PaginatedResult<ActivityRecord>
            {
                Items = pagedItems,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = size
            };

            return Task.FromResult(result);
        }
    }

    public Task<ActivityRecord?> GetByIdAsync(string id)
    {
        lock (_lock)
        {
            var item = _activities.FirstOrDefault(a => a.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(item);
        }
    }

    public Task<ActivityRecord> ManualCheckInAsync(ManualCheckInRequest request)
    {
        lock (_lock)
        {
            _idCounter++;
            _volunteersCheckedIn++;

            var loc = request.Location.Trim();
            if (_areaPresence.TryGetValue(loc, out var currentArea))
            {
                _areaPresence[loc] = (currentArea.count + 1, currentArea.position);
            }

            var record = new ActivityRecord
            {
                Id = $"ACT-{_idCounter:D3}",
                Time = DateTime.UtcNow.ToString("HH:mm"),
                Volunteer = request.Volunteer.Trim(),
                Activity = "Checked in",
                Task = request.Task.Trim(),
                Location = loc,
                Verification = string.IsNullOrWhiteSpace(request.Verification) ? "GPS verified" : request.Verification.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _activities.Insert(0, record);
            return Task.FromResult(record);
        }
    }

    public Task<ActivityRecord> RecordActivityAsync(RecordActivityRequest request)
    {
        lock (_lock)
        {
            _idCounter++;
            var activityType = string.IsNullOrWhiteSpace(request.Activity) ? "Progress update" : request.Activity.Trim();
            if (activityType.Equals("Checked in", StringComparison.OrdinalIgnoreCase))
            {
                _volunteersCheckedIn++;
            }
            else if (activityType.Equals("Checked out", StringComparison.OrdinalIgnoreCase) && _volunteersCheckedIn > 0)
            {
                _volunteersCheckedIn--;
            }

            var loc = request.Location.Trim();
            if (activityType.Equals("Checked in", StringComparison.OrdinalIgnoreCase) && _areaPresence.TryGetValue(loc, out var currentArea))
            {
                _areaPresence[loc] = (currentArea.count + 1, currentArea.position);
            }

            var record = new ActivityRecord
            {
                Id = $"ACT-{_idCounter:D3}",
                Time = DateTime.UtcNow.ToString("HH:mm"),
                Volunteer = request.Volunteer.Trim(),
                Activity = activityType,
                Task = request.Task.Trim(),
                Location = loc,
                Verification = string.IsNullOrWhiteSpace(request.Verification) ? "Manual entry" : request.Verification.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _activities.Insert(0, record);
            return Task.FromResult(record);
        }
    }

    public Task<ActivityRecord?> UpdateActivityAsync(string id, UpdateActivityRequest request)
    {
        lock (_lock)
        {
            var item = _activities.FirstOrDefault(a => a.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (item == null) return Task.FromResult<ActivityRecord?>(null);

            if (!string.IsNullOrWhiteSpace(request.Volunteer)) item.Volunteer = request.Volunteer.Trim();
            if (!string.IsNullOrWhiteSpace(request.Activity)) item.Activity = request.Activity.Trim();
            if (!string.IsNullOrWhiteSpace(request.Task)) item.Task = request.Task.Trim();
            if (!string.IsNullOrWhiteSpace(request.Location)) item.Location = request.Location.Trim();
            if (!string.IsNullOrWhiteSpace(request.Verification)) item.Verification = request.Verification.Trim();
            if (request.Notes != null) item.Notes = request.Notes;

            return Task.FromResult<ActivityRecord?>(item);
        }
    }

    public Task<bool> DeleteActivityAsync(string id)
    {
        lock (_lock)
        {
            var item = _activities.FirstOrDefault(a => a.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (item == null) return Task.FromResult(false);

            if (item.Activity.Equals("Checked in", StringComparison.OrdinalIgnoreCase) && _volunteersCheckedIn > 0)
            {
                _volunteersCheckedIn--;
                if (_areaPresence.TryGetValue(item.Location, out var area) && area.count > 0)
                {
                    _areaPresence[item.Location] = (area.count - 1, area.position);
                }
            }

            _activities.Remove(item);
            return Task.FromResult(true);
        }
    }

    public Task<List<string>> GetLocationsAsync()
    {
        lock (_lock)
        {
            var locations = _activities
                .Select(a => a.Location)
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Distinct()
                .OrderBy(l => l)
                .ToList();

            return Task.FromResult(locations);
        }
    }

    public Task<byte[]> ExportCsvAsync(ActivityTrackingQueryDto query)
    {
        lock (_lock)
        {
            var paged = GetActivitiesAsync(new ActivityTrackingQueryDto
            {
                Search = query.Search,
                Activity = query.Activity,
                Location = query.Location,
                PageNumber = 1,
                PageSize = 10000
            }).Result;

            var sb = new StringBuilder();
            sb.AppendLine("Activity ID,Time,Volunteer,Activity,Task,Location,Verification,Created At");

            foreach (var a in paged.Items)
            {
                sb.AppendLine($"\"{EscapeCsv(a.Id)}\",\"{EscapeCsv(a.Time)}\",\"{EscapeCsv(a.Volunteer)}\",\"{EscapeCsv(a.Activity)}\",\"{EscapeCsv(a.Task)}\",\"{EscapeCsv(a.Location)}\",\"{EscapeCsv(a.Verification)}\",\"{a.CreatedAt:u}\"");
            }

            return Task.FromResult(Encoding.UTF8.GetBytes(sb.ToString()));
        }
    }

    private static string EscapeCsv(string? value)
    {
        return (value ?? string.Empty).Replace("\"", "\"\"");
    }
}
