using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class CreateAuditEventDto
{
    [Required(ErrorMessage = "Action is required.")]
    public string Action { get; set; } = string.Empty;

    [Required(ErrorMessage = "Actor is required.")]
    public string Actor { get; set; } = string.Empty;

    [Required(ErrorMessage = "Associated Record ID is required.")]
    public string Record { get; set; } = string.Empty;

    public string Previous { get; set; } = "-";

    [Required(ErrorMessage = "Current value is required.")]
    public string Current { get; set; } = string.Empty;

    [Required(ErrorMessage = "Auditable rationale is mandatory.")]
    public string Rationale { get; set; } = string.Empty;

    [Required(ErrorMessage = "Record Type is required.")]
    public string Type { get; set; } = "AI Decision";

    public string? IpAddress { get; set; }
}

public class AuditQueryParameters
{
    public string? Search { get; set; }
    public string? Action { get; set; }
    public string? ActionType { get; set; }
    public string? Actor { get; set; }
    public string? User { get; set; }
    public string? Type { get; set; }
    public string? RecordType { get; set; }
    public string? DateRange { get; set; }
    public string? DateFrom { get; set; }
    public string? DateTo { get; set; }
}
