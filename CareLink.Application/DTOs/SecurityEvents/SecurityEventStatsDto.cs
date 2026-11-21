namespace CareLink.Application.DTOs.SecurityEvents;

public class SecurityEventStatsDto
{
    public int OpenEvents { get; set; }
    public string OpenEventsDetail { get; set; } = string.Empty;
    public int HighSeverityEvents { get; set; }
    public string HighSeverityDetail { get; set; } = string.Empty;
    public int BlockedAttempts { get; set; }
    public string BlockedAttemptsDetail { get; set; } = "Last 24 hours";
    public int ResolvedToday { get; set; }
    public string ResolvedTodayDetail { get; set; } = "Median 18 minutes";
    public string ThreatLevel { get; set; } = "Elevated";
    public string ThreatHeadline { get; set; } = string.Empty;
    public string MonitoringStatus { get; set; } = "Monitoring active";
    public int TotalEvents { get; set; }
}
