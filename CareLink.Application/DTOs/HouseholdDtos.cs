namespace CareLink.Application.DTOs;

public class HouseholdMemberDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
    public string Role { get; set; } = "Household member";
    public string Support { get; set; } = "No support needs";
    public int? Age { get; set; }
    public string? MedicalNotes { get; set; }
    public string? DietaryNotes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ReadinessChecklistItemDto
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public string? ActionText { get; set; }
}

public class HouseholdSummaryDto
{
    public int TotalMembers { get; set; }
    public int SupportNeededCount { get; set; }
    public int ReadinessCompletedCount { get; set; }
    public int ReadinessTotalCount { get; set; }
    public int ReadinessPercentage { get; set; }
}

public class HouseholdProfileDto
{
    public string Id { get; set; } = string.Empty;
    public string CitizenName { get; set; } = string.Empty;
    public string CitizenId { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Ward { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string EmergencyContactName { get; set; } = string.Empty;
    public string EmergencyContactPhone { get; set; } = string.Empty;
    public string EmergencyContactRelationship { get; set; } = string.Empty;
    public List<HouseholdMemberDto> Members { get; set; } = new();
    public List<ReadinessChecklistItemDto> ReadinessChecklist { get; set; } = new();
    public HouseholdSummaryDto Summary { get; set; } = new();
}

public class CreateHouseholdMemberRequest
{
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = "Household member";
    public string Support { get; set; } = "No support needs";
    public int? Age { get; set; }
    public string? MedicalNotes { get; set; }
    public string? DietaryNotes { get; set; }
}

public class UpdateHouseholdMemberRequest
{
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = "Household member";
    public string Support { get; set; } = "No support needs";
    public int? Age { get; set; }
    public string? MedicalNotes { get; set; }
    public string? DietaryNotes { get; set; }
}

public class UpdateAddressRequest
{
    public string Address { get; set; } = string.Empty;
    public string? Ward { get; set; }
    public string? City { get; set; }
}

public class UpdateEmergencyContactRequest
{
    public string ContactName { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string? Relationship { get; set; }
}

public class ToggleReadinessRequest
{
    public string Key { get; set; } = string.Empty;
    public bool? IsCompleted { get; set; }
}
