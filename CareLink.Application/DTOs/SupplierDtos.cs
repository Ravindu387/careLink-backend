namespace CareLink.Application.DTOs;

public class SupplierDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Coverage { get; set; } = string.Empty;
    public string OnTimeRate { get; set; } = string.Empty;
    public string LeadTime { get; set; } = string.Empty;
    public string Verification { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateSupplierRequest
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = "Medical";
    public string Coverage { get; set; } = string.Empty;
    public string OnTimeRate { get; set; } = "95%";
    public string LeadTime { get; set; } = "2.0 days";
    public string Verification { get; set; } = "Pending";
    public string Status { get; set; } = "Active";
}

public class UpdateSupplierStatusRequest
{
    public string? Status { get; set; }
    public string? Verification { get; set; }
}

public class SupplierSummaryDto
{
    public int ActiveCount { get; set; }
    public int PendingCount { get; set; }
    public int VerifiedCount { get; set; }
    public int ExceptionCount { get; set; }
    public int TotalCount { get; set; }
}
