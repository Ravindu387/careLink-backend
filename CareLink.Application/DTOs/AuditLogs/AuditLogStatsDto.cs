namespace CareLink.Application.DTOs.AuditLogs;

public class AuditLogStatsDto
{
    public int TotalRecords { get; set; }
    public string RetentionPeriod { get; set; } = "7 years";
    public int UniqueActors { get; set; }
    public int ActionsToday { get; set; }
    public IReadOnlyList<string> ActionTypes { get; set; } = [];
    public IReadOnlyList<string> RecordTypes { get; set; } = [];
    public IReadOnlyList<string> Actors { get; set; } = [];
}
