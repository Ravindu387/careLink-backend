namespace CareLink.Application.DTOs;

public class ProfileSettingsDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Warehouse { get; set; } = string.Empty;
}

public class InventoryRulesSettingsDto
{
    public string LowStock { get; set; } = "20";
    public string CriticalStock { get; set; } = "5";
    public string ExpiryDays { get; set; } = "30";
    public bool AutoReorder { get; set; } = true;
    public bool TrackExpiry { get; set; } = true;
    public bool AllowNegative { get; set; } = false;
}

public class DispatchPreferencesSettingsDto
{
    public string DefaultVehicle { get; set; } = "Truck";
    public string Approval { get; set; } = "Manager approval required";
    public bool Proof { get; set; } = true;
    public bool Tracking { get; set; } = true;
    public bool DelayAlert { get; set; } = true;
}

public class NotificationPreferencesSettingsDto
{
    public bool Email { get; set; } = true;
    public bool System { get; set; } = true;
    public bool Critical { get; set; } = true;
    public bool LowStock { get; set; } = true;
    public bool Dispatch { get; set; } = false;
    public bool Weekly { get; set; } = true;
}

public class SecuritySettingsDto
{
    public bool TwoFactor { get; set; } = true;
    public string SessionTimeout { get; set; } = "30 minutes";
    public bool LoginAlerts { get; set; } = true;
}

public class ResourceManagerSettingsDto
{
    public ProfileSettingsDto Profile { get; set; } = new();
    public InventoryRulesSettingsDto Inventory { get; set; } = new();
    public DispatchPreferencesSettingsDto Dispatch { get; set; } = new();
    public NotificationPreferencesSettingsDto Notifications { get; set; } = new();
    public SecuritySettingsDto Security { get; set; } = new();
    public DateTime UpdatedAt { get; set; }
}

public class UpdateSettingsRequest
{
    public ProfileSettingsDto? Profile { get; set; }
    public InventoryRulesSettingsDto? Inventory { get; set; }
    public DispatchPreferencesSettingsDto? Dispatch { get; set; }
    public NotificationPreferencesSettingsDto? Notifications { get; set; }
    public SecuritySettingsDto? Security { get; set; }
}
