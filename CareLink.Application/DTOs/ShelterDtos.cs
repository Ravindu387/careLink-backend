namespace CareLink.Application.DTOs;

public class ShelterDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double Distance { get; set; }
    public int Capacity { get; set; }
    public int Availability { get; set; }
    public int Occupied { get; set; }
    public List<string> Services { get; set; } = new();
    public string Status { get; set; } = "Space available";
    public bool Accessible { get; set; }
    public string ContactPhone { get; set; } = string.Empty;
    public string ManagerName { get; set; } = string.Empty;
    public string OperatingHours { get; set; } = string.Empty;
    public bool IsEvacuationHub { get; set; }
    public int OccupancyPercentage => Capacity > 0 ? (int)Math.Round((double)Occupied / Capacity * 100) : 100;
}

public class CreateShelterRequest
{
    public string Name { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double Distance { get; set; } = 1.0;
    public int Capacity { get; set; } = 200;
    public int Availability { get; set; } = 50;
    public List<string> Services { get; set; } = new();
    public bool Accessible { get; set; } = true;
    public string? ContactPhone { get; set; }
    public string? ManagerName { get; set; }
    public string? OperatingHours { get; set; }
    public bool IsEvacuationHub { get; set; }
}

public class UpdateShelterRequest
{
    public string? Name { get; set; }
    public string? Area { get; set; }
    public string? Address { get; set; }
    public double? Distance { get; set; }
    public int? Capacity { get; set; }
    public int? Availability { get; set; }
    public List<string>? Services { get; set; }
    public bool? Accessible { get; set; }
    public string? Status { get; set; }
    public string? ContactPhone { get; set; }
    public string? ManagerName { get; set; }
    public string? OperatingHours { get; set; }
    public bool? IsEvacuationHub { get; set; }
}

public class RequestShelterSpacePayload
{
    public string ShelterId { get; set; } = string.Empty;
    public string? CitizenName { get; set; }
    public string? ContactPhone { get; set; }
    public int NumberOfPeople { get; set; } = 1;
    public string? SpecialNeeds { get; set; }
}

public class ShelterBookingDto
{
    public string BookingReference { get; set; } = string.Empty;
    public string ShelterId { get; set; } = string.Empty;
    public string ShelterName { get; set; } = string.Empty;
    public string CitizenName { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public int NumberOfPeople { get; set; }
    public string? SpecialNeeds { get; set; }
    public string Status { get; set; } = "Confirmed";
    public DateTime BookedAt { get; set; }
    public string ConfirmationMessage { get; set; } = string.Empty;
}

public class ShelterSummaryDto
{
    public int TotalShelters { get; set; }
    public int AvailableSheltersCount { get; set; }
    public int FullSheltersCount { get; set; }
    public int TotalCapacity { get; set; }
    public int TotalAvailableSpaces { get; set; }
    public int TotalOccupiedSpaces { get; set; }
    public int AccessibleCount { get; set; }
    public ShelterDto? NearestAvailableShelter { get; set; }
}
