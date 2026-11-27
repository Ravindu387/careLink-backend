using System;

namespace CareLink.Application.DTOs.AuditLogs;

public class AuditLogDto
{
    public string Id { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Actor { get; set; } = string.Empty;
    public string? ActorId { get; set; }
    public string Timestamp { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Record { get; set; } = string.Empty;
    public string Change { get; set; } = string.Empty;
    public string Rationale { get; set; } = string.Empty;
    public string Type { get; set; } = "System";

    private string? _actionType;
    public string ActionType
    {
        get => _actionType ?? Action;
        set => _actionType = value;
    }

    private string? _recordType;
    public string RecordType
    {
        get => _recordType ?? Type;
        set => _recordType = value;
    }

    private string? _date;
    public string Date
    {
        get => _date ?? CreatedAt.ToString("yyyy-MM-dd");
        set => _date = value;
    }

    public string Environment { get; set; } = "Production";
    public string? IpAddress { get; set; }
    public string? Metadata { get; set; }
}
