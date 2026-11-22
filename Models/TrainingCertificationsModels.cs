namespace CareLink.Backend.Models;

public class TrainingCertStatsDto
{
    public string TrainingCompletionRate { get; set; } = "88%";
    public string CompletionSubtitle { get; set; } = "Mandatory modules";
    public string CertificationsValid { get; set; } = "1,842";
    public string ValidSubtitle { get; set; } = "Across verified volunteers";
    public string ExpiringSoon { get; set; } = "42";
    public string ExpiringSubtitle { get; set; } = "Within 30 days";
    public string Expired { get; set; } = "18";
    public string ExpiredSubtitle { get; set; } = "Matching suspended";
}

public class TrainingDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Requirement { get; set; } = "Required"; // Required, Skill-based, Optional
    public string Duration { get; set; } = "45 min";
    public int Enrolled { get; set; } = 0;
    public int Completion { get; set; } = 0;
    public string Status { get; set; } = "Active"; // Active, Inactive
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class CertificateDto
{
    public string Id { get; set; } = string.Empty;
    public string Volunteer { get; set; } = string.Empty;
    public string Qualification { get; set; } = string.Empty;
    public string Expiry { get; set; } = string.Empty;
    public string Remaining { get; set; } = string.Empty;
    public string Status { get; set; } = "Valid"; // Valid, Expiring Soon, Expired
    public string? DocumentUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class TrainingOverviewDto
{
    public TrainingCertStatsDto Stats { get; set; } = new();
    public List<TrainingDto> Trainings { get; set; } = [];
    public List<CertificateDto> Certificates { get; set; } = [];
}

public class CreateTrainingRequest
{
    public string Name { get; set; } = string.Empty;
    public string Requirement { get; set; } = "Required";
    public string Duration { get; set; } = "45 min";
    public int Enrolled { get; set; } = 0;
    public int Completion { get; set; } = 0;
}

public class UpdateTrainingStatusRequest
{
    public string Status { get; set; } = "Active";
}

public class CreateCertificateRequest
{
    public string Volunteer { get; set; } = string.Empty;
    public string Qualification { get; set; } = string.Empty;
    public string Expiry { get; set; } = string.Empty;
    public string? FileName { get; set; }
}

public class UpdateCertificateStatusRequest
{
    public string Status { get; set; } = "Valid";
}
