using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class FieldActivityDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Scheduled { get; set; } = string.Empty;
    public string Team { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public string Sector { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? BeneficiariesReached { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
}

public class CreateFieldActivityDto
{
    [Required(ErrorMessage = "Activity name is required.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Location is required.")]
    public string Location { get; set; } = string.Empty;

    [Required(ErrorMessage = "Scheduled time or date is required.")]
    public string Scheduled { get; set; } = string.Empty;

    [Required(ErrorMessage = "Assigned team is required.")]
    public string Team { get; set; } = "Field Team North";

    [Required(ErrorMessage = "Target or result is required.")]
    public string Target { get; set; } = string.Empty;

    [Required(ErrorMessage = "Sector is required.")]
    public string Sector { get; set; } = "Food Security";

    public string? Status { get; set; } = "Scheduled";
    public string? Description { get; set; }
}

public class UpdateFieldActivityDto
{
    public string? Name { get; set; }
    public string? Location { get; set; }
    public string? Scheduled { get; set; }
    public string? Team { get; set; }
    public string? Target { get; set; }
    public string? Sector { get; set; }
    public string? Status { get; set; }
    public string? Description { get; set; }
    public int? BeneficiariesReached { get; set; }
}

public class UpdateFieldActivityStatusDto
{
    [Required(ErrorMessage = "Status is required.")]
    public string Status { get; set; } = string.Empty;
}

public class FieldActivityMetricsDto
{
    public int ActiveToday { get; set; }
    public string PeopleTargeted { get; set; } = "4,840";
    public string PeopleReachedToday { get; set; } = "1,864";
    public int TeamsDeployed { get; set; }
    public int StaffInField { get; set; }
    public int UpdatesOverdue { get; set; }
    public int TotalActivities { get; set; }
    public int NeedsActionCount { get; set; }
    public int ActiveCount { get; set; }
    public int CompletedCount { get; set; }
    public int ArchivedCount { get; set; }
    public DateTimeOffset LastUpdated { get; set; }
}

public class FieldActivityQueryParameters
{
    public string? Search { get; set; }
    public string? Tab { get; set; }
    public string? Sector { get; set; }
    public string? Location { get; set; }
    public string? Team { get; set; }
    public string? Status { get; set; }
}
