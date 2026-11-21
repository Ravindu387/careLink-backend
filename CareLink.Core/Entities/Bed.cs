namespace CareLink.Core.Entities;

public class Bed
{
    public string Id { get; set; } = string.Empty; // e.g. "RM-001-B01"
    public string BedNumber { get; set; } = string.Empty; // e.g. "B-01"
    public string RoomId { get; set; } = string.Empty;
    public string Status { get; set; } = "Available"; // "Available" | "Occupied" | "Reserved" | "Unavailable"
    public string? ResidentName { get; set; }
    public string? ResidentId { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
