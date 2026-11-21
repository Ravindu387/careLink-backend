namespace CareLink.Core.Entities;

public class CoordinatorSettings
{
    public CoordinatorProfileSettings Profile { get; set; } = new();
    public CoordinatorOperationalSettings Operational { get; set; } = new();
    public CoordinatorNotificationSettings Notifications { get; set; } = new();
    public SecuritySettings Security { get; set; } = new();
    public List<UserSession> Sessions { get; set; } = new();
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class CoordinatorProfileSettings
{
    public string FullName { get; set; } = "Amina Mensah";
    public string Email { get; set; } = "amina@response.org";
    public string Phone { get; set; } = "+94 77 412 8041";
    public string Area { get; set; } = "Greater Riverton";
    public string Role { get; set; } = "Emergency Coordinator";
    public string Initials { get; set; } = "AM";
    public string BadgeNumber { get; set; } = "EC-8842";
}

public class CoordinatorOperationalSettings
{
    public bool AutoRefresh { get; set; } = true;
    public bool AiRecommendations { get; set; } = true;
    public bool CriticalAlerts { get; set; } = true;
    public bool LiveMap { get; set; } = true;
    public string DefaultPriority { get; set; } = "High";
    public string RefreshInterval { get; set; } = "30 seconds";
}

public class CoordinatorNotificationSettings
{
    public bool Critical { get; set; } = true;
    public bool Ai { get; set; } = true;
    public bool Resources { get; set; } = true;
    public bool Shelters { get; set; } = true;
    public bool Partners { get; set; } = true;
    public bool Reports { get; set; } = true;
    public bool Email { get; set; } = true;
    public bool Browser { get; set; } = true;
}

public class SecuritySettings
{
    public string PasswordHash { get; set; } = "Coordinator@123";
    public bool TwoFactorEnabled { get; set; } = true;
    public bool LoginAlerts { get; set; } = true;
    public string SessionTimeout { get; set; } = "30 minutes";
    public DateTimeOffset LastPasswordChangedAt { get; set; } = DateTimeOffset.UtcNow.AddDays(-14);
}

public class UserSession
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DeviceType { get; set; } = "desktop"; // "desktop" or "mobile"
    public string Time { get; set; } = "Active now";
    public string IpAddress { get; set; } = "127.0.0.1";
    public bool Current { get; set; }
    public DateTimeOffset LastActiveAt { get; set; } = DateTimeOffset.UtcNow;
}

public class PartnerSettings
{
    public PartnerProfileSettings Profile { get; set; } = new();
    public PartnerOrganisationSettings Organisation { get; set; } = new();
    public PartnerSharingSettings Sharing { get; set; } = new();
    public PartnerNotificationSettings Notifications { get; set; } = new();
    public SecuritySettings Security { get; set; } = new();
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class PartnerProfileSettings
{
    public string FullName { get; set; } = "Sara Khan";
    public string Email { get; set; } = "sara@globalrelief.org";
    public string Phone { get; set; } = "+94 77 412 8014";
    public string Role { get; set; } = "Partner Officer";
}

public class PartnerOrganisationSettings
{
    public string Name { get; set; } = "Global Relief Network";
    public string Registration { get; set; } = "ORG-027";
    public string Type { get; set; } = "International NGO";
    public string Location { get; set; } = "Central Ward, Greater Riverton";
    public string Timezone { get; set; } = "Asia/Colombo";
    public string Language { get; set; } = "English";
    public string Currency { get; set; } = "LKR";
}

public class PartnerSharingSettings
{
    public bool Referrals { get; set; } = true;
    public bool Resources { get; set; } = true;
    public bool Activities { get; set; } = true;
    public bool Reports { get; set; } = false;
    public bool Documents { get; set; } = true;
    public bool PublicProfile { get; set; } = true;
}

public class PartnerNotificationSettings
{
    public bool Urgent { get; set; } = true;
    public bool Referrals { get; set; } = true;
    public bool Collaboration { get; set; } = true;
    public bool Resources { get; set; } = true;
    public bool Reports { get; set; } = true;
    public bool Team { get; set; } = false;
    public bool Email { get; set; } = true;
    public bool Push { get; set; } = true;
}

public class PartnerSecuritySettings
{
    public bool TwoFactor { get; set; } = true;
    public bool LoginAlerts { get; set; } = true;
    public string SessionTimeout { get; set; } = "30 minutes";
}
