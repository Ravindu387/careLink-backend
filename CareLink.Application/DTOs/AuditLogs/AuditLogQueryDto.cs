namespace CareLink.Application.DTOs.AuditLogs;

public class AuditLogQueryDto
{
    public string? Search { get; set; }
    public string? Action { get; set; }
    public string? Actor { get; set; }
    public string? Type { get; set; }
    public string? DateFilter { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
