using System.Text;
using CareLink.Backend.Models;

namespace CareLink.Backend.Services;

public class AssignmentsService : IAssignmentsService
{
    private readonly List<AssignmentItem> _assignments = [];
    private readonly object _lock = new();

    public AssignmentsService()
    {
        InitializeDemonstrationData();
    }

    private void InitializeDemonstrationData()
    {
        _assignments.AddRange([
            new AssignmentItem
            {
                Id = "ASN-4421",
                Volunteer = "Elena Rossi",
                Task = "Evacuation Support",
                Location = "North Ward",
                Schedule = "13:30–18:00",
                Status = "Checked In",
                Progress = 82,
                CreatedAt = DateTime.UtcNow.AddMinutes(-35)
            },
            new AssignmentItem
            {
                Id = "ASN-4418",
                Volunteer = "Lin Wei",
                Task = "Clinic Support",
                Location = "Central Clinic",
                Schedule = "12:00–20:00",
                Status = "In Progress",
                Progress = 65,
                CreatedAt = DateTime.UtcNow.AddMinutes(-50)
            },
            new AssignmentItem
            {
                Id = "ASN-4412",
                Volunteer = "Daniel Okafor",
                Task = "Supply Dispatch",
                Location = "West Warehouse",
                Schedule = "10:00–17:00",
                Status = "In Progress",
                Progress = 76,
                CreatedAt = DateTime.UtcNow.AddMinutes(-75)
            },
            new AssignmentItem
            {
                Id = "ASN-4407",
                Volunteer = "Musa Bello",
                Task = "Rescue Vehicle Standby",
                Location = "North Ward",
                Schedule = "14:00–22:00",
                Status = "Awaiting Acceptance",
                Progress = 0,
                CreatedAt = DateTime.UtcNow.AddMinutes(-90)
            },
            new AssignmentItem
            {
                Id = "ASN-4402",
                Volunteer = "Sara Khan",
                Task = "Shelter Support",
                Location = "Central Ward",
                Schedule = "09:00–16:00",
                Status = "Checked In",
                Progress = 55,
                CreatedAt = DateTime.UtcNow.AddMinutes(-120)
            },
            new AssignmentItem
            {
                Id = "ASN-4398",
                Volunteer = "Maya Fernando",
                Task = "Medical Assistance",
                Location = "Central Clinic",
                Schedule = "08:00–16:00",
                Status = "Completed",
                Progress = 100,
                CreatedAt = DateTime.UtcNow.AddMinutes(-180)
            },
            new AssignmentItem
            {
                Id = "ASN-4392",
                Volunteer = "Theo Martin",
                Task = "Rescue Operations",
                Location = "West District",
                Schedule = "11:00–19:00",
                Status = "In Progress",
                Progress = 45,
                CreatedAt = DateTime.UtcNow.AddMinutes(-210)
            },
            new AssignmentItem
            {
                Id = "ASN-4387",
                Volunteer = "Abdul Rahman",
                Task = "Transport Support",
                Location = "North Ward",
                Schedule = "14:00–22:00",
                Status = "Awaiting Acceptance",
                Progress = 0,
                CreatedAt = DateTime.UtcNow.AddMinutes(-240)
            }
        ]);
    }

    public Task<AssignmentsStatsDto> GetStatsAsync()
    {
        lock (_lock)
        {
            var baseActive = 387;
            var baseAwaiting = 28;
            var baseCheckedIn = 294;
            var baseReplacement = 6;
            var baseTasks = 64;

            var localActive = _assignments.Count(a => a.Status != "Completed");
            var localAwaiting = _assignments.Count(a => a.Status == "Awaiting Acceptance");
            var localCheckedIn = _assignments.Count(a => a.Status == "Checked In");

            var added = _assignments.Count - 8;
            var active = baseActive + Math.Max(0, added);
            var awaiting = baseAwaiting + Math.Max(0, localAwaiting - 2);
            var checkedIn = baseCheckedIn + Math.Max(0, localCheckedIn - 2);

            var stats = new AssignmentsStatsDto
            {
                ActiveAssignments = active,
                AwaitingAcceptance = awaiting,
                CheckedIn = checkedIn,
                ReplacementNeeded = baseReplacement,
                TasksCount = baseTasks,
                ExpiringSoonAlert = "12 expire within 2 hours",
                CheckedInRate = "76% of active"
            };

            return Task.FromResult(stats);
        }
    }

    public Task<PaginatedResult<AssignmentItem>> GetAssignmentsAsync(AssignmentsQueryDto query)
    {
        lock (_lock)
        {
            var items = _assignments.AsEnumerable();

            // Search filter
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var s = query.Search.Trim().ToLowerInvariant();
                items = items.Where(a =>
                    a.Id.ToLowerInvariant().Contains(s) ||
                    a.Volunteer.ToLowerInvariant().Contains(s) ||
                    a.Task.ToLowerInvariant().Contains(s) ||
                    a.Location.ToLowerInvariant().Contains(s) ||
                    a.Schedule.ToLowerInvariant().Contains(s));
            }

            // Status filter
            if (!string.IsNullOrWhiteSpace(query.Status) && !query.Status.Equals("All assignment states", StringComparison.OrdinalIgnoreCase))
            {
                items = items.Where(a => a.Status.Equals(query.Status, StringComparison.OrdinalIgnoreCase));
            }

            // Task filter
            if (!string.IsNullOrWhiteSpace(query.Task) && !query.Task.Equals("All tasks", StringComparison.OrdinalIgnoreCase))
            {
                items = items.Where(a => a.Task.Equals(query.Task, StringComparison.OrdinalIgnoreCase));
            }

            // Location filter
            if (!string.IsNullOrWhiteSpace(query.Location) && !query.Location.Equals("All locations", StringComparison.OrdinalIgnoreCase))
            {
                items = items.Where(a => a.Location.Equals(query.Location, StringComparison.OrdinalIgnoreCase));
            }

            // Schedule filter
            if (!string.IsNullOrWhiteSpace(query.Schedule) && !query.Schedule.Equals("Any schedule", StringComparison.OrdinalIgnoreCase) && !query.Schedule.Equals("Today", StringComparison.OrdinalIgnoreCase))
            {
                items = items.Where(a => a.Schedule.Equals(query.Schedule, StringComparison.OrdinalIgnoreCase));
            }

            // Progress filter
            if (query.Progress.HasValue)
            {
                if (query.Progress.Value == 0)
                {
                    items = items.Where(a => a.Progress == 0);
                }
                else if (query.Progress.Value == 100)
                {
                    items = items.Where(a => a.Progress == 100);
                }
            }

            var list = items.OrderByDescending(a => a.CreatedAt).ToList();
            var totalCount = list.Count;
            var page = Math.Max(1, query.PageNumber);
            var size = query.PageSize > 0 ? query.PageSize : 4;
            var pagedItems = list.Skip((page - 1) * size).Take(size).ToList();

            var result = new PaginatedResult<AssignmentItem>
            {
                Items = pagedItems,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = size
            };

            return Task.FromResult(result);
        }
    }

    public Task<AssignmentItem?> GetByIdAsync(string id)
    {
        lock (_lock)
        {
            var item = _assignments.FirstOrDefault(a => a.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(item);
        }
    }

    public Task<AssignmentItem> CreateAssignmentAsync(CreateAssignmentRequest request)
    {
        lock (_lock)
        {
            var randomSuffix = new Random().Next(4422, 4999);
            var assignment = new AssignmentItem
            {
                Id = $"ASN-{randomSuffix}",
                Volunteer = request.Volunteer.Trim(),
                Task = request.Task.Trim(),
                Location = request.Location.Trim(),
                Schedule = request.Schedule.Trim(),
                Status = "Awaiting Acceptance",
                Progress = 0,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow
            };

            _assignments.Insert(0, assignment);
            return Task.FromResult(assignment);
        }
    }

    public Task<AssignmentItem?> UpdateAssignmentAsync(string id, UpdateAssignmentRequest request)
    {
        lock (_lock)
        {
            var item = _assignments.FirstOrDefault(a => a.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (item != null)
            {
                if (!string.IsNullOrWhiteSpace(request.Status)) item.Status = request.Status;
                if (request.Progress.HasValue) item.Progress = Math.Clamp(request.Progress.Value, 0, 100);
                if (!string.IsNullOrWhiteSpace(request.Schedule)) item.Schedule = request.Schedule;
                if (!string.IsNullOrWhiteSpace(request.Location)) item.Location = request.Location;
                if (!string.IsNullOrWhiteSpace(request.Notes)) item.Notes = request.Notes;
            }

            return Task.FromResult(item);
        }
    }

    public Task<AssignmentOptionsDto> GetOptionsAsync()
    {
        lock (_lock)
        {
            var tasks = _assignments.Select(a => a.Task).Distinct().OrderBy(t => t).ToList();
            var locations = _assignments.Select(a => a.Location).Distinct().OrderBy(l => l).ToList();
            var schedules = _assignments.Select(a => a.Schedule).Distinct().OrderBy(s => s).ToList();

            return Task.FromResult(new AssignmentOptionsDto
            {
                Tasks = tasks,
                Locations = locations,
                Schedules = schedules
            });
        }
    }

    public Task<byte[]> ExportCsvAsync(AssignmentsQueryDto query)
    {
        lock (_lock)
        {
            var paged = GetAssignmentsAsync(new AssignmentsQueryDto
            {
                Search = query.Search,
                Status = query.Status,
                Task = query.Task,
                Location = query.Location,
                Schedule = query.Schedule,
                Progress = query.Progress,
                PageNumber = 1,
                PageSize = 10000
            }).Result;

            var sb = new StringBuilder();
            sb.AppendLine("Assignment ID,Volunteer,Task,Location,Schedule,Status,Progress");

            foreach (var a in paged.Items)
            {
                sb.AppendLine($"\"{EscapeCsv(a.Id)}\",\"{EscapeCsv(a.Volunteer)}\",\"{EscapeCsv(a.Task)}\",\"{EscapeCsv(a.Location)}\",\"{EscapeCsv(a.Schedule)}\",\"{EscapeCsv(a.Status)}\",\"{a.Progress}%\"");
            }

            return Task.FromResult(Encoding.UTF8.GetBytes(sb.ToString()));
        }
    }

    private static string EscapeCsv(string? value)
    {
        return (value ?? string.Empty).Replace("\"", "\"\"");
    }
}
