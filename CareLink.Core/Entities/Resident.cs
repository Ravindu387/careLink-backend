namespace CareLink.Core.Entities;

public class Resident
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string Contact { get; set; } = string.Empty;
    public string FamilyGroup { get; set; } = string.Empty;
    public string ShelterId { get; set; } = string.Empty;
    public string ShelterName { get; set; } = string.Empty;
    public string Room { get; set; } = string.Empty;
    public string Bed { get; set; } = string.Empty;
    public DateTime CheckIn { get; set; }
    public string Status { get; set; } = "Active"; // Active | Assistance | Checked Out
    public string AssistanceNeeds { get; set; } = "None";
    public DateTime? CheckedOutAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
