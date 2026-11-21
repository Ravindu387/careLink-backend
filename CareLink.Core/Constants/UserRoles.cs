namespace CareLink.Core.Constants;

public static class UserRoles
{
    public const string Admin = "Admin";
    public const string SystemAdmin = "SystemAdmin";
    public const string EmergencyCoordinator = "EmergencyCoordinator";
    public const string PartnerOfficer = "PartnerOfficer";
    public const string Volunteer = "Volunteer";
    public const string Citizen = "Citizen";
    public const string IncidentManager = "IncidentManager";
    public const string LogisticsManager = "LogisticsManager";
    public const string ShelterAdmin = "ShelterAdmin";
    public const string VolunteerCoordinator = "VolunteerCoordinator";
    public const string Donor = "Donor";

    public static readonly IReadOnlyList<string> AllRoles =
    [
        Admin,
        SystemAdmin,
        EmergencyCoordinator,
        PartnerOfficer,
        Volunteer,
        Citizen,
        IncidentManager,
        LogisticsManager,
        ShelterAdmin,
        VolunteerCoordinator,
        Donor
    ];

    public static readonly IReadOnlyList<string> CoreSystemRoles =
    [
        Admin,
        SystemAdmin,
        EmergencyCoordinator,
        PartnerOfficer,
        Volunteer,
        Citizen
    ];

    public static readonly IReadOnlyList<string> PrivilegedRoles =
    [
        Admin,
        SystemAdmin,
        EmergencyCoordinator,
        PartnerOfficer,
        IncidentManager,
        LogisticsManager,
        ShelterAdmin,
        VolunteerCoordinator
    ];

    public static bool IsValidRole(string role) =>
        AllRoles.Contains(role, StringComparer.OrdinalIgnoreCase);

    public static bool IsAdminRole(string role) =>
        string.Equals(role, Admin, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(role, SystemAdmin, StringComparison.OrdinalIgnoreCase);

    public static bool IsPrivilegedRole(string role) =>
        PrivilegedRoles.Contains(role, StringComparer.OrdinalIgnoreCase);
}
