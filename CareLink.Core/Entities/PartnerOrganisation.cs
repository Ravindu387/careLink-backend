namespace CareLink.Core.Entities;

public class PartnerOrganisation
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "Local NGO"; // Government, Local NGO, International NGO, Private Organisation, Community Organisation
    public string Region { get; set; } = "Greater Riverton";
    public string Role { get; set; } = "Shelter Operations"; // Lead Coordinator, Shelter Operations, Food - Medical, Transport - Warehousing, Emergency Assistance
    public string Verification { get; set; } = "Pending"; // Verified, Pending, Rejected
    public string Status { get; set; } = "Active"; // Active, Inactive
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public int SharedResourcesCount { get; set; }
    public int CollaborationRequestsCount { get; set; }
    public string? Address { get; set; }
    public string? Website { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? VerifiedAt { get; set; }
}
