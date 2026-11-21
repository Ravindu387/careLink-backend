namespace CareLink.Core.Entities;

public class HouseholdMember
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
    public string Role { get; set; } = "Household member";
    public string SupportNeeds { get; set; } = "No support needs";
    public int? Age { get; set; }
    public string? MedicalNotes { get; set; }
    public string? DietaryNotes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public class ReadinessChecklistItem
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public string? ActionText { get; set; }
}

public class HouseholdProfile
{
    public string Id { get; set; } = "HH-001";
    public string CitizenName { get; set; } = "Samira Khan";
    public string CitizenId { get; set; } = "CIT-001";
    public string Address { get; set; } = "54 Riverside Lane, North Ward, Greater Riverton";
    public string Ward { get; set; } = "North Ward";
    public string City { get; set; } = "Greater Riverton";
    public string EmergencyContactName { get; set; } = "Rashid Khan";
    public string EmergencyContactPhone { get; set; } = "+94 77 455 0182";
    public string EmergencyContactRelationship { get; set; } = "Brother";
    public List<HouseholdMember> Members { get; set; } = new();
    public List<ReadinessChecklistItem> ReadinessChecklist { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
