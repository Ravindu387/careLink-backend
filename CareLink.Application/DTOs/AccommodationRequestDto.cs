namespace CareLink.Application.DTOs;

public class AccommodationRequestDto
{
    public string Id { get; set; } = string.Empty;
    public string Applicant { get; set; } = string.Empty;
    public int FamilyMembers { get; set; }
    public string CurrentLocation { get; set; } = string.Empty;
    public string RequestedShelter { get; set; } = string.Empty;
    public string ShelterId { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string RequestDate { get; set; } = string.Empty;
    public DateTime RequestDateUtc { get; set; }
}
