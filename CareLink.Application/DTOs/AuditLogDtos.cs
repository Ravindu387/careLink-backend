using CareLink.Application.DTOs.AuditLogs;

namespace CareLink.Application.DTOs;

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
    public string? Date { get => DateFilter; set => DateFilter = value; }
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
