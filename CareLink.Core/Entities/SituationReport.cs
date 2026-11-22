namespace CareLink.Core.Entities;

public class SituationReport
{
    public string Id { get; set; } = string.Empty; // e.g. "SITREP 024"
    public string Title { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;
    public string Status { get; set; } = "Draft"; // Approved, Published, Draft
    public string PreparedBy { get; set; } = "Operations Analysis Team";
    public string ApprovedBy { get; set; } = "Pending approval";
    public int Recipients { get; set; } = 0;
    public string Event { get; set; } = "Monsoon Floods 2025";
    public string? ExecutiveSummary { get; set; }
    public string? KeyHighlights { get; set; }
    public string? OperationalChallenges { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ApprovedAt { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
}

public class SituationReportMetrics
{
    public int PublishedCount { get; set; }
    public int DraftCount { get; set; }
    public int TotalRecipients { get; set; }
    public string NextReportDue { get; set; } = "2h 18m";
    public string NextReportId { get; set; } = "SITREP 025";
    public SituationReport? LatestApproved { get; set; }
    public int TotalReports { get; set; }
}
