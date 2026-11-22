namespace CareLink.Backend.Models;

public class VolunteerVerificationApplication
{
    public string Id { get; set; } = string.Empty;
    public string Applicant { get; set; } = string.Empty;
    public List<string> Skills { get; set; } = [];
    public string Location { get; set; } = string.Empty;
    public List<string> Documents { get; set; } = [];
    public string Submitted { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Priority { get; set; } = "Standard"; // Standard, High, Urgent
    public string DocumentState { get; set; } = "Complete"; // Complete, Incomplete
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? ReviewNotes { get; set; }
    public string? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
}

public class VolunteerVerificationStatsDto
{
    public int TotalApplications { get; set; }
    public int PendingCount { get; set; }
    public int UrgentCount { get; set; }
    public int HighPriorityCount { get; set; }
    public int StandardPriorityCount { get; set; }
    public int CompleteDocumentsCount { get; set; }
    public int IncompleteDocumentsCount { get; set; }
    public int ApprovedCount { get; set; }
    public int RejectedCount { get; set; }
    public string OldestPendingSubmission { get; set; } = "2 days";
}

public class VolunteerVerificationQueryDto
{
    public string? Search { get; set; }
    public string? Priority { get; set; }
    public string? Skill { get; set; }
    public string? DocumentState { get; set; }
    public string? DateFilter { get; set; }
    public string? Status { get; set; } = "Pending";
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class ReviewVerificationRequest
{
    public string? Notes { get; set; }
    public string? ReviewedBy { get; set; }
}

public class CreateVerificationApplicationRequest
{
    public string Applicant { get; set; } = string.Empty;
    public List<string> Skills { get; set; } = [];
    public string Location { get; set; } = string.Empty;
    public List<string> Documents { get; set; } = [];
    public string Priority { get; set; } = "Standard";
    public string DocumentState { get; set; } = "Complete";
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}
