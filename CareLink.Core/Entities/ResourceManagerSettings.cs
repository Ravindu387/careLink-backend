namespace CareLink.Core.Entities;

public class ProfileSettings
{
    public string FullName { get; set; } = "Daniel Perera";
    public string Email { get; set; } = "daniel@logistics.gov";
    public string Phone { get; set; } = "+94 77 412 8088";
    public string Warehouse { get; set; } = "Central Logistics Hub";
}

public class InventoryRulesSettings
{
    public string LowStock { get; set; } = "20";
    public string CriticalStock { get; set; } = "5";
    public string ExpiryDays { get; set; } = "30";
    public bool AutoReorder { get; set; } = true;
    public bool TrackExpiry { get; set; } = true;
    public bool AllowNegative { get; set; } = false;
}

public class DispatchPreferencesSettings
{
    public string DefaultVehicle { get; set; } = "Truck";
    public string Approval { get; set; } = "Manager approval required";
    public bool Proof { get; set; } = true;
    public bool Tracking { get; set; } = true;
    public bool DelayAlert { get; set; } = true;
}

public class NotificationPreferencesSettings
{
    public bool Email { get; set; } = true;
    public bool System { get; set; } = true;
    public bool Critical { get; set; } = true;
    public bool LowStock { get; set; } = true;
    public bool Dispatch { get; set; } = false;
    public bool Weekly { get; set; } = true;
}

public class SecuritySettings
{
    public bool TwoFactor { get; set; } = true;
    public string SessionTimeout { get; set; } = "30 minutes";
    public bool LoginAlerts { get; set; } = true;
}

public class ResourceManagerSettings
{
    public ProfileSettings Profile { get; set; } = new();
    public InventoryRulesSettings Inventory { get; set; } = new();
    public DispatchPreferencesSettings Dispatch { get; set; } = new();
    public NotificationPreferencesSettings Notifications { get; set; } = new();
    public SecuritySettings Security { get; set; } = new();
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
