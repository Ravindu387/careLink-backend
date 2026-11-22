using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class BulkDonorActionDto
{
    [Required]
    public List<string> Ids { get; set; } = new();

    [Required]
    public string Action { get; set; } = string.Empty; // "Activate" | "Restrict" | "Verify"
}
