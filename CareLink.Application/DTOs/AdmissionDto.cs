namespace CareLink.Application.DTOs;

public class AdmissionDto
{
    public string Id { get; set; } = string.Empty;
    public string Resident { get; set; } = string.Empty;
    public string ShelterId { get; set; } = string.Empty;
    public string ShelterName { get; set; } = string.Empty;
    public int People { get; set; }
    public string Date { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
