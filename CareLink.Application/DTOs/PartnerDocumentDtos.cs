using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class PartnerDocumentDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public string Updated { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Visibility { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? FileName { get; set; }
    public string? FileUrl { get; set; }
    public long? FileSizeBytes { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public class CreatePartnerDocumentDto
{
    [Required(ErrorMessage = "Document name is required.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Document type is required.")]
    public string Type { get; set; } = "Operational Plan";

    [Required(ErrorMessage = "Document owner is required.")]
    public string Owner { get; set; } = "Sara Khan";

    [Required(ErrorMessage = "Visibility setting is required.")]
    public string Visibility { get; set; } = "Internal";

    public string? Status { get; set; } = "Under Review";
    public string? Version { get; set; } = "v1.0";
    public string? FileName { get; set; }
    public string? FileUrl { get; set; }
    public long? FileSizeBytes { get; set; }
    public string? Description { get; set; }
}

public class UpdatePartnerDocumentDto
{
    public string? Name { get; set; }
    public string? Type { get; set; }
    public string? Owner { get; set; }
    public string? Visibility { get; set; }
    public string? Status { get; set; }
    public string? Version { get; set; }
    public string? FileName { get; set; }
    public string? FileUrl { get; set; }
    public string? Description { get; set; }
}

public class UpdatePartnerDocumentStatusDto
{
    [Required(ErrorMessage = "Status is required.")]
    public string Status { get; set; } = string.Empty;
}

public class PartnerDocumentMetricsDto
{
    public int ActiveRecords { get; set; }
    public int NeedsReview { get; set; }
    public int VerifiedPercentage { get; set; }
    public int DueSoon { get; set; }
    public int TotalDocuments { get; set; }
    public int NeedsActionCount { get; set; }
    public int ActiveCount { get; set; }
    public int CompletedCount { get; set; }
    public int ArchivedCount { get; set; }
    public DateTimeOffset LastUpdated { get; set; }
}

public class PartnerDocumentQueryParameters
{
    public string? Search { get; set; }
    public string? Tab { get; set; }
    public string? Type { get; set; }
    public string? Owner { get; set; }
    public string? Visibility { get; set; }
    public string? Status { get; set; }
}
