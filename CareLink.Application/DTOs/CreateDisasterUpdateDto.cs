using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class CreateDisasterUpdateDto
{
    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Type { get; set; } = "Announcement";

    [Required]
    public string Description { get; set; } = string.Empty;

    public string? Location { get; set; } = "Greater Riverton";
    public string? Source { get; set; } = "CareLink Coordination";
    public string? PublishedBy { get; set; } = "Sanjana A.";
    public string? Instructions { get; set; }
    public bool IsDraft { get; set; } = false;
}
