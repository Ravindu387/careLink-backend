namespace CareLink.Application.DTOs;

public class AuditTimelineMetricsDto
{
    public int TotalEvents { get; set; }
    public int EventsToday { get; set; }
    public int AIDecisionEvents { get; set; }
    public int HumanOverrideEvents { get; set; }
    public int IntegrityScore { get; set; }
    public string RetentionPeriod { get; set; } = string.Empty;
    public bool AllRecordsVerified { get; set; }
}
