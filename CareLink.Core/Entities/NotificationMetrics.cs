namespace CareLink.Core.Entities;

public class NotificationMetrics
{
    public int All { get; set; }
    public int Unread { get; set; }
    public int Urgent { get; set; }
    public int Critical { get; set; }
    public int Ai { get; set; }
    public int Resource { get; set; }
    public int Shelter { get; set; }
    public int Partner { get; set; }
    public int Report { get; set; }
    public int Collaboration { get; set; }
    public int Reporting { get; set; }
    public int Team { get; set; }
    public int Agreement { get; set; }
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
}
