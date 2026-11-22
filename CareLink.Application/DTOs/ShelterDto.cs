namespace CareLink.Application.DTOs;

public class ShelterDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Manager { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public int Capacity { get; set; }
    public int CurrentOccupancy { get; set; }
    public int Occupied { get => CurrentOccupancy; set => CurrentOccupancy = value; }
    public int Reserved { get; set; }
    public int Remaining
    {
        get => Math.Max(0, Capacity - CurrentOccupancy);
        set { }
    }
    public int Available => Capacity - CurrentOccupancy;
    public int Rate => Capacity > 0 ? (int)Math.Round((double)CurrentOccupancy / Capacity * 100) : 0;
    public List<string> Services { get; set; } = new();
    public string ContactPerson { get => Manager; set => Manager = value; }
    public string ContactPhone { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
