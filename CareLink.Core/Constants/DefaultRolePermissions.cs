namespace CareLink.Core.Constants;

public static class DefaultRolePermissions
{
    private static readonly IReadOnlyList<string> CaseManagerPermissions =
    [
        Permissions.Crises.View, Permissions.Crises.Create, Permissions.Crises.Update, Permissions.Crises.Manage,
        Permissions.Requests.View, Permissions.Requests.Update, Permissions.Requests.Verify, Permissions.Requests.Assign, Permissions.Requests.Resolve,
        Permissions.Reports.View,
        Permissions.Notifications.View
    ];

    private static readonly IReadOnlyList<string> ShelterAdminPermissions =
    [
        Permissions.Shelters.View, Permissions.Shelters.Create, Permissions.Shelters.Update, Permissions.Shelters.ManageOccupancy,
        Permissions.Requests.View,
        Permissions.Resources.View,
        Permissions.Reports.View,
        Permissions.Notifications.View
    ];

    private static readonly IReadOnlyList<string> ResourceManagerPermissions =
    [
        Permissions.Resources.View, Permissions.Resources.Create, Permissions.Resources.Update, Permissions.Resources.Delete, Permissions.Resources.Allocate,
        Permissions.Requests.View,
        Permissions.Reports.View,
        Permissions.Notifications.View
    ];

    private static readonly IReadOnlyList<string> FundingOfficerPermissions =
    [
        Permissions.Crises.View,
        Permissions.Donations.View, Permissions.Donations.Manage,
        Permissions.Reports.View, Permissions.Reports.Export,
        Permissions.Notifications.View
    ];

    public static readonly Dictionary<string, IReadOnlyList<string>> Map = new(StringComparer.OrdinalIgnoreCase)
    {
        [UserRoles.Admin] = Permissions.All,
        [UserRoles.SystemAdmin] = Permissions.All,

        [UserRoles.EmergencyCoordinator] =
        [
            Permissions.Crises.View, Permissions.Crises.Create, Permissions.Crises.Update, Permissions.Crises.Manage,
            Permissions.Requests.View, Permissions.Requests.Update, Permissions.Requests.Verify, Permissions.Requests.Assign, Permissions.Requests.Resolve,
            Permissions.AIRecommendations.View, Permissions.AIRecommendations.Approve, Permissions.AIRecommendations.Modify, Permissions.AIRecommendations.Defer,
            Permissions.Resources.View, Permissions.Resources.Allocate,
            Permissions.Shelters.View, Permissions.Shelters.ManageOccupancy,
            Permissions.Volunteers.View, Permissions.Volunteers.Assign,
            Permissions.Reports.View, Permissions.Reports.Export,
            Permissions.Audit.View,
            Permissions.Notifications.View, Permissions.Notifications.Manage
        ],

        [UserRoles.PartnerOfficer] =
        [
            Permissions.Crises.View,
            Permissions.Requests.View, Permissions.Requests.Update,
            Permissions.Resources.View, Permissions.Resources.Create, Permissions.Resources.Update,
            Permissions.Donations.View, Permissions.Donations.Manage,
            Permissions.Reports.View,
            Permissions.Notifications.View
        ],

        [UserRoles.Volunteer] =
        [
            Permissions.Crises.View,
            Permissions.Requests.View,
            Permissions.Volunteers.View,
            Permissions.Shelters.View,
            Permissions.Notifications.View
        ],

        [UserRoles.Citizen] =
        [
            Permissions.Crises.View,
            Permissions.Requests.View, Permissions.Requests.Create,
            Permissions.Shelters.View,
            Permissions.Notifications.View
        ],

        [UserRoles.CaseManager] = CaseManagerPermissions,
        [UserRoles.IncidentManager] = CaseManagerPermissions,

        [UserRoles.ShelterAdministrator] = ShelterAdminPermissions,
        [UserRoles.ShelterAdmin] = ShelterAdminPermissions,

        [UserRoles.VolunteerCoordinator] =
        [
            Permissions.Volunteers.View, Permissions.Volunteers.Manage, Permissions.Volunteers.Assign,
            Permissions.Requests.View,
            Permissions.Reports.View,
            Permissions.Notifications.View
        ],

        [UserRoles.ResourceManager] = ResourceManagerPermissions,
        [UserRoles.LogisticsManager] = ResourceManagerPermissions,

        [UserRoles.FundingOfficer] = FundingOfficerPermissions,
        [UserRoles.Donor] = FundingOfficerPermissions
    };

    public static IReadOnlyList<string> GetDefaultPermissions(string role)
    {
        if (Map.TryGetValue(role, out var permissions))
        {
            return permissions;
        }
        return Array.Empty<string>();
    }
}
