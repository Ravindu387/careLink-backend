using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class CreateAccommodationRequestDto
{
    [Required]
    public string Applicant { get; set; } = string.Empty;

    [Range(1, 100)]
    public int FamilyMembers { get; set; } = 1;

    [Required]
    public string CurrentLocation { get; set; } = string.Empty;

    [Required]
    public string RequestedShelter { get; set; } = string.Empty;

    public string? Priority { get; set; } = "Medium";
}
