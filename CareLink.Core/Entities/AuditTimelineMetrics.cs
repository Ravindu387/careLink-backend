namespace CareLink.Core.Entities;

public class AuditTimelineMetrics
{
    public int TotalEvents { get; set; }
    public int EventsToday { get; set; }
    public int AIDecisionEvents { get; set; }
    public int HumanOverrideEvents { get; set; }
    public int IntegrityScore { get; set; } = 100;
    public string RetentionPeriod { get; set; } = "7 Years";
    public bool AllRecordsVerified { get; set; } = true;
}
