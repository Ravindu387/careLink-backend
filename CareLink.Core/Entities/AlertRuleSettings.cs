namespace CareLink.Core.Entities;

public class AlertRuleSettings
{
    public bool Critical { get; set; } = true;
    public bool LowStock { get; set; } = true;
    public bool Capacity { get; set; } = true;
    public bool Dispatch { get; set; } = true;
    public bool Expiry { get; set; } = true;
}
