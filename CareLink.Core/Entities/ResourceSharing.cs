namespace CareLink.Core.Entities;

public class ResourceSharingCommitment
{
    public string Id { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Source { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string Partner { get; set; } = string.Empty;
    public string Scheduled { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending Approval";
    public string Type { get; set; } = "Food";
    public string Location { get; set; } = "North District";
    public string? Description { get; set; }
    public string? ContactPerson { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class ResourceSharingMetrics
{
    public int ActiveCommitments { get; set; } = 12;
    public string ActiveCommitmentsSubtitle { get; set; } = "5 in delivery";
    public string UnitsCommitted { get; set; } = "24.8K";
    public string UnitsCommittedSubtitle { get; set; } = "Across 8 categories";
    public string DeliveredThisWeek { get; set; } = "18.4K";
    public string DeliveredThisWeekSubtitle { get; set; } = "96% confirmed";
    public int Exceptions { get; set; } = 3;
    public string ExceptionsSubtitle { get; set; } = "Partial or delayed";

    public int TotalCommitments { get; set; }
    public int NeedsActionCount { get; set; }
    public int ActiveCount { get; set; }
    public int CompletedCount { get; set; }
    public int ArchivedCount { get; set; }
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
}

public class ResourceSharingQuery
{
    public string? Search { get; set; }
    public string? Tab { get; set; }
    public string? Type { get; set; }
    public string? Location { get; set; }
    public string? Partner { get; set; }
    public string? Status { get; set; }
}
