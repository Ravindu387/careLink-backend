using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class SummaryCardDto
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string IconClass { get; set; } = string.Empty;
}

public class PartnerActivityDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
}

public class PartnerDashboardOrgDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string VerificationStatus { get; set; } = string.Empty;
    public string AssignedRegion { get; set; } = string.Empty;
    public int FieldLocationsCount { get; set; }
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
}

public class PartnerDashboardResponseDto
{
    public string ActiveCrisisName { get; set; } = string.Empty;
    public List<SummaryCardDto> SummaryCards { get; set; } = new();
    public List<PartnerActivityDto> Activities { get; set; } = new();
    public PartnerDashboardOrgDto Organisation { get; set; } = new();
    public DateTimeOffset LastUpdated { get; set; }
}

public class CreatePartnerActivityDto
{
    [Required(ErrorMessage = "Activity title is required.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    public string Description { get; set; } = string.Empty;

    public string Icon { get; set; } = "ClipboardList";
    public string Color { get; set; } = "text-blue-600 bg-blue-50";
}
