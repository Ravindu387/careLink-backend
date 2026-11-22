using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class CreateNotificationDto
{
    [Required]
    [StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    public string Category { get; set; } = "critical";
    public string Type { get => Category; set => Category = value; }

    public string Icon { get; set; } = "resident";
    public bool? Urgent { get; set; }
    public string Role { get; set; } = "ShelterAdmin";
    public string? TargetRole { get => Role; set => Role = value ?? "ShelterAdmin"; }

    public string? Reference { get; set; }

    [StringLength(2000)]
    public string? Details { get; set; }

    public string? ActionUrl { get; set; }
    public string? ActionLabel { get; set; }
}
