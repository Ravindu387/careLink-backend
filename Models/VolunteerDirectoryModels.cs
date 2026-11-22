namespace CareLink.Backend.Models;

public class VolunteerDirectoryStatsDto
{
    public string TotalRegistered { get; set; } = "2,418";
    public string Verified { get; set; } = "2,184";
    public string AvailableNow { get; set; } = "642";
    public string Assigned { get; set; } = "387";
}

public class VolunteerDirectoryItemDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<string> Skills { get; set; } = [];
    public string Location { get; set; } = string.Empty;
    public string Availability { get; set; } = "Available now"; // Available now, Available 14:00, Unavailable
    public string Verification { get; set; } = "Verified"; // Verified, Pending, Rejected
    public string Assignment { get; set; } = "Unassigned";
    public string Rating { get; set; } = "4.8";
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string JoinedDate { get; set; } = string.Empty;
    public int HoursContributed { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class VolunteerDirectoryQueryDto
{
    public string? Search { get; set; }
    public string? Skill { get; set; } = "All skills";
    public string? Location { get; set; } = "All locations";
    public string? Availability { get; set; } = "Any availability";
    public string? Verification { get; set; } = "All verification states";
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class VolunteerDirectoryOverviewDto
{
    public List<VolunteerDirectoryItemDto> Volunteers { get; set; } = [];
    public VolunteerDirectoryStatsDto Stats { get; set; } = new();
    public List<string> Locations { get; set; } = [];
    public List<string> Skills { get; set; } = [];
    public int TotalCount { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalPages { get; set; } = 1;
}

public class CreateVolunteerDirectoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public List<string> Skills { get; set; } = [];
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Availability { get; set; } = "Available now";
    public string? Assignment { get; set; } = "Unassigned";
}

public class UpdateVolunteerDirectoryRequest
{
    public string? Name { get; set; }
    public string? Location { get; set; }
    public List<string>? Skills { get; set; }
    public string? Availability { get; set; }
    public string? Verification { get; set; }
    public string? Assignment { get; set; }
    public string? Rating { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
}

public class UpdateVolunteerVerificationRequest
{
    public string Verification { get; set; } = "Verified";
}

public class UpdateVolunteerAvailabilityRequest
{
    public string Availability { get; set; } = "Available now";
}
