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
    public string Environment { get; set; } = "Production";
    public string? IpAddress { get; set; }
    public string? Metadata { get; set; }
}
