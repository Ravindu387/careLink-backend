using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class CreateRoomDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Shelter { get; set; } = string.Empty;

    [Required]
    public string Floor { get; set; } = "Ground floor";

    [Required]
    public string Type { get; set; } = "Family Room";

    [Range(1, 100)]
    public int BedCount { get; set; } = 4;
}
