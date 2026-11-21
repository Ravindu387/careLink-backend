namespace CareLink.Core.Entities;

public class Shelter
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double Distance { get; set; } = 1.0;
    public int Capacity { get; set; } = 250;
    public int Availability { get; set; } = 50;
    public int Occupied { get; set; } = 200;
    public List<string> Services { get; set; } = new();
    public string Status { get; set; } = "Space available"; // "Space available", "Limited space", "Full"
    public bool Accessible { get; set; } = true;
    public string ContactPhone { get; set; } = "+94 11 234 5678";
    public string ManagerName { get; set; } = "Site Coordinator";
    public string OperatingHours { get; set; } = "24/7 during crisis operations";
    public bool IsEvacuationHub { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
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
