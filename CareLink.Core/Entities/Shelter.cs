namespace CareLink.Core.Entities;

public class Shelter
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;      // Public School, Community Centre, etc.
    public string Manager { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";        // Active | Near Capacity | Full | Maintenance
    public int Capacity { get; set; }
    public int CurrentOccupancy { get; set; }
    public int Reserved { get; set; }
}
