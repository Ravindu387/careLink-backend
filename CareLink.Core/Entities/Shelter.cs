namespace CareLink.Core.Entities;

public class Shelter
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = "Central Ward"; // Central Ward, North Ward, Eastbank, West District
    public int Capacity { get; set; } = 500;
    public int Occupied { get; set; }
    public int Remaining => Math.Max(0, Capacity - Occupied);
    public List<string> Services { get; set; } = new();
    public string Status { get; set; } = "Operational"; // Operational, Near Capacity, Full
    public string ContactPerson { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
