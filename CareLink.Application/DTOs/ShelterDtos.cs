using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class ShelterDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int Occupied { get; set; }
    public int Remaining { get; set; }
    public List<string> Services { get; set; } = new();
    public string Status { get; set; } = "Operational";
    public string ContactPerson { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

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

public class ShelterQueryParameters
{
    public string? Search { get; set; }
    public string? Area { get; set; }
    public string? Status { get; set; }
    public string? Availability { get; set; }
    public string? Service { get; set; }
}

public class CreateShelterDto
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
    public int Occupied { get; set; } = 0;

    public List<string>? Services { get; set; }

    [StringLength(100)]
    public string? ContactPerson { get; set; }

    [StringLength(50)]
    public string? ContactPhone { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
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
