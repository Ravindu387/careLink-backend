namespace CareLink.Core.Entities;

public class Shelter
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = "Central Ward"; // Central Ward, North Ward, Eastbank, West District
    public string District { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;      // Public School, Community Centre, etc.
    public string Manager { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";        // Active | Near Capacity | Full | Maintenance
    public int Capacity { get; set; } = 500;
    public int CurrentOccupancy { get; set; }
    public int Occupied
    {
        get => CurrentOccupancy;
        set => CurrentOccupancy = value;
    }
    public int Reserved { get; set; }
    public int Remaining => Math.Max(0, Capacity - CurrentOccupancy);
    public List<string> Services { get; set; } = new();
    public string ContactPerson
    {
        get => Manager;
        set => Manager = value;
    }
    public string ContactPhone { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
