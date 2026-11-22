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
