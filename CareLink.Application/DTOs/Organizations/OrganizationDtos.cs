namespace CareLink.Application.DTOs.Organizations;

public class OrganizationDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public string? RegistrationNumber { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public int MembersCount { get; set; }
    public string VerificationStatus { get; set; } = "Verified";
    public string Status { get; set; } = "Active";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public string? Description { get; set; }
}

public class OrganizationStatsDto
{
    public int TotalOrganizations { get; set; }
    public int ActiveOrganizations { get; set; }
    public int PendingOrganizations { get; set; }
    public int SuspendedOrganizations { get; set; }
    public int VerifiedOrganizations { get; set; }
}

public class OrganizationQueryDto
{
    public string? Search { get; set; }
    public string? Status { get; set; }
    public string? Type { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class CreateOrganizationRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Scope { get; set; } = "National";
    public string? RegistrationNumber { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Description { get; set; }
}

public class UpdateOrganizationRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public string? RegistrationNumber { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Description { get; set; }
}

public class UpdateOrganizationStatusRequestDto
{
    public string Status { get; set; } = "Active"; // Active | Pending | Suspended
}
