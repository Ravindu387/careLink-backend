namespace CareLink.Application.DTOs.Dashboard;

public class DashboardSummaryDto
{
    public int OpenRequestsCount { get; set; }
    public int OpenRequestsTodayDelta { get; set; }
    public int CriticalRequestsCount { get; set; }
    public int OverdueCriticalCount { get; set; }
    public int PeopleInSheltersCount { get; set; }
    public int ShelterCapacityPercentage { get; set; }
    public int ResourcesAvailablePercentage { get; set; }
    public int ResourceShortagesCount { get; set; }
    public int TotalRegisteredUsers { get; set; }
    public int ActiveUsersCount { get; set; }
    public int PendingVerificationCount { get; set; }
    public int ActiveVolunteersCount { get; set; }
    public int PendingPartnerApprovalsCount { get; set; }
    public string ActiveCrisisName { get; set; } = "Monsoon Floods 2025";
    public string ActiveCrisisStatus { get; set; } = "Active Emergency";
    public IReadOnlyList<RoleDistributionDto> RoleDistribution { get; set; } = new List<RoleDistributionDto>();
}

public class RoleDistributionDto
{
    public string Role { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class DashboardChartPointDto
{
    public string Day { get; set; } = string.Empty;
    public int Received { get; set; }
    public int Fulfilled { get; set; }
}

public class DashboardRequestPreviewDto
{
    public string Id { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int AffectedPeople { get; set; }
    public string Contact { get; set; } = string.Empty;
    public bool Critical { get; set; }
    public string TimeAgo { get; set; } = string.Empty;
}

public class DashboardActivityItemDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TimeAgo { get; set; } = string.Empty;
    public string Icon { get; set; } = "Truck";
    public string Color { get; set; } = "blue";
    public DateTime Timestamp { get; set; }
}

public class DashboardAlertItemDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = "Warning";
    public string TimeAgo { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class DashboardDataDto
{
    public DashboardSummaryDto Summary { get; set; } = new();
    public IReadOnlyList<DashboardChartPointDto> WeeklyTrend { get; set; } = new List<DashboardChartPointDto>();
    public IReadOnlyList<DashboardRequestPreviewDto> UrgentRequests { get; set; } = new List<DashboardRequestPreviewDto>();
    public IReadOnlyList<DashboardActivityItemDto> RecentActivities { get; set; } = new List<DashboardActivityItemDto>();
    public IReadOnlyList<DashboardAlertItemDto> ActiveAlerts { get; set; } = new List<DashboardAlertItemDto>();
}
