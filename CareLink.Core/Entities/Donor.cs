namespace CareLink.Core.Entities;

public class Donor
{
    public string Id { get; set; } = string.Empty; // e.g. DNR-2048
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "Foundation"; // Foundation | Corporate | International NGO | Community Group | Individual
    public string Contact { get; set; } = string.Empty;
    public string Scope { get; set; } = "National"; // International | National | Greater Riverton
    public string Verification { get; set; } = "Pending"; // Verified | Under Review | Pending
    public decimal Contributions { get; set; }
    public string Status { get; set; } = "Active"; // Active | Restricted | Inactive
    public string? Address { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
