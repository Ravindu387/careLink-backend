using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class AddBedsDto
{
    [Required]
    public string RoomId { get; set; } = string.Empty;

    [Range(1, 100)]
    public int Count { get; set; } = 1;
}
