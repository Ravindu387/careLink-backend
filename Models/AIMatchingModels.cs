namespace CareLink.Backend.Models;

public class MatchingTask
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public List<string> Skills { get; set; } = [];
    public string Priority { get; set; } = "Medium"; // Critical, High, Medium
    public string Status { get; set; } = "Open";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class MatchingCandidate
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
    public List<string> Skills { get; set; } = [];
    public string Distance { get; set; } = string.Empty;
    public string Availability { get; set; } = "Available now"; // Available now, Available 14:00, Pending verification
    public int Score { get; set; } = 80;
    public bool Verified { get; set; } = true;
    public string Experience { get; set; } = string.Empty;
    public int CompletedTasks { get; set; }
    public List<string> Checks { get; set; } = [];
    public string? Warning { get; set; }
}

public class MatchingDecisionRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TaskId { get; set; } = string.Empty;
    public string TaskTitle { get; set; } = string.Empty;
    public string CandidateId { get; set; } = string.Empty;
    public string CandidateName { get; set; } = string.Empty;
    public string Decision { get; set; } = "Approved"; // Approved, Rejected, Deferred
    public string Rationale { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class RecordDecisionRequest
{
    public string TaskId { get; set; } = string.Empty;
    public string CandidateId { get; set; } = string.Empty;
    public string CandidateName { get; set; } = string.Empty;
    public string Decision { get; set; } = "Approved";
    public string Rationale { get; set; } = string.Empty;
}

public class MatchingSummaryDto
{
    public int TotalOpenTasks { get; set; }
    public int TotalVerifiedCandidates { get; set; }
    public string ModelVersion { get; set; } = "Model v2.4";
    public int DecisionsCount { get; set; }
}
