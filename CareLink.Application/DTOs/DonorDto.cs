namespace CareLink.Application.DTOs;

public class DonorDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Contact { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public string Verification { get; set; } = "Pending";
    public decimal Contributions { get; set; }
    public string Status { get; set; } = "Active";
    public string? Address { get; set; }
    public string? Notes { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
    public string? UpdatedAt { get; set; }
}
