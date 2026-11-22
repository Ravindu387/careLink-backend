namespace CareLink.Backend.Models;

public class ActivityRecord
{
    public string Id { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public string Volunteer { get; set; } = string.Empty;
    public string Activity { get; set; } = "Checked in";
    public string Task { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Verification { get; set; } = "GPS verified";
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class PresenceArea
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
    public string Position { get; set; } = string.Empty;
}

public class FieldPresenceSummaryDto
{
    public int MedicalTeams { get; set; } = 58;
    public int Logistics { get; set; } = 96;
    public int ShelterSupport { get; set; } = 74;
    public List<PresenceArea> Areas { get; set; } = [];
}

public class ActivityTrackingStatsDto
{
    public int VolunteersCheckedIn { get; set; } = 294;
    public string HoursToday { get; set; } = "2,184h";
    public string TasksOnTrack { get; set; } = "89.4%";
    public int SafetyAlerts { get; set; } = 3;
    public string LastSync { get; set; } = "Just now";
    public int OperationalAreasCount { get; set; } = 12;
    public FieldPresenceSummaryDto PresenceSummary { get; set; } = new();
}

public class ActivityTrackingQueryDto
{
    public string? Search { get; set; }
    public string? Activity { get; set; }
    public string? Location { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class ManualCheckInRequest
{
    public string Volunteer { get; set; } = string.Empty;
    public string Task { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Verification { get; set; } = "GPS verified";
}

public class RecordActivityRequest
{
    public string Volunteer { get; set; } = string.Empty;
    public string Activity { get; set; } = "Progress update";
    public string Task { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Verification { get; set; } = "Manual entry";
}

public class UpdateActivityRequest
{
    public string? Volunteer { get; set; }
    public string? Activity { get; set; }
    public string? Task { get; set; }
    public string? Location { get; set; }
    public string? Verification { get; set; }
    public string? Notes { get; set; }
}
