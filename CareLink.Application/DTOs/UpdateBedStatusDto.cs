using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class UpdateBedStatusDto
{
    [Required]
    public string Status { get; set; } = "Available";

    public string? Resident { get; set; }
}
