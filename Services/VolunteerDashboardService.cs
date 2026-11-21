using System.Text;
using CareLink.Backend.Models;

namespace CareLink.Backend.Services;

public class VolunteerDashboardService : IVolunteerDashboardService
{
    private readonly object _lock = new();
    private readonly IVolunteerTasksService? _tasksService;
    private readonly List<DashboardStatItemDto> _stats = [];
    private readonly List<DashboardSkillAvailabilityDto> _skills = [];
    private readonly List<WeeklyTaskChartItemDto> _weeklyData = [];
    private readonly List<DashboardUrgentTaskDto> _urgentTasks = [];
    private readonly List<DashboardActivityItemDto> _activities = [];

    public VolunteerDashboardService(IVolunteerTasksService? tasksService = null)
    {
        _tasksService = tasksService;
        InitializeData();
    }

    private void InitializeData()
    {
        _stats.AddRange([
            new DashboardStatItemDto { Title = "Registered volunteers", Value = "2,418", Subtitle = "+84 this week", Key = "registered_volunteers", Color = "blue" },
            new DashboardStatItemDto { Title = "Available now", Value = "642", Subtitle = "Within affected areas", Key = "available_now", Color = "green" },
            new DashboardStatItemDto { Title = "Active assignments", Value = "387", Subtitle = "92% accepted", Key = "active_assignments", Color = "blue" },
            new DashboardStatItemDto { Title = "Open tasks", Value = "74", Subtitle = "18 urgent", Key = "open_tasks", Color = "amber" },
            new DashboardStatItemDto { Title = "Pending verification", Value = "18", Subtitle = "4 high priority", Key = "pending_verification", Color = "amber" },
            new DashboardStatItemDto { Title = "Task completion", Value = "89.4%", Subtitle = "+2.3% this week", Key = "task_completion", Color = "green" },
            new DashboardStatItemDto { Title = "Hours contributed", Value = "14,280", Subtitle = "Current reporting period", Key = "hours_contributed", Color = "blue" },
            new DashboardStatItemDto { Title = "Safety alerts", Value = "3", Subtitle = "1 action required", Key = "safety_alerts", Color = "red" },
        ]);

        _skills.AddRange([
            new DashboardSkillAvailabilityDto { Name = "Logistics & distribution", Available = 184, Percentage = 84 },
            new DashboardSkillAvailabilityDto { Name = "First aid & medical", Available = 96, Percentage = 70 },
            new DashboardSkillAvailabilityDto { Name = "Driving & transport", Available = 128, Percentage = 81 },
            new DashboardSkillAvailabilityDto { Name = "Shelter support", Available = 142, Percentage = 80 },
            new DashboardSkillAvailabilityDto { Name = "Translation", Available = 52, Percentage = 58 },
            new DashboardSkillAvailabilityDto { Name = "Search & rescue", Available = 40, Percentage = 57 },
        ]);

        _weeklyData.AddRange([
            new WeeklyTaskChartItemDto { Day = "Mon", Created = 12, Completed = 9 },
            new WeeklyTaskChartItemDto { Day = "Tue", Created = 16, Completed = 11 },
            new WeeklyTaskChartItemDto { Day = "Wed", Created = 14, Completed = 10 },
            new WeeklyTaskChartItemDto { Day = "Thu", Created = 20, Completed = 17 },
            new WeeklyTaskChartItemDto { Day = "Fri", Created = 18, Completed = 15 },
            new WeeklyTaskChartItemDto { Day = "Sat", Created = 25, Completed = 21 },
            new WeeklyTaskChartItemDto { Day = "Today", Created = 17, Completed = 11 },
        ]);

        _urgentTasks.AddRange([
            new DashboardUrgentTaskDto
            {
                Id = "TASK-782",
                Title = "Evacuation transport support",
                Location = "North Ward",
                Skills = "Driving · Rescue",
                Priority = "Critical",
                Schedule = "Today · 14:00",
                Filled = "2 / 3",
                Status = "In Progress"
            },
            new DashboardUrgentTaskDto
            {
                Id = "TASK-779",
                Title = "Medical point registration",
                Location = "Central Clinic",
                Skills = "First Aid · Admin",
                Priority = "High",
                Schedule = "Today · 15:30",
                Filled = "1 / 2",
                Status = "Assigned"
            },
            new DashboardUrgentTaskDto
            {
                Id = "TASK-774",
                Title = "Food pack distribution",
                Location = "Eastbank Hub",
                Skills = "Logistics",
                Priority = "High",
                Schedule = "Today · 16:00",
                Filled = "4 / 6",
                Status = "Open"
            },
            new DashboardUrgentTaskDto
            {
                Id = "TASK-655",
                Title = "Emergency medical assistance",
                Location = "Central Ward",
                Skills = "Medical · First Aid",
                Priority = "Critical",
                Schedule = "Today · 17:00",
                Filled = "1 / 4",
                Status = "Open"
            }
        ]);

        _activities.AddRange([
            new DashboardActivityItemDto
            {
                Id = "ACT-1",
                Title = "Volunteer checked in",
                Description = "Elena Rossi · TASK-782",
                Time = "4 min",
                Type = "checkin",
                CreatedAt = DateTime.UtcNow.AddMinutes(-4)
            },
            new DashboardActivityItemDto
            {
                Id = "ACT-2",
                Title = "AI match ready for review",
                Description = "TASK-774 · 6 candidates",
                Time = "12 min",
                Type = "ai_match",
                CreatedAt = DateTime.UtcNow.AddMinutes(-12)
            },
            new DashboardActivityItemDto
            {
                Id = "ACT-3",
                Title = "Verification approved",
                Description = "Maya Fernando · Nursing",
                Time = "19 min",
                Type = "verification",
                CreatedAt = DateTime.UtcNow.AddMinutes(-19)
            },
            new DashboardActivityItemDto
            {
                Id = "ACT-4",
                Title = "Task needs replacement",
                Description = "TASK-768 · North Ward",
                Time = "27 min",
                Type = "replacement",
                CreatedAt = DateTime.UtcNow.AddMinutes(-27)
            }
        ]);
    }

    public async Task<VolunteerDashboardOverviewDto> GetOverviewAsync()
    {
        var tasksList = await GetLiveOrLocalTasksAsync();

        lock (_lock)
        {
            var overview = new VolunteerDashboardOverviewDto
            {
                Stats = _stats.Select(s => new DashboardStatItemDto
                {
                    Title = s.Title,
                    Value = s.Value,
                    Subtitle = s.Subtitle,
                    Key = s.Key,
                    Color = s.Color
                }).ToList(),
                Skills = _skills.Select(sk => new DashboardSkillAvailabilityDto
                {
                    Name = sk.Name,
                    Available = sk.Available,
                    Percentage = sk.Percentage
                }).ToList(),
                WeeklyData = _weeklyData.Select(w => new WeeklyTaskChartItemDto
                {
                    Day = w.Day,
                    Created = w.Created,
                    Completed = w.Completed
                }).ToList(),
                UrgentTasks = tasksList,
                RecentActivities = _activities.Select(a => new DashboardActivityItemDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    Time = a.Time,
                    Type = a.Type,
                    CreatedAt = a.CreatedAt
                }).ToList(),
                OperationsAlert = "Realistic sample records for the Greater Riverton flood response.",
                LastUpdated = DateTime.UtcNow
            };

            return overview;
        }
    }

    public Task<List<DashboardStatItemDto>> GetStatsAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(_stats.ToList());
        }
    }

    public async Task<List<DashboardUrgentTaskDto>> GetUrgentTasksAsync()
    {
        return await GetLiveOrLocalTasksAsync();
    }

    public Task<List<DashboardActivityItemDto>> GetActivitiesAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(_activities.ToList());
        }
    }

    public Task<DashboardActivityItemDto> AddActivityAsync(CreateDashboardActivityRequest request)
    {
        lock (_lock)
        {
            var activity = new DashboardActivityItemDto
            {
                Id = $"ACT-{_activities.Count + 1}",
                Title = request.Title.Trim(),
                Description = request.Description.Trim(),
                Time = "Just now",
                Type = string.IsNullOrWhiteSpace(request.Type) ? "general" : request.Type,
                CreatedAt = DateTime.UtcNow
            };

            _activities.Insert(0, activity);
            return Task.FromResult(activity);
        }
    }

    public async Task<byte[]> ExportDashboardSummaryAsync()
    {
        var tasks = await GetLiveOrLocalTasksAsync();

        lock (_lock)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Volunteer Coordination Dashboard Summary");
            sb.AppendLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            sb.AppendLine("----------------------------------------");
            sb.AppendLine();
            sb.AppendLine("KEY METRICS:");
            foreach (var stat in _stats)
            {
                sb.AppendLine($"{stat.Title}: {stat.Value} ({stat.Subtitle})");
            }
            sb.AppendLine();
            sb.AppendLine("SKILL AVAILABILITY:");
            foreach (var skill in _skills)
            {
                sb.AppendLine($"{skill.Name}: {skill.Available} available ({skill.Percentage}%)");
            }
            sb.AppendLine();
            sb.AppendLine("URGENT OPEN TASKS:");
            sb.AppendLine("Task ID,Title,Location,Skills,Priority,Schedule,Filled,Status");
            foreach (var task in tasks)
            {
                sb.AppendLine($"\"{task.Id}\",\"{task.Title}\",\"{task.Location}\",\"{task.Skills}\",\"{task.Priority}\",\"{task.Schedule}\",\"{task.Filled}\",\"{task.Status}\"");
            }

            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            return bytes;
        }
    }

    private async Task<List<DashboardUrgentTaskDto>> GetLiveOrLocalTasksAsync()
    {
        if (_tasksService != null)
        {
            try
            {
                var overview = await _tasksService.GetTasksOverviewAsync(new VolunteerTaskQueryDto
                {
                    PageSize = 10
                });

                if (overview?.Tasks != null && overview.Tasks.Count > 0)
                {
                    var urgent = overview.Tasks
                        .Where(t => t.Priority.Equals("Critical", StringComparison.OrdinalIgnoreCase) ||
                                    t.Priority.Equals("High", StringComparison.OrdinalIgnoreCase))
                        .Take(6)
                        .Select(t => new DashboardUrgentTaskDto
                        {
                            Id = t.Id,
                            Title = t.Title,
                            Location = t.Location,
                            Skills = string.Join(" · ", t.Skills),
                            Priority = t.Priority,
                            Schedule = t.Schedule,
                            Filled = t.Volunteers,
                            Status = t.Status
                        }).ToList();

                    if (urgent.Count > 0)
                    {
                        return urgent;
                    }
                }
            }
            catch
            {
                // Fall back to local list
            }
        }

        lock (_lock)
        {
            return _urgentTasks.ToList();
        }
    }
}
