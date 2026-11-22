using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class ShelterMetricsDto
{
    public int OperationalShelters { get; set; }
    public int TotalCapacity { get; set; }
    public int TotalOccupied { get; set; }
    public int AvailableSpaces { get; set; }
    public int AcceptingArrivals { get; set; }
    public int NearCapacityCount { get; set; }
    public int FullCount { get; set; }
    public int TotalShelters { get; set; }
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
}

public class UpdateShelterDto
{
    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Location { get; set; } = "Central Ward";

    [Required]
    [Range(1, 50000, ErrorMessage = "Capacity must be at least 1.")]
    public int Capacity { get; set; }

    [Range(0, 50000, ErrorMessage = "Occupied count cannot be negative.")]
    public int Occupied { get; set; }

    public List<string>? Services { get; set; }

    [StringLength(100)]
    public string? ContactPerson { get; set; }

    [StringLength(50)]
    public string? ContactPhone { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}

public class UpdateShelterOccupancyDto
{
    [Required]
    [Range(0, 50000, ErrorMessage = "Occupied count cannot be negative.")]
    public int Occupied { get; set; }
}

// ── Citizen Portal & Resource Manager Shelter DTOs (Oshadi) ──────
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
    // Operational & Health Metrics (HEAD / Prabhani)
    public int Registered { get; set; }
    public int Operational { get; set; }
    public int NearCapacity { get; set; }
    public int Maintenance { get; set; }

    // Citizen Portal Summary Metrics (Oshadi)
    public int TotalShelters { get; set; }
    public int AvailableSheltersCount { get; set; }
    public int FullSheltersCount { get; set; }
    public int TotalCapacity { get; set; }
    public int TotalAvailableSpaces { get; set; }
    public int TotalOccupiedSpaces { get; set; }
    public int AccessibleCount { get; set; }
    public ShelterDto? NearestAvailableShelter { get; set; }
}
