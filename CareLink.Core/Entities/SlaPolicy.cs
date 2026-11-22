namespace CareLink.Core.Entities;

public class SlaPolicy
{
    public string Level { get; set; } = string.Empty; // Critical, High, Medium, Low
    public string Time { get; set; } = string.Empty; // Immediate response, Within 30 minutes, etc.
    public int TargetMinutes { get; set; }
    public string Color { get; set; } = string.Empty; // e.g. "bg-red-100 text-red-700"
    public string Description { get; set; } = string.Empty;
}
