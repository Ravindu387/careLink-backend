namespace CareLink.Application.DTOs;

public class AuditLogDto
{
    public string Id { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Actor { get; set; } = string.Empty;
    public string Timestamp { get; set; } = string.Empty;
    public string Record { get; set; } = string.Empty;
    public string Change { get; set; } = string.Empty;
    public string Rationale { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string RecordType { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateAuditLogRequest
{
    public string Action { get; set; } = string.Empty;
    public string Actor { get; set; } = "Daniel Perera";
    public string Record { get; set; } = string.Empty;
    public string Change { get; set; } = string.Empty;
    public string Rationale { get; set; } = string.Empty;
    public string ActionType { get; set; } = "Verification";
    public string RecordType { get; set; } = "System";
    public string? Date { get; set; }
}

public class AuditLogFilterParams
{
    public string? Search { get; set; }
    public string? ActionType { get; set; }
    public string? Actor { get; set; }
    public string? RecordType { get; set; }
    public string? DateFilter { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
}

public class AuditLogSummaryDto
{
    public int TotalLogs { get; set; }
    public int ApprovalsCount { get; set; }
    public int DispatchesCount { get; set; }
    public int InventoryCount { get; set; }
    public int TodayCount { get; set; }
}
