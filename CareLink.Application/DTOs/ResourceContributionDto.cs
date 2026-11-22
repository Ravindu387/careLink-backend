namespace CareLink.Application.DTOs;

public class ResourceContributionDto
{
    public string Id { get; set; } = string.Empty;
    public string Donor { get; set; } = string.Empty;
    public string Type { get; set; } = "Material";
    public string Quantity { get; set; } = string.Empty;
    public string Campaign { get; set; } = string.Empty;
    public string Received { get; set; } = string.Empty;
    public DateTime ReceivedDate { get; set; }
    public string Verification { get; set; } = "Pending";
    public string Allocation { get; set; } = "Unallocated";
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateResourceContributionDto
{
    public string? Id { get; set; }
    public string Donor { get; set; } = string.Empty;
    public string Type { get; set; } = "Material";
    public string Quantity { get; set; } = string.Empty;
    public string Campaign { get; set; } = string.Empty;
    public string? Received { get; set; }
    public string? ReceivedDate { get; set; } // YYYY-MM-DD
    public string? Notes { get; set; }
}

public class UpdateResourceContributionDto
{
    public string? Donor { get; set; }
    public string? Type { get; set; }
    public string? Quantity { get; set; }
    public string? Campaign { get; set; }
    public string? Verification { get; set; }
    public string? Allocation { get; set; }
    public string? Notes { get; set; }
}

public class BulkVerifyResourceContributionsDto
{
    public List<string> Ids { get; set; } = new();
}

public class UpdateAllocationStatusDto
{
    public string Allocation { get; set; } = "Allocated";
}

public class UpdateVerificationStatusDto
{
    public string Verification { get; set; } = "Verified";
}
