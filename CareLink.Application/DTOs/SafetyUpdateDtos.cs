namespace CareLink.Application.DTOs;

public class SafetyUpdateDto
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string? ActionInstructions { get; set; }
    public List<string> SafeRoutes { get; set; } = new();
    public List<string> HazardAreas { get; set; } = new();
    public string Helpline { get; set; } = "112";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class CreateSafetyUpdateRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Level { get; set; } = "Emergency alert";
    public string? Type { get; set; }
    public string? Severity { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Area { get; set; } = "Greater Riverton";
    public string? Source { get; set; }
    public string? ActionInstructions { get; set; }
    public List<string>? SafeRoutes { get; set; }
    public List<string>? HazardAreas { get; set; }
    public string? Helpline { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class UpdateSafetyUpdateRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Level { get; set; }
    public string? Severity { get; set; }
    public string? Location { get; set; }
    public string? Area { get; set; }
    public string? Source { get; set; }
    public string? ActionInstructions { get; set; }
    public List<string>? SafeRoutes { get; set; }
    public List<string>? HazardAreas { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class SafetyUpdateSummaryDto
{
    public int TotalUpdates { get; set; }
    public int EmergencyCount { get; set; }
    public int WarningsCount { get; set; }
    public int ServiceUpdatesCount { get; set; }
    public List<string> MonitoredAreas { get; set; } = new();
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
}
