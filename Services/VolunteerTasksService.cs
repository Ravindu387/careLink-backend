using System.Text;
using CareLink.Backend.Models;

namespace CareLink.Backend.Services;

public class VolunteerTasksService : IVolunteerTasksService
{
    private readonly object _lock = new();
    private readonly List<VolunteerTaskDto> _tasks = [];
    private int _taskCounter = 800;

    public VolunteerTasksService()
    {
        InitializeTasks();
    }

    private void InitializeTasks()
    {
        _tasks.AddRange([
            new VolunteerTaskDto
            {
                Id = "TASK-782",
                Title = "Evacuation transport support",
                Location = "North Ward",
                Skills = ["Driving", "Rescue"],
                Priority = "Critical",
                Schedule = "Today · 14:00",
                Volunteers = "2 / 3",
                RequiredVolunteers = 3,
                AssignedVolunteers = 2,
                Status = "In Progress",
                CreatedAt = DateTime.UtcNow.AddHours(-5)
            },
            new VolunteerTaskDto
            {
                Id = "TASK-779",
                Title = "Medical point registration",
                Location = "Central Clinic",
                Skills = ["First Aid", "Admin"],
                Priority = "High",
                Schedule = "Today · 15:30",
                Volunteers = "1 / 2",
                RequiredVolunteers = 2,
                AssignedVolunteers = 1,
                Status = "Assigned",
                CreatedAt = DateTime.UtcNow.AddHours(-6)
            },
            new VolunteerTaskDto
            {
                Id = "TASK-774",
                Title = "Food pack distribution",
                Location = "Eastbank Hub",
                Skills = ["Logistics"],
                Priority = "High",
                Schedule = "Today · 16:00",
                Volunteers = "4 / 6",
                RequiredVolunteers = 6,
                AssignedVolunteers = 4,
                Status = "Open",
                CreatedAt = DateTime.UtcNow.AddHours(-8)
            },
            new VolunteerTaskDto
            {
                Id = "TASK-768",
                Title = "Shelter childcare support",
                Location = "Riverton School",
                Skills = ["Childcare"],
                Priority = "Medium",
                Schedule = "Tomorrow · 08:00",
                Volunteers = "0 / 3",
                RequiredVolunteers = 3,
                AssignedVolunteers = 0,
                Status = "Open",
                CreatedAt = DateTime.UtcNow.AddHours(-10)
            },
            new VolunteerTaskDto
            {
                Id = "TASK-761",
                Title = "Multilingual hotline",
                Location = "Remote",
                Skills = ["Translation"],
                Priority = "Medium",
                Schedule = "Tomorrow · 09:00",
                Volunteers = "2 / 4",
                RequiredVolunteers = 4,
                AssignedVolunteers = 2,
                Status = "Awaiting Acceptance",
                CreatedAt = DateTime.UtcNow.AddHours(-12)
            },
            new VolunteerTaskDto
            {
                Id = "TASK-682",
                Title = "Emergency dispatch staging",
                Location = "North Ward",
                Skills = ["Driving", "Rescue"],
                Priority = "Critical",
                Schedule = "Today · 14:00",
                Volunteers = "2 / 3",
                RequiredVolunteers = 3,
                AssignedVolunteers = 2,
                Status = "In Progress",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new VolunteerTaskDto
            {
                Id = "TASK-679",
                Title = "Clinic intake verification",
                Location = "Central Clinic",
                Skills = ["First Aid", "Admin"],
                Priority = "High",
                Schedule = "Today · 15:30",
                Volunteers = "1 / 2",
                RequiredVolunteers = 2,
                AssignedVolunteers = 1,
                Status = "Assigned",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new VolunteerTaskDto
            {
                Id = "TASK-674",
                Title = "Dry ration bulk packaging",
                Location = "Eastbank Hub",
                Skills = ["Logistics"],
                Priority = "High",
                Schedule = "Today · 16:00",
                Volunteers = "4 / 6",
                RequiredVolunteers = 6,
                AssignedVolunteers = 4,
                Status = "Open",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new VolunteerTaskDto
            {
                Id = "TASK-668",
                Title = "Community shelter assistance",
                Location = "Riverton School",
                Skills = ["Childcare"],
                Priority = "Medium",
                Schedule = "Tomorrow · 08:00",
                Volunteers = "0 / 3",
                RequiredVolunteers = 3,
                AssignedVolunteers = 0,
                Status = "Open",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new VolunteerTaskDto
            {
                Id = "TASK-661",
                Title = "Disaster hotline operator",
                Location = "Remote",
                Skills = ["Translation"],
                Priority = "Medium",
                Schedule = "Tomorrow · 09:00",
                Volunteers = "2 / 4",
                RequiredVolunteers = 4,
                AssignedVolunteers = 2,
                Status = "Awaiting Acceptance",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new VolunteerTaskDto
            {
                Id = "TASK-655",
                Title = "Emergency medical assistance",
                Location = "Central Ward",
                Skills = ["Medical", "First Aid"],
                Priority = "Critical",
                Schedule = "Today · 17:00",
                Volunteers = "1 / 4",
                RequiredVolunteers = 4,
                AssignedVolunteers = 1,
                Status = "Open",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new VolunteerTaskDto
            {
                Id = "TASK-648",
                Title = "Warehouse supply management",
                Location = "West District",
                Skills = ["Logistics"],
                Priority = "Low",
                Schedule = "Tomorrow · 10:00",
                Volunteers = "3 / 3",
                RequiredVolunteers = 3,
                AssignedVolunteers = 3,
                Status = "Completed",
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            }
        ]);
    }

    public Task<VolunteerTasksOverviewDto> GetTasksOverviewAsync(VolunteerTaskQueryDto query)
    {
        lock (_lock)
        {
            var tabCounts = CalculateTabCounts();
            var filtered = ApplyFilter(_tasks, query);

            var totalCount = filtered.Count;
            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize < 1 ? 10 : query.PageSize;
            var totalPages = Math.Max(1, (int)Math.Ceiling((double)totalCount / pageSize));

            var paged = filtered
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(CloneTask)
                .ToList();

            var locations = _tasks.Select(t => t.Location).Distinct().OrderBy(l => l).ToList();
            var skills = _tasks.SelectMany(t => t.Skills).Distinct().OrderBy(s => s).ToList();
            var priorities = new List<string> { "Critical", "High", "Medium", "Low" };

            var overview = new VolunteerTasksOverviewDto
            {
                Tasks = paged,
                TabCounts = tabCounts,
                Locations = locations,
                SkillGroups = skills,
                Priorities = priorities,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages
            };

            return Task.FromResult(overview);
        }
    }

    public Task<VolunteerTaskTabsCountDto> GetTabCountsAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(CalculateTabCounts());
        }
    }

    public Task<VolunteerTaskDto?> GetTaskByIdAsync(string id)
    {
        lock (_lock)
        {
            var task = _tasks.FirstOrDefault(t => t.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(task != null ? CloneTask(task) : null);
        }
    }

    public Task<VolunteerTaskDto> CreateTaskAsync(CreateVolunteerTaskRequest request)
    {
        lock (_lock)
        {
            var newId = $"TASK-{_taskCounter++}";
            string scheduleDate = request.Date?.Trim() ?? string.Empty;
            var todayStr = DateTime.UtcNow.ToString("yyyy-MM-dd");
            var tomorrowStr = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd");

            if (string.IsNullOrWhiteSpace(scheduleDate) || scheduleDate.Equals(todayStr, StringComparison.OrdinalIgnoreCase))
            {
                scheduleDate = "Today";
            }
            else if (scheduleDate.Equals(tomorrowStr, StringComparison.OrdinalIgnoreCase))
            {
                scheduleDate = "Tomorrow";
            }

            var timeStr = string.IsNullOrWhiteSpace(request.Time) ? "12:00" : request.Time.Trim();
            var schedule = $"{scheduleDate} · {timeStr}";

            var reqVolunteers = request.Volunteers > 0 ? request.Volunteers : 1;

            var task = new VolunteerTaskDto
            {
                Id = newId,
                Title = request.Title.Trim(),
                Location = request.Location.Trim(),
                Skills = request.Skills != null && request.Skills.Count > 0 ? request.Skills : ["General Support"],
                Priority = string.IsNullOrWhiteSpace(request.Priority) ? "Medium" : request.Priority,
                Schedule = schedule,
                Volunteers = $"0 / {reqVolunteers}",
                RequiredVolunteers = reqVolunteers,
                AssignedVolunteers = 0,
                Status = "Open",
                CreatedAt = DateTime.UtcNow
            };

            _tasks.Insert(0, task);
            return Task.FromResult(CloneTask(task));
        }
    }

    public Task<VolunteerTaskDto?> UpdateTaskAsync(string id, UpdateVolunteerTaskRequest request)
    {
        lock (_lock)
        {
            var task = _tasks.FirstOrDefault(t => t.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (task == null)
            {
                return Task.FromResult<VolunteerTaskDto?>(null);
            }

            if (!string.IsNullOrWhiteSpace(request.Title))
                task.Title = request.Title.Trim();

            if (!string.IsNullOrWhiteSpace(request.Location))
                task.Location = request.Location.Trim();

            if (request.Skills != null && request.Skills.Count > 0)
                task.Skills = [.. request.Skills];

            if (!string.IsNullOrWhiteSpace(request.Priority))
                task.Priority = request.Priority.Trim();

            if (!string.IsNullOrWhiteSpace(request.Schedule))
                task.Schedule = request.Schedule.Trim();

            if (request.RequiredVolunteers.HasValue && request.RequiredVolunteers.Value > 0)
                task.RequiredVolunteers = request.RequiredVolunteers.Value;

            if (request.AssignedVolunteers.HasValue && request.AssignedVolunteers.Value >= 0)
                task.AssignedVolunteers = Math.Min(request.AssignedVolunteers.Value, task.RequiredVolunteers);

            task.Volunteers = $"{task.AssignedVolunteers} / {task.RequiredVolunteers}";

            if (!string.IsNullOrWhiteSpace(request.Status))
                task.Status = request.Status.Trim();

            return Task.FromResult<VolunteerTaskDto?>(CloneTask(task));
        }
    }

    public Task<VolunteerTaskDto?> AdjustAssignedVolunteersAsync(string id, int delta)
    {
        lock (_lock)
        {
            var task = _tasks.FirstOrDefault(t => t.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (task == null)
            {
                return Task.FromResult<VolunteerTaskDto?>(null);
            }

            var newAssigned = Math.Clamp(task.AssignedVolunteers + delta, 0, task.RequiredVolunteers);
            task.AssignedVolunteers = newAssigned;
            task.Volunteers = $"{task.AssignedVolunteers} / {task.RequiredVolunteers}";

            if (task.AssignedVolunteers == 0 && task.Status != "Completed")
            {
                task.Status = "Open";
            }
            else if (task.AssignedVolunteers > 0 && task.Status == "Open")
            {
                task.Status = "Assigned";
            }

            return Task.FromResult<VolunteerTaskDto?>(CloneTask(task));
        }
    }

    public Task<VolunteerTaskDto?> UpdateTaskStatusAsync(string id, string status)
    {
        lock (_lock)
        {
            var task = _tasks.FirstOrDefault(t => t.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (task == null)
            {
                return Task.FromResult<VolunteerTaskDto?>(null);
            }

            task.Status = status;
            return Task.FromResult<VolunteerTaskDto?>(CloneTask(task));
        }
    }

    public Task<bool> DeleteTaskAsync(string id)
    {
        lock (_lock)
        {
            var task = _tasks.FirstOrDefault(t => t.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (task == null)
            {
                return Task.FromResult(false);
            }

            _tasks.Remove(task);
            return Task.FromResult(true);
        }
    }

    public Task<byte[]> ExportTasksCsvAsync(VolunteerTaskQueryDto query)
    {
        lock (_lock)
        {
            var filtered = ApplyFilter(_tasks, query);

            var sb = new StringBuilder();
            sb.AppendLine("Task ID,Task Title,Location,Required Skills,Priority,Schedule,Volunteers,Status");

            foreach (var t in filtered)
            {
                var cleanSkills = string.Join(" / ", t.Skills);
                sb.AppendLine($"\"{EscapeCsv(t.Id)}\",\"{EscapeCsv(t.Title)}\",\"{EscapeCsv(t.Location)}\",\"{EscapeCsv(cleanSkills)}\",\"{EscapeCsv(t.Priority)}\",\"{EscapeCsv(t.Schedule)}\",\"{EscapeCsv(t.Volunteers)}\",\"{EscapeCsv(t.Status)}\"");
            }

            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            return Task.FromResult(bytes);
        }
    }

    private static List<VolunteerTaskDto> ApplyFilter(List<VolunteerTaskDto> source, VolunteerTaskQueryDto query)
    {
        var result = source.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim().ToLowerInvariant();
            result = result.Where(t =>
                t.Id.ToLowerInvariant().Contains(s) ||
                t.Title.ToLowerInvariant().Contains(s) ||
                t.Location.ToLowerInvariant().Contains(s) ||
                t.Skills.Any(sk => sk.ToLowerInvariant().Contains(s)));
        }

        if (!string.IsNullOrWhiteSpace(query.Priority) && !query.Priority.Equals("All priorities", StringComparison.OrdinalIgnoreCase))
        {
            result = result.Where(t => t.Priority.Equals(query.Priority, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.Location) && !query.Location.Equals("All locations", StringComparison.OrdinalIgnoreCase))
        {
            result = result.Where(t => t.Location.Equals(query.Location, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.Skill) && !query.Skill.Equals("All skill groups", StringComparison.OrdinalIgnoreCase))
        {
            result = result.Where(t => t.Skills.Any(sk => sk.Equals(query.Skill, StringComparison.OrdinalIgnoreCase)));
        }

        if (!string.IsNullOrWhiteSpace(query.Schedule) && !query.Schedule.Equals("Any schedule", StringComparison.OrdinalIgnoreCase))
        {
            var todayStr = DateTime.UtcNow.ToString("yyyy-MM-dd");
            var tomorrowStr = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd");

            if (query.Schedule.Equals("Today", StringComparison.OrdinalIgnoreCase))
            {
                result = result.Where(t => t.Schedule.StartsWith("Today", StringComparison.OrdinalIgnoreCase) ||
                                           t.Schedule.StartsWith(todayStr, StringComparison.OrdinalIgnoreCase));
            }
            else if (query.Schedule.Equals("Tomorrow", StringComparison.OrdinalIgnoreCase))
            {
                result = result.Where(t => t.Schedule.StartsWith("Tomorrow", StringComparison.OrdinalIgnoreCase) ||
                                           t.Schedule.StartsWith(tomorrowStr, StringComparison.OrdinalIgnoreCase));
            }
        }

        if (!string.IsNullOrWhiteSpace(query.Tab) && !query.Tab.Equals("All tasks", StringComparison.OrdinalIgnoreCase))
        {
            if (query.Tab.Equals("Open", StringComparison.OrdinalIgnoreCase))
            {
                result = result.Where(t => t.Status.Equals("Open", StringComparison.OrdinalIgnoreCase));
            }
            else if (query.Tab.Equals("Assigned", StringComparison.OrdinalIgnoreCase))
            {
                result = result.Where(t => t.Status.Equals("Assigned", StringComparison.OrdinalIgnoreCase) ||
                                           t.Status.Equals("Awaiting Acceptance", StringComparison.OrdinalIgnoreCase));
            }
            else if (query.Tab.Equals("In progress", StringComparison.OrdinalIgnoreCase))
            {
                result = result.Where(t => t.Status.Equals("In Progress", StringComparison.OrdinalIgnoreCase));
            }
            else if (query.Tab.Equals("Completed", StringComparison.OrdinalIgnoreCase))
            {
                result = result.Where(t => t.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase));
            }
        }

        return result.ToList();
    }

    private VolunteerTaskTabsCountDto CalculateTabCounts()
    {
        return new VolunteerTaskTabsCountDto
        {
            AllTasks = _tasks.Count,
            Open = _tasks.Count(t => t.Status.Equals("Open", StringComparison.OrdinalIgnoreCase)),
            Assigned = _tasks.Count(t => t.Status.Equals("Assigned", StringComparison.OrdinalIgnoreCase) ||
                                         t.Status.Equals("Awaiting Acceptance", StringComparison.OrdinalIgnoreCase)),
            InProgress = _tasks.Count(t => t.Status.Equals("In Progress", StringComparison.OrdinalIgnoreCase)),
            Completed = _tasks.Count(t => t.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
        };
    }

    private static VolunteerTaskDto CloneTask(VolunteerTaskDto t) => new()
    {
        Id = t.Id,
        Title = t.Title,
        Location = t.Location,
        Skills = [.. t.Skills],
        Priority = t.Priority,
        Schedule = t.Schedule,
        Volunteers = t.Volunteers,
        RequiredVolunteers = t.RequiredVolunteers,
        AssignedVolunteers = t.AssignedVolunteers,
        Status = t.Status,
        CreatedAt = t.CreatedAt
    };

    private static string EscapeCsv(string val) => (val ?? "").Replace("\"", "\"\"");
}
