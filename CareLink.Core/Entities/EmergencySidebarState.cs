namespace CareLink.Core.Entities;

public class CoordinatorProfile
{
    public string Id { get; set; } = "COORD-001";
    public string Name { get; set; } = "Amina Mensah";
    public string Role { get; set; } = "Emergency Coordinator";
    public string Initials { get; set; } = "AM";
    public string Email { get; set; } = "amina.mensah@reliefos.org";
    public string DutyStatus { get; set; } = "On Duty";
    public string OperationalArea { get; set; } = "Greater Riverton";
    public string BadgeNumber { get; set; } = "EC-8842";
    public string Phone { get; set; } = "+94 77 123 4567";
    public DateTime ShiftStartedAt { get; set; } = DateTime.UtcNow.AddHours(-4);
}

public class ActiveCrisisSummary
{
    public string Id { get; set; } = "CR-2025-001";
    public string Name { get; set; } = "Western Province Monsoon Flash Floods 2025";
    public string ShortTitle { get; set; } = "Monsoon Floods 2025";
    public string Type { get; set; } = "Flood";
    public string Severity { get; set; } = "Severe · Level 4";
    public int Level { get; set; } = 4;
    public string OperationalArea { get; set; } = "Greater Riverton";
    public string Status { get; set; } = "Active";
    public int AffectedPeople { get; set; } = 112000;
    public string FormattedAffectedPeople { get; set; } = "112K";
    public string StartedDate { get; set; } = "2025-10-12";
}

public class SidebarBadgeCounts
{
    public int CrisisEvents { get; set; } = 3;
    public int AssistanceRequests { get; set; } = 86;
    public int PriorityQueue { get; set; } = 12;
    public int AiRecommendations { get; set; } = 8;
    public int Notifications { get; set; } = 14;
    public int AuditTimeline { get; set; } = 0;
}

public class SystemReadiness
{
    public string Status { get; set; } = "Operational · Level 4";
    public bool IsActiveResponse { get; set; } = true;
    public DateTime LastSyncedAt { get; set; } = DateTime.UtcNow;
    public int ActiveOperationsCount { get; set; } = 3;
    public string Heartbeat { get; set; } = "Healthy";
}

public class EmergencySidebarState
{
    public CoordinatorProfile Coordinator { get; set; } = new();
    public ActiveCrisisSummary ActiveCrisis { get; set; } = new();
    public SidebarBadgeCounts Badges { get; set; } = new();
    public SystemReadiness System { get; set; } = new();
}
