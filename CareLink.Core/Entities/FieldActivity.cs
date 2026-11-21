namespace CareLink.Core.Entities;

public class FieldActivity
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Scheduled { get; set; } = string.Empty;
    public string Team { get; set; } = "Field Team North";
    public string Target { get; set; } = string.Empty;
    public string Sector { get; set; } = "Food Security";
    public string Status { get; set; } = "Scheduled";
    public string? Description { get; set; }
    public int? BeneficiariesReached { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CompletedAt { get; set; }
}
