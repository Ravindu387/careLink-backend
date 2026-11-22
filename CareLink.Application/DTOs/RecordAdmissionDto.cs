using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class RecordAdmissionDto
{
    [Required]
    public string Resident { get; set; } = string.Empty;

    [Required]
    public string ShelterId { get; set; } = string.Empty;

    [Range(1, 1000)]
    public int People { get; set; } = 1;

    public string? Date { get; set; }
}
