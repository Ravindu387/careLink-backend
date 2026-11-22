using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class PartnerOrganisationDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "Local NGO";
    public string Region { get; set; } = "Greater Riverton";
    public string Role { get; set; } = "Shelter Operations";
    public string Verification { get; set; } = "Pending";
    public string Status { get; set; } = "Active";
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public int SharedResourcesCount { get; set; }
    public int CollaborationRequestsCount { get; set; }
    public string? Address { get; set; }
    public string? Website { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? VerifiedAt { get; set; }
}

public class PartnerOrganisationsMetricsDto
{
    public int ActivePartnersCount { get; set; }
    public int TotalPartnersCount { get; set; }
    public int SharedResourcesCount { get; set; }
    public int CollaborationRequestsCount { get; set; }
    public int PendingCollaborationRequestsCount { get; set; }
    public int VerificationCoveragePercent { get; set; }
    public int PendingVerificationReviewsCount { get; set; }
    public int SectorsCount { get; set; } = 6;
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
}

public class CreatePartnerOrganisationDto
{
    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Type { get; set; } = "Local NGO";

    [Required]
    [StringLength(100)]
    public string Region { get; set; } = "Greater Riverton";

    [Required]
    [StringLength(100)]
    public string Role { get; set; } = "Shelter Operations";

    [Required]
    [EmailAddress]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Phone { get; set; } = string.Empty;

    [StringLength(150)]
    public string? ContactPerson { get; set; }

    [StringLength(250)]
    public string? Address { get; set; }

    [StringLength(250)]
    public string? Website { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}

public class UpdatePartnerOrganisationDto
{
    [StringLength(150)]
    public string? Name { get; set; }

    [StringLength(100)]
    public string? Type { get; set; }

    [StringLength(100)]
    public string? Region { get; set; }

    [StringLength(100)]
    public string? Role { get; set; }

    [EmailAddress]
    [StringLength(150)]
    public string? Email { get; set; }

    [StringLength(50)]
    public string? Phone { get; set; }

    [StringLength(150)]
    public string? ContactPerson { get; set; }

    [RegularExpression("^(Active|Inactive)$", ErrorMessage = "Status must be 'Active' or 'Inactive'.")]
    public string? Status { get; set; }

    [RegularExpression("^(Verified|Pending|Rejected)$", ErrorMessage = "Verification must be 'Verified', 'Pending', or 'Rejected'.")]
    public string? Verification { get; set; }

    [StringLength(250)]
    public string? Address { get; set; }

    [StringLength(250)]
    public string? Website { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}

public class UpdatePartnerStatusDto
{
    [Required]
    [RegularExpression("^(Active|Inactive)$", ErrorMessage = "Status must be 'Active' or 'Inactive'.")]
    public string Status { get; set; } = "Active";
}

public class UpdatePartnerVerificationDto
{
    [Required]
    [RegularExpression("^(Verified|Pending|Rejected)$", ErrorMessage = "Verification must be 'Verified', 'Pending', or 'Rejected'.")]
    public string Verification { get; set; } = "Pending";
}

public class PartnerOrganisationQueryParameters
{
    public string? Search { get; set; }
    public string? Region { get; set; }
    public string? Status { get; set; }
    public string? Verification { get; set; }
    public string? Type { get; set; }
    public string? Role { get; set; }
}
