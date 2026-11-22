namespace CareLink.Backend.Models;

public class AssignmentItem
{
    public string Id { get; set; } = string.Empty;
    public string Volunteer { get; set; } = string.Empty;
    public string Task { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Schedule { get; set; } = string.Empty;
    public string Status { get; set; } = "Awaiting Acceptance";
    public int Progress { get; set; } = 0;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class AssignmentsStatsDto
{
    public int ActiveAssignments { get; set; } = 387;
    public int AwaitingAcceptance { get; set; } = 28;
    public int CheckedIn { get; set; } = 294;
    public int ReplacementNeeded { get; set; } = 6;
    public int TasksCount { get; set; } = 64;
    public string ExpiringSoonAlert { get; set; } = "12 expire within 2 hours";
    public string CheckedInRate { get; set; } = "76% of active";
}

public class AssignmentsQueryDto
{
    public string? Search { get; set; }
    public string? Status { get; set; }
    public string? Task { get; set; }
    public string? Location { get; set; }
    public string? Schedule { get; set; }
    public int? Progress { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class CreateAssignmentRequest
{
    public string Volunteer { get; set; } = string.Empty;
    public string Task { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Schedule { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class UpdateAssignmentRequest
{
    public string? Status { get; set; }
    public int? Progress { get; set; }
    public string? Schedule { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }
}

public class AssignmentOptionsDto
{
    public List<string> Tasks { get; set; } = [];
    public List<string> Locations { get; set; } = [];
    public List<string> Schedules { get; set; } = [];
}
