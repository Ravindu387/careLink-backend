namespace CareLink.Core.Constants;

public static class UserRoles
{
    // Official 9 CareLink Roles + Legacy Identity Role Aliases
    public const string Admin = "Admin";
    public const string SystemAdmin = "SystemAdmin";
    public const string CaseManager = "CaseManager";
    public const string IncidentManager = "IncidentManager";
    public const string Citizen = "Citizen";
    public const string EmergencyCoordinator = "EmergencyCoordinator";
    public const string FundingOfficer = "FundingOfficer";
    public const string Donor = "Donor";
    public const string PartnerOfficer = "PartnerOfficer";
    public const string ResourceManager = "ResourceManager";
    public const string LogisticsManager = "LogisticsManager";
    public const string ShelterAdministrator = "ShelterAdministrator";
    public const string ShelterAdmin = "ShelterAdmin";
    public const string VolunteerCoordinator = "VolunteerCoordinator";
    public const string Volunteer = "Volunteer";

    public static readonly IReadOnlyList<string> OfficialRoles =
    [
        Admin,
        CaseManager,
        Citizen,
        EmergencyCoordinator,
        FundingOfficer,
        PartnerOfficer,
        ResourceManager,
        ShelterAdministrator,
        VolunteerCoordinator
    ];

    public static readonly IReadOnlyList<string> AllRoles =
    [
        Admin,
        SystemAdmin,
        CaseManager,
        IncidentManager,
        Citizen,
        EmergencyCoordinator,
        FundingOfficer,
        Donor,
        PartnerOfficer,
        ResourceManager,
        LogisticsManager,
        ShelterAdministrator,
        ShelterAdmin,
        VolunteerCoordinator,
        Volunteer
    ];

    public static readonly IReadOnlyList<string> PublicRegistrationRoles =
    [
        Citizen,
        Volunteer,
        VolunteerCoordinator,
        PartnerOfficer
    ];

    public static readonly IReadOnlyList<string> InternalRoles =
    [
        Admin,
        SystemAdmin,
        CaseManager,
        IncidentManager,
        EmergencyCoordinator,
        FundingOfficer,
        Donor,
        ResourceManager,
        LogisticsManager,
        ShelterAdministrator,
        ShelterAdmin,
        VolunteerCoordinator
    ];

    public static readonly IReadOnlyList<string> CoreSystemRoles =
    [
        Admin,
        SystemAdmin,
        CaseManager,
        Citizen,
        EmergencyCoordinator,
        FundingOfficer,
        PartnerOfficer,
        ResourceManager,
        ShelterAdministrator,
        VolunteerCoordinator
    ];

    public static readonly IReadOnlyList<string> PrivilegedRoles =
    [
        Admin,
        SystemAdmin,
        CaseManager,
        IncidentManager,
        EmergencyCoordinator,
        FundingOfficer,
        Donor,
        PartnerOfficer,
        ResourceManager,
        LogisticsManager,
        ShelterAdministrator,
        ShelterAdmin,
        VolunteerCoordinator
    ];

    public static bool IsValidRole(string? role)
    {
        if (string.IsNullOrWhiteSpace(role)) return false;
        return TryNormalizeRoleName(role, out _);
    }

    public static bool IsPublicRegistrationRole(string? role)
    {
        if (string.IsNullOrWhiteSpace(role)) return false;
        var trimmed = role.Trim();
        var lower = trimmed.ToLowerInvariant();
        if (lower is "citizen" or "affected citizen" or "volunteer" or "partner" or "partner officer"
            or "partnerofficer" or "partner organization" or "partner organisation"
            or "partnerorganization" or "partnerorganisation")
        {
            return true;
        }
        return false;
    }

    public static bool IsAdminRole(string? role) =>
        string.Equals(role, Admin, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(role, SystemAdmin, StringComparison.OrdinalIgnoreCase);

    public static bool IsPrivilegedRole(string? role)
    {
        if (string.IsNullOrWhiteSpace(role)) return false;
        var normalized = NormalizeRoleName(role);
        return PrivilegedRoles.Contains(normalized, StringComparer.OrdinalIgnoreCase);
    }

    public static bool IsInternalOnlyRole(string? role)
    {
        if (string.IsNullOrWhiteSpace(role)) return false;
        if (IsPublicRegistrationRole(role)) return false;
        if (!TryNormalizeRoleName(role, out var normalized)) return false;
        return InternalRoles.Contains(normalized, StringComparer.OrdinalIgnoreCase);
    }

    public static bool TryNormalizeRoleName(string? role, out string normalizedRole)
    {
        normalizedRole = string.Empty;
        if (string.IsNullOrWhiteSpace(role)) return false;

        var trimmed = role.Trim();
        var compact = trimmed.Replace(" ", "").Replace("_", "").Replace("-", "").ToLowerInvariant();

        var mapped = compact switch
        {
            "admin" or "administrator" => Admin,
            "systemadmin" or "systemadministrator" or "systemadminstorter" => SystemAdmin,
            "casemanager" => CaseManager,
            "incidentmanager" => IncidentManager,
            "citizen" or "affectedcitizen" => Citizen,
            "emergencycoordinator" or "coordinator" => EmergencyCoordinator,
            "fundingofficer" => FundingOfficer,
            "donor" => Donor,
            "partnerofficer" or "partner" or "partnerorganization" or "partnerorganisation"
                or "partnerorganisationofficer" or "partnerorganizationofficer" => PartnerOfficer,
            "resourcemanager" => ResourceManager,
            "logisticsmanager" => LogisticsManager,
            "shelteradministrator" => ShelterAdministrator,
            "shelteradmin" => ShelterAdministrator,
            "volunteercoordinator" => VolunteerCoordinator,
            "volunteer" => Volunteer,
            _ => AllRoles.FirstOrDefault(r =>
                    r.Equals(trimmed, StringComparison.OrdinalIgnoreCase) ||
                    r.Replace(" ", "").Equals(compact, StringComparison.OrdinalIgnoreCase)) ?? string.Empty
        };

        if (string.IsNullOrEmpty(mapped))
            return false;

        normalizedRole = mapped;
        return true;
    }

    public static string NormalizeRoleName(string? role)
    {
        return TryNormalizeRoleName(role, out var normalized) ? normalized : (role?.Trim() ?? string.Empty);
    }
}
