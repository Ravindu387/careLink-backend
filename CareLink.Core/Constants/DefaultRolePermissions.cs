namespace CareLink.Core.Constants;

public static class DefaultRolePermissions
{
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
            Permissions.Reports.View
        ],

        [UserRoles.Volunteer] =
        [
            Permissions.Crises.View,
            Permissions.Requests.View,
            Permissions.Volunteers.View,
            Permissions.Shelters.View
        ],

        [UserRoles.Citizen] =
        [
            Permissions.Crises.View,
            Permissions.Requests.View, Permissions.Requests.Create,
            Permissions.Shelters.View
        ],

        [UserRoles.IncidentManager] =
        [
            Permissions.Crises.View, Permissions.Crises.Create, Permissions.Crises.Update, Permissions.Crises.Manage,
            Permissions.Requests.View, Permissions.Requests.Verify,
            Permissions.Reports.View
        ],

        [UserRoles.ShelterAdmin] =
        [
            Permissions.Shelters.View, Permissions.Shelters.Create, Permissions.Shelters.Update, Permissions.Shelters.ManageOccupancy,
            Permissions.Requests.View,
            Permissions.Resources.View
        ],

        [UserRoles.VolunteerCoordinator] =
        [
            Permissions.Volunteers.View, Permissions.Volunteers.Manage, Permissions.Volunteers.Assign,
            Permissions.Requests.View,
            Permissions.Reports.View
        ],

        [UserRoles.LogisticsManager] =
        [
            Permissions.Resources.View, Permissions.Resources.Create, Permissions.Resources.Update, Permissions.Resources.Delete, Permissions.Resources.Allocate,
            Permissions.Requests.View
        ],

        [UserRoles.Donor] =
        [
            Permissions.Crises.View,
            Permissions.Donations.View, Permissions.Donations.Manage
        ]
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
