using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class CreateNotificationDto
{
    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    public string Type { get; set; } = "System";

    public string Icon { get; set; } = "resident";

    public string Details { get; set; } = string.Empty;

    public string Reference { get; set; } = string.Empty;

    public string Role { get; set; } = "ShelterAdmin";
}
