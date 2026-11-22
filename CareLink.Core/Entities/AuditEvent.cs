namespace CareLink.Core.Entities;

public class AuditEvent
{
    public string Id { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Actor { get; set; } = string.Empty;
    public string Record { get; set; } = string.Empty;
    public string Previous { get; set; } = string.Empty;
    public string Current { get; set; } = string.Empty;
    public string Rationale { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? IpAddress { get; set; } = "127.0.0.1";
    public string Checksum { get; set; } = string.Empty;
    public bool IsTamperProofVerified { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
