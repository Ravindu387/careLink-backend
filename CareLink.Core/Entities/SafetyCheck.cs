namespace CareLink.Core.Entities;

public class SafetyCheck
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Color { get; set; } = "green"; // green, orange, blue, red
    public string IconKey { get; set; } = "Check"; // Check, AlertTriangle, ShieldCheck, Accessibility
    public string Category { get; set; } = "General";
    public DateTime? LastCheckedAt { get; set; }
}
