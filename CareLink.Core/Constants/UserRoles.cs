namespace CareLink.Core.Constants;

public static class UserRoles
{
    public const string Citizen = "Citizen";
    public const string EmergencyCoordinator = "EmergencyCoordinator";
    public const string IncidentManager = "IncidentManager";
    public const string LogisticsManager = "LogisticsManager";
    public const string ShelterAdmin = "ShelterAdmin";
    public const string VolunteerCoordinator = "VolunteerCoordinator";
    public const string Donor = "Donor";
    public const string PartnerOfficer = "PartnerOfficer";
    public const string SystemAdmin = "SystemAdmin";

    public static readonly IReadOnlyList<string> AllRoles =
    [
        Citizen,
        EmergencyCoordinator,
        IncidentManager,
        LogisticsManager,
        ShelterAdmin,
        VolunteerCoordinator,
        Donor,
        PartnerOfficer,
        SystemAdmin
    ];

    public static bool IsValidRole(string role) =>
        AllRoles.Contains(role, StringComparer.OrdinalIgnoreCase);
}
