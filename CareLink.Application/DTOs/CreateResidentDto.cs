using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class CreateResidentDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Range(1, 130)]
    public int Age { get; set; }

    [Required]
    public string Gender { get; set; } = "Female";

    [Required]
    public string Contact { get; set; } = string.Empty;

    public string? FamilyGroup { get; set; }

    [Required]
    public string ShelterId { get; set; } = string.Empty;

    [Required]
    public string Room { get; set; } = string.Empty;

    [Required]
    public string Bed { get; set; } = string.Empty;

    public string? AssistanceNeeds { get; set; } = "None";
}
