namespace CareLink.Core.Entities;

public class PartnerOrganisationsMetrics
{
    public int ActivePartnersCount { get; set; }
    public int TotalPartnersCount { get; set; }
    public int SharedResourcesCount { get; set; }
    public int CollaborationRequestsCount { get; set; }
    public int PendingCollaborationRequestsCount { get; set; }
    public int VerificationCoveragePercent { get; set; }
    public int PendingVerificationReviewsCount { get; set; }
    public int SectorsCount { get; set; } = 6;
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
}
