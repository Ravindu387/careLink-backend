namespace CareLink.Application.DTOs;

public class CitizenRequestDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string CitizenName { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string TimeAgo { get; set; } = string.Empty;
    public int Progress { get; set; } = 1;
    public string FormattedDate { get; set; } = string.Empty;
    public string Updated { get; set; } = string.Empty;
    public bool CanMessage { get; set; } = true;
    public string? DispatchTrackingId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CitizenRequestsSummaryDto
{
    public int TotalRequests { get; set; }
    public int ActiveCount { get; set; }
    public int CompletedCount { get; set; }
    public int DraftsCount { get; set; }
}

public class CreateCitizenRequest
{
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CitizenName { get; set; }
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }
    public string? Priority { get; set; }
}

public class UpdateCitizenRequestStatus
{
    public string Status { get; set; } = string.Empty;
}

public class NearbyFacilityDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Distance { get; set; } = string.Empty;
    public double DistanceKm { get; set; } = 1.0;
    public string Hours { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Color { get; set; } = "green";
    public double MapX { get; set; } = 50;
    public double MapY { get; set; } = 50;
    public string IconKey { get; set; } = "House";
    public List<string> Services { get; set; } = new();
}

public class NearbySupportSummaryDto
{
    public int TotalLocations { get; set; }
    public int OpenCount { get; set; }
    public int SheltersCount { get; set; }
    public int MedicalCount { get; set; }
    public int SupportCentresCount { get; set; }
}

public class CreateFacilityRequest
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = "Support centre";
    public string Distance { get; set; } = "1.0 km";
    public double? DistanceKm { get; set; }
    public string Hours { get; set; } = "Open 24 hours";
    public string Status { get; set; } = "Open";
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Color { get; set; }
    public double? MapX { get; set; }
    public double? MapY { get; set; }
    public string? IconKey { get; set; }
    public List<string>? Services { get; set; }
}

public class CrisisOperationDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string ActiveAlertTitle { get; set; } = string.Empty;
    public string ActiveAlertDescription { get; set; } = string.Empty;
    public string ActiveAlertSeverity { get; set; } = string.Empty;
    public string Helpline { get; set; } = string.Empty;
}

public class HelpCategoryDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconKey { get; set; } = string.Empty;
}

public class CitizenHomeSummaryDto
{
    public int ActiveRequestsCount { get; set; }
    public int NearbyFacilitiesCount { get; set; }
    public int ActiveAlertsCount { get; set; }
    public int UnreadNotificationsCount { get; set; }
}

public class CitizenHomeOverviewDto
{
    public CrisisOperationDto Operation { get; set; } = new();
    public List<CitizenRequestDto> ActiveRequests { get; set; } = new();
    public List<NearbyFacilityDto> NearbyFacilities { get; set; } = new();
    public List<HelpCategoryDto> Categories { get; set; } = new();
    public CitizenHomeSummaryDto Summary { get; set; } = new();
}
