namespace CareLink.Application.DTOs;

public class AlertDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string? AssignedTo { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateAlertRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Severity { get; set; } = "Warning";
    public string Icon { get; set; } = "alert";
    public string? AssignedTo { get; set; }
}

public class AlertSummaryDto
{
    public int TotalCount { get; set; }
    public int CriticalCount { get; set; }
    public int WarningCount { get; set; }
    public int AcknowledgedCount { get; set; }
    public int ResolvedCount { get; set; }
}

public class AlertRulesDto
{
    public bool Critical { get; set; }
    public bool LowStock { get; set; }
    public bool Capacity { get; set; }
    public bool Dispatch { get; set; }
    public bool Expiry { get; set; }
}
