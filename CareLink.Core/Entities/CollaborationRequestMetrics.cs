namespace CareLink.Core.Entities;

public class CollaborationRequestMetrics
{
    public int OpenCollaborations { get; set; }
    public int AwaitingResponse { get; set; }
    public int ActivePartnerships { get; set; }
    public int CompletedThisMonth { get; set; }
    public int TotalCollaborations { get; set; }
    public int SectorsCount { get; set; }
    public int PartnersCount { get; set; }
    public int NeedsActionCount { get; set; }
    public int ActiveCount { get; set; }
    public int CompletedCount { get; set; }
    public int ArchivedCount { get; set; }
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
}
