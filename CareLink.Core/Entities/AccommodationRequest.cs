namespace CareLink.Core.Entities;

public class AccommodationRequest
{
    public string Id { get; set; } = string.Empty;
    public string Applicant { get; set; } = string.Empty;
    public int FamilyMembers { get; set; }
    public string CurrentLocation { get; set; } = string.Empty;
    public string RequestedShelter { get; set; } = string.Empty;
    public string ShelterId { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium";
    public string Status { get; set; } = "Pending";
    public DateTime RequestDate { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
