using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs.AuditLogs;

public class ExportAuditLogRequestDto
{
    public string ActionTitle { get; set; } = "Audit Export";

    public string Environment { get; set; } = "Production";

    public string Scope { get; set; } = "Current filtered results"; // "Selected record", "Current filtered results", "All available records"

    public string ApprovalType { get; set; } = "Standard administrator approval";

    [Required]
    public string Rationale { get; set; } = string.Empty;

    public string? SelectedRecordId { get; set; }

    // Optional query filters to scope the export
    public string? Search { get; set; }
    public string? Action { get; set; }
    public string? Actor { get; set; }
    public string? Type { get; set; }
    public string? DateFilter { get; set; }
}
