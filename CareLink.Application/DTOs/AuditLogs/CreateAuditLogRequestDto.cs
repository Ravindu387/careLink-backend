using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs.AuditLogs;

public class CreateAuditLogRequestDto
{
    [Required]
    public string Action { get; set; } = string.Empty;

    [Required]
    public string Actor { get; set; } = string.Empty;

    public string? ActorId { get; set; }

    [Required]
    public string Record { get; set; } = string.Empty;

    [Required]
    public string Change { get; set; } = string.Empty;

    [Required]
    public string Rationale { get; set; } = string.Empty;

    [Required]
    public string Type { get; set; } = "System";

    public string Environment { get; set; } = "Production";

    public string? IpAddress { get; set; }

    public string? Metadata { get; set; }
}
