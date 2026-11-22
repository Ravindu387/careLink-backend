namespace CareLink.Core.Entities;

public class PartnerDocumentMetrics
{
    public int ActiveRecords { get; set; }
    public int NeedsReview { get; set; }
    public int VerifiedPercentage { get; set; } = 96;
    public int DueSoon { get; set; }
    public int TotalDocuments { get; set; }
    public int NeedsActionCount { get; set; }
    public int ActiveCount { get; set; }
    public int CompletedCount { get; set; }
    public int ArchivedCount { get; set; }
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
}
