namespace CareLink.Core.Entities;

public class Supplier
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Coverage { get; set; } = string.Empty;
    public string OnTimeRate { get; set; } = "95%";
    public string LeadTime { get; set; } = "2.0 days";
    public string Verification { get; set; } = "Pending";
    public string Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
