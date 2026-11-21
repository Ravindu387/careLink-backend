using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class SituationReportDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;
    public string Status { get; set; } = "Draft"; // Approved, Published, Draft
    public string PreparedBy { get; set; } = string.Empty;
    public string ApprovedBy { get; set; } = string.Empty;
    public int Recipients { get; set; }
    public string Event { get; set; } = string.Empty;
    public string? ExecutiveSummary { get; set; }
    public string? KeyHighlights { get; set; }
    public string? OperationalChallenges { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
}

public class SituationReportMetricsDto
{
    public int PublishedCount { get; set; }
    public int DraftCount { get; set; }
    public int TotalRecipients { get; set; }
    public string NextReportDue { get; set; } = "2h 18m";
    public string NextReportId { get; set; } = "SITREP 025";
    public SituationReportDto? LatestApproved { get; set; }
    public int TotalReports { get; set; }
}

public class CreateSituationReportDto
{
    [Required]
    [StringLength(180, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string Event { get; set; } = "Monsoon Floods 2025";

    public string? Period { get; set; }

    public string? PreparedBy { get; set; } = "Operations Analysis Team";

    public string? ExecutiveSummary { get; set; }

    public string? KeyHighlights { get; set; }

    public string? OperationalChallenges { get; set; }
}

public class UpdateReportStatusDto
{
    [Required]
    [RegularExpression("^(Approved|Published|Draft)$", ErrorMessage = "Status must be Approved, Published, or Draft.")]
    public string Status { get; set; } = "Approved";

    public string? ApprovedBy { get; set; }

    public int? Recipients { get; set; }
}

public class SituationReportQueryParameters
{
    public string? Search { get; set; }
    public string? Event { get; set; }
    public string? Status { get; set; }
    public string? DateRange { get; set; }
}
