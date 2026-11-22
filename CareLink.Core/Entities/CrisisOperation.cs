namespace CareLink.Core.Entities;

public class CrisisOperation
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Status { get; set; } = "Active Local Response";
    public string Region { get; set; } = "Greater Riverton";
    public string ActiveAlertTitle { get; set; } = string.Empty;
    public string ActiveAlertDescription { get; set; } = string.Empty;
    public string ActiveAlertSeverity { get; set; } = "Critical";
    public string Helpline { get; set; } = "112";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
