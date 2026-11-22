namespace CareLink.Application.DTOs;

public class AuditEventResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Actor { get; set; } = string.Empty;
    public string Timestamp { get; set; } = string.Empty;
    public string Record { get; set; } = string.Empty;
    public string Previous { get; set; } = string.Empty;
    public string Current { get; set; } = string.Empty;
    public string Change { get; set; } = string.Empty;
    public string Rationale { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string RecordType { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string Checksum { get; set; } = string.Empty;
    public bool IsVerified { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
}
