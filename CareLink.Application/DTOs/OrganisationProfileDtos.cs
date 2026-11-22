using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class OrganisationVerificationItemDto
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Status { get; set; } = "Current";
    public bool Done { get; set; } = true;
    public DateTimeOffset? DueDate { get; set; }
}

public class CapabilityMetricDto
{
    public string Title { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public int TotalCount { get; set; }
    public int ActiveCount { get; set; }
}

public class OrganisationCapabilitiesDto
{
    public CapabilityMetricDto Personnel { get; set; } = new();
    public CapabilityMetricDto Fleet { get; set; } = new();
    public CapabilityMetricDto Stock { get; set; } = new();
    public CapabilityMetricDto Coverage { get; set; } = new();
}

public class OrganisationProfileDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string LegalName { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string RegisteredDate { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Verification { get; set; } = string.Empty;
    public string HeadOffice { get; set; } = string.Empty;
    public string PrimaryContact { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string EmergencyLine { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public int ProfileCompleteness { get; set; }
    public int CompletenessReviewItems { get; set; }
    public List<string> Sectors { get; set; } = new();
    public List<OrganisationVerificationItemDto> Verifications { get; set; } = new();
    public OrganisationCapabilitiesDto Capabilities { get; set; } = new();
    public DateTimeOffset UpdatedAt { get; set; }
}

public class UpdateOrganisationProfileDto
{
    [StringLength(150)]
    public string? Name { get; set; }

    [StringLength(200)]
    public string? LegalName { get; set; }

    [StringLength(100)]
    public string? RegistrationNumber { get; set; }

    [StringLength(100)]
    public string? Type { get; set; }

    [StringLength(250)]
    public string? HeadOffice { get; set; }

    [StringLength(150)]
    public string? PrimaryContact { get; set; }

    [EmailAddress]
    [StringLength(150)]
    public string? ContactEmail { get; set; }

    [StringLength(50)]
    public string? EmergencyLine { get; set; }

    [StringLength(250)]
    public string? Website { get; set; }

    public List<string>? Sectors { get; set; }
}

public class UpdateCapabilitiesDto
{
    public int? PersonnelActive { get; set; }
    public int? PersonnelDeployable { get; set; }

    public int? FleetTotal { get; set; }
    public int? FleetAvailable { get; set; }

    public int? StockCategories { get; set; }
    public int? StockAvailabilityPercent { get; set; }

    public int? CoverageTotalDistricts { get; set; }
    public int? CoverageActiveDistricts { get; set; }
}
