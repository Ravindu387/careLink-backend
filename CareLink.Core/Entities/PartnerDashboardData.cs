namespace CareLink.Core.Entities;

public class PartnerDashboardSummary
{
    public int ActiveCrisisEvents { get; set; } = 8;
    public string ActiveCrisisDetail { get; set; } = "3 require attention";

    public int AssistanceReferrals { get; set; } = 24;
    public string AssistanceReferralsDetail { get; set; } = "12 awaiting response";

    public int SharedResources { get; set; } = 16;
    public string SharedResourcesDetail { get; set; } = "Across 5 locations";

    public int ActiveTeamMembers { get; set; } = 32;
    public string ActiveTeamMembersDetail { get; set; } = "4 field teams deployed";
}

public class PartnerActivityItem
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public string IconType { get; set; } = "ClipboardList"; // ClipboardList, Package, Handshake, Users, AlertTriangle
    public string ColorClass { get; set; } = "text-blue-600 bg-blue-50";
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}

public class PartnerOrganisationProfileSummary
{
    public string Name { get; set; } = "Global Relief Network";
    public string Type { get; set; } = "International NGO";
    public string Code { get; set; } = "ORG-027";
    public string VerificationStatus { get; set; } = "Verified";
    public string AssignedRegion { get; set; } = "Western";
    public int FieldLocationsCount { get; set; } = 5;
    public string ContactEmail { get; set; } = "coordination@globalrelief.org";
    public string ContactPhone { get; set; } = "+94 11 234 5678";
}

public class PartnerDashboardData
{
    public string ActiveCrisisName { get; set; } = "Monsoon Floods 2025";
    public PartnerDashboardSummary Summary { get; set; } = new();
    public List<PartnerActivityItem> RecentActivities { get; set; } = new();
    public PartnerOrganisationProfileSummary Organisation { get; set; } = new();
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
}
