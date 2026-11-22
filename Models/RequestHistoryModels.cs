namespace CareLink.Backend.Models;

public class HistoryEventItem
{
    public string Id { get; set; } = string.Empty;
    public string CaseId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Actor { get; set; } = string.Empty;
    public string Change { get; set; } = string.Empty;
    public string Rationale { get; set; } = string.Empty;
    public string Timestamp { get; set; } = string.Empty;
    public string EventType { get; set; } = "Decision"; // Decision, Assignment, Verification, Escalation, Information, Creation, Status Change
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class RequestHistoryStatsDto
{
    public int TotalEvents { get; set; }
    public int DecisionsCount { get; set; }
    public int AssignmentsCount { get; set; }
    public int VerificationsCount { get; set; }
    public int EscalationsCount { get; set; }
}

public class RequestHistoryQueryDto
{
    public string? Search { get; set; }
    public string? EventType { get; set; } = "All event types";
    public string? Actor { get; set; } = "All actors";
    public string? CaseId { get; set; } = "All cases";
    public string? DateRange { get; set; } = "Date range";
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 5;
}

public class LogHistoryEventRequest
{
    public string CaseId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Actor { get; set; } = string.Empty;
    public string Change { get; set; } = string.Empty;
    public string Rationale { get; set; } = string.Empty;
    public string EventType { get; set; } = "Decision";
    public string? Details { get; set; }
}
