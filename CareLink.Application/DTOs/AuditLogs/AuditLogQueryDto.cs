using System;

namespace CareLink.Application.DTOs.AuditLogs;

public class AuditLogQueryDto
{
    public string? Search { get; set; }
    public string? Action { get; set; }
    public string? Actor { get; set; }
    public string? Type { get; set; }
    public string? RecordType { get => Type; set => Type = value; }
    public string? DateFilter { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int Page { get => PageNumber; set => PageNumber = value; }
    public int PageSize { get; set; } = 10;
}
