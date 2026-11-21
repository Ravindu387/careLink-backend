using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs.Notifications;

public class CreateNotificationRequestDto
{
    [Required]
    public string Type { get; set; } = "System"; // Emergency, Assistance, Volunteer, Resource, System

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string Message { get; set; } = string.Empty;

    public string Priority { get; set; } = "Normal"; // Critical, High, Normal

    [MaxLength(150)]
    public string? Location { get; set; }

    [MaxLength(50)]
    public string? Action { get; set; }

    [MaxLength(250)]
    public string? ActionUrl { get; set; }

    public string? TargetUserRole { get; set; }
}
