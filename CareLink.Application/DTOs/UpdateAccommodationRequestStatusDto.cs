using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class UpdateAccommodationRequestStatusDto
{
    [Required]
    public string Status { get; set; } = string.Empty;
}
