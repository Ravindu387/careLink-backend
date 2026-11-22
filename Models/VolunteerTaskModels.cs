namespace CareLink.Backend.Models;

public class VolunteerTaskDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public List<string> Skills { get; set; } = [];
    public string Priority { get; set; } = "Medium"; // Critical, High, Medium, Low
    public string Schedule { get; set; } = string.Empty;
    public string Volunteers { get; set; } = "0 / 1";
    public int RequiredVolunteers { get; set; } = 1;
    public int AssignedVolunteers { get; set; } = 0;
    public string Status { get; set; } = "Open"; // In Progress, Assigned, Open, Awaiting Acceptance, Completed
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class VolunteerTaskQueryDto
{
    public string? Tab { get; set; } = "All tasks";
    public string? Search { get; set; }
    public string? Priority { get; set; } = "All priorities";
    public string? Location { get; set; } = "All locations";
    public string? Skill { get; set; } = "All skill groups";
    public string? Schedule { get; set; } = "Any schedule";
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class VolunteerTaskTabsCountDto
{
    public int AllTasks { get; set; }
    public int Open { get; set; }
    public int Assigned { get; set; }
    public int InProgress { get; set; }
    public int Completed { get; set; }
}

public class CreateVolunteerTaskRequest
{
    public string Title { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public List<string> Skills { get; set; } = [];
    public string Priority { get; set; } = "Medium";
    public string Date { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public int Volunteers { get; set; } = 3;
}

public class VolunteerTasksOverviewDto
{
    public List<VolunteerTaskDto> Tasks { get; set; } = [];
    public VolunteerTaskTabsCountDto TabCounts { get; set; } = new();
    public List<string> Locations { get; set; } = [];
    public List<string> SkillGroups { get; set; } = [];
    public List<string> Priorities { get; set; } = [];
    public int TotalCount { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalPages { get; set; } = 1;
}

public class UpdateTaskStatusRequest
{
    public string Status { get; set; } = "Open";
}

public class UpdateVolunteerTaskRequest
{
    public string? Title { get; set; }
    public string? Location { get; set; }
    public List<string>? Skills { get; set; }
    public string? Priority { get; set; }
    public string? Schedule { get; set; }
    public int? RequiredVolunteers { get; set; }
    public int? AssignedVolunteers { get; set; }
    public string? Status { get; set; }
}

public class AssignVolunteerRequest
{
    public int Delta { get; set; } = 1; // +1 to assign, -1 to unassign
}
