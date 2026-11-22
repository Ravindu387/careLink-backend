namespace CareLink.Application.DTOs;

public class CitizenHelpRequestDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Under review";
    public string Priority { get; set; } = "Normal";
    public string CitizenName { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? District { get; set; }
    public string Color { get; set; } = "blue";
    public bool ImmediateThreat { get; set; }
    public string? AdditionalInfo { get; set; }
    public int PeopleCount { get; set; } = 1;
    public int Progress { get; set; } = 1;
    public string? TimeAgo { get; set; }
    public string? FormattedDate { get; set; }
    public string? Updated { get; set; }
    public bool CanMessage { get; set; } = true;
    public string? DispatchTrackingId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateCitizenHelpRequestPayload
{
    public string Category { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Situation { get; set; }
    public string? Description { get; set; }
    public bool ImmediateThreat { get; set; }
    public string? Address { get; set; }
    public string? District { get; set; }
    public string? ContactName { get; set; }
    public string? CitizenName { get; set; }
    public string? Phone { get; set; }
    public string? ContactPhone { get; set; }
    public string? AdditionalInfo { get; set; }
    public int? PeopleCount { get; set; }
    public string? Priority { get; set; }
}

public class UpdateRequestStatusPayload
{
    public string Status { get; set; } = string.Empty;
    public int? Progress { get; set; }
}
