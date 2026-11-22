namespace CareLink.Core.Entities;

public class Shelter
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = "Central Ward"; // Central Ward, North Ward, Eastbank, West District
    public string District { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;      // Public School, Community Centre, etc.
    public string Manager { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";        // Active | Near Capacity | Full | Maintenance | Space available | Limited space
    public int Capacity { get; set; } = 500;
    public int CurrentOccupancy { get; set; }
    public int Occupied
    {
        get => CurrentOccupancy;
        set => CurrentOccupancy = value;
    }
    public int Reserved { get; set; }
    public int Remaining => Math.Max(0, Capacity - CurrentOccupancy);
    public int Availability
    {
        get => Remaining;
        set
        {
            if (value >= 0 && Capacity > 0)
            {
                CurrentOccupancy = Math.Max(0, Capacity - value);
            }
        }
    }
    public List<string> Services { get; set; } = new();
    public string ContactPerson
    {
        get => Manager;
        set => Manager = value;
    }
    public string ManagerName
    {
        get => Manager;
        set => Manager = value;
    }
    public string ContactPhone { get; set; } = string.Empty;
    public string? Notes { get; set; }

    // Citizen portal & Resource Manager extensions
    public string Area
    {
        get => Location;
        set => Location = value;
    }
    public string Address { get; set; } = string.Empty;
    public double Distance { get; set; } = 1.0;
    public bool Accessible { get; set; } = true;
    public string OperatingHours { get; set; } = "24/7 during crisis operations";
    public bool IsEvacuationHub { get; set; } = false;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class ShelterBooking
{
    public string Id { get; set; } = string.Empty;
    public string ShelterId { get; set; } = string.Empty;
    public string ShelterName { get; set; } = string.Empty;
    public string CitizenName { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public int NumberOfPeople { get; set; } = 1;
    public string? SpecialNeeds { get; set; }
    public string Status { get; set; } = "Confirmed";
    public DateTime BookedAt { get; set; } = DateTime.UtcNow;
}
