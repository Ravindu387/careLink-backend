namespace CareLink.Core.Constants;

public static class Permissions
{
    public static class Users
    {
        public const string View = "Users.View";
        public const string Create = "Users.Create";
        public const string Update = "Users.Update";
        public const string Delete = "Users.Delete";
        public const string AssignRoles = "Users.AssignRoles";
        public const string ManagePermissions = "Users.ManagePermissions";
    }

    public static class Crises
    {
        public const string View = "Crises.View";
        public const string Create = "Crises.Create";
        public const string Update = "Crises.Update";
        public const string Delete = "Crises.Delete";
        public const string Manage = "Crises.Manage";
    }

    public static class Requests
    {
        public const string View = "Requests.View";
        public const string Create = "Requests.Create";
        public const string Update = "Requests.Update";
        public const string Verify = "Requests.Verify";
        public const string Assign = "Requests.Assign";
        public const string Resolve = "Requests.Resolve";
    }

    public static class AIRecommendations
    {
        public const string View = "AIRecommendations.View";
        public const string Approve = "AIRecommendations.Approve";
        public const string Modify = "AIRecommendations.Modify";
        public const string Defer = "AIRecommendations.Defer";
    }

    public static class Resources
    {
        public const string View = "Resources.View";
        public const string Create = "Resources.Create";
        public const string Update = "Resources.Update";
        public const string Delete = "Resources.Delete";
        public const string Allocate = "Resources.Allocate";
    }

    public static class Shelters
    {
        public const string View = "Shelters.View";
        public const string Create = "Shelters.Create";
        public const string Update = "Shelters.Update";
        public const string ManageOccupancy = "Shelters.ManageOccupancy";
    }

    public static class Volunteers
    {
        public const string View = "Volunteers.View";
        public const string Manage = "Volunteers.Manage";
        public const string Assign = "Volunteers.Assign";
    }

    public static class Donations
    {
        public const string View = "Donations.View";
        public const string Manage = "Donations.Manage";
        public const string Verify = "Donations.Verify";
    }

    public static class Reports
    {
        public const string View = "Reports.View";
        public const string Export = "Reports.Export";
    }

    public static class Audit
    {
        public const string View = "Audit.View";
    }

    public static class Notifications
    {
        public const string View = "Notifications.View";
        public const string Manage = "Notifications.Manage";
    }

    public static readonly IReadOnlyList<string> All =
    [
        Users.View, Users.Create, Users.Update, Users.Delete, Users.AssignRoles, Users.ManagePermissions,
        Crises.View, Crises.Create, Crises.Update, Crises.Delete, Crises.Manage,
        Requests.View, Requests.Create, Requests.Update, Requests.Verify, Requests.Assign, Requests.Resolve,
        AIRecommendations.View, AIRecommendations.Approve, AIRecommendations.Modify, AIRecommendations.Defer,
        Resources.View, Resources.Create, Resources.Update, Resources.Delete, Resources.Allocate,
        Shelters.View, Shelters.Create, Shelters.Update, Shelters.ManageOccupancy,
        Volunteers.View, Volunteers.Manage, Volunteers.Assign,
        Donations.View, Donations.Manage, Donations.Verify,
        Reports.View, Reports.Export,
        Audit.View,
        Notifications.View, Notifications.Manage
    ];

    public static bool IsValid(string permission) =>
        All.Contains(permission, StringComparer.OrdinalIgnoreCase);
}
