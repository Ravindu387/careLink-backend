namespace CareLink.Core.Entities;

public class OrganisationVerificationItem
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Status { get; set; } = "Current"; // Current, Due Soon, Expired
    public bool Done { get; set; } = true;
    public DateTimeOffset? DueDate { get; set; }
}

public class CapabilityMetric
{
    public string Title { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public int TotalCount { get; set; }
    public int ActiveCount { get; set; }
}

public class OrganisationCapabilities
{
    public CapabilityMetric Personnel { get; set; } = new()
    {
        Title = "Personnel",
        Value = "84 active staff",
        Subtitle = "62 deployable today",
        TotalCount = 84,
        ActiveCount = 62
    };

    public CapabilityMetric Fleet { get; set; } = new()
    {
        Title = "Fleet",
        Value = "18 vehicles",
        Subtitle = "14 available",
        TotalCount = 18,
        ActiveCount = 14
    };

    public CapabilityMetric Stock { get; set; } = new()
    {
        Title = "Stock",
        Value = "8 resource categories",
        Subtitle = "68% available",
        TotalCount = 8,
        ActiveCount = 68
    };

    public CapabilityMetric Coverage { get; set; } = new()
    {
        Title = "Coverage",
        Value = "6 districts",
        Subtitle = "4 active now",
        TotalCount = 6,
        ActiveCount = 4
    };
}

public class OrganisationProfile
{
    public string Id { get; set; } = "ORG-027";
    public string Name { get; set; } = "Global Relief Network";
    public string LegalName { get; set; } = "Global Relief Network International";
    public string RegistrationNumber { get; set; } = "NGO-2011-0482";
    public string Type { get; set; } = "International NGO";
    public string RegisteredDate { get; set; } = "18 March 2011";
    public string Status { get; set; } = "Active";
    public string Verification { get; set; } = "Verified";
    public string HeadOffice { get; set; } = "Central Ward, Greater Riverton";
    public string PrimaryContact { get; set; } = "Sara Khan - Partner Officer";
    public string ContactEmail { get; set; } = "sara@globalrelief.org";
    public string EmergencyLine { get; set; } = "+94 11 555 0142";
    public string Website { get; set; } = "https://globalrelief.org";
    public int ProfileCompleteness { get; set; } = 94;
    public int CompletenessReviewItems { get; set; } = 2;
    public List<string> Sectors { get; set; } = new()
    {
        "Food Security",
        "WASH",
        "Community Health",
        "Emergency Logistics"
    };

    public List<OrganisationVerificationItem> Verifications { get; set; } = new()
    {
        new()
        {
            Title = "Organisation verification",
            Subtitle = "Verified",
            Status = "Current",
            Done = true
        },
        new()
        {
            Title = "Data sharing agreement",
            Subtitle = "Active · expires 18 Mar 2026",
            Status = "Current",
            Done = true
        },
        new()
        {
            Title = "Safeguarding attestation",
            Subtitle = "Review due in 18 days",
            Status = "Due Soon",
            Done = false
        },
        new()
        {
            Title = "Banking verification",
            Subtitle = "Verified",
            Status = "Current",
            Done = true
        }
    };

    public OrganisationCapabilities Capabilities { get; set; } = new();

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
