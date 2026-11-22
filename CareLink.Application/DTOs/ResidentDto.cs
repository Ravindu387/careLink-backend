namespace CareLink.Application.DTOs;

public class ResidentDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string Contact { get; set; } = string.Empty;
    public string FamilyGroup { get; set; } = string.Empty;
    public string Shelter { get; set; } = string.Empty;
    public string ShelterId { get; set; } = string.Empty;
    public string Room { get; set; } = string.Empty;
    public string Bed { get; set; } = string.Empty;
    public string CheckIn { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public string Status { get; set; } = "Active";
    public string AssistanceNeeds { get; set; } = "None";
    public DateTime? CheckedOutAt { get; set; }
}
