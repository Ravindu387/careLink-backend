using CareLink.Backend.Models;

namespace CareLink.Backend.Services;

public class AIVolunteerMatchingService : IAIVolunteerMatchingService
{
    private readonly List<MatchingTask> _tasks = [];
    private readonly List<MatchingCandidate> _candidatePool = [];
    private readonly List<MatchingDecisionRecord> _decisions = [];
    private readonly object _lock = new();

    public AIVolunteerMatchingService()
    {
        InitializeDemonstrationData();
    }

    private void InitializeDemonstrationData()
    {
        _tasks.AddRange([
            new MatchingTask
            {
                Id = "TASK-782",
                Title = "Evacuation transport support",
                Location = "North Ward",
                Skills = ["Driving", "Rescue"],
                Priority = "Critical",
                Status = "Open",
                CreatedAt = DateTime.UtcNow.AddHours(-3)
            },
            new MatchingTask
            {
                Id = "TASK-779",
                Title = "Medical point registration",
                Location = "Central Clinic",
                Skills = ["First Aid", "Admin"],
                Priority = "High",
                Status = "Open",
                CreatedAt = DateTime.UtcNow.AddHours(-4)
            },
            new MatchingTask
            {
                Id = "TASK-774",
                Title = "Food pack distribution",
                Location = "Eastbank Hub",
                Skills = ["Logistics"],
                Priority = "High",
                Status = "Open",
                CreatedAt = DateTime.UtcNow.AddHours(-5)
            },
            new MatchingTask
            {
                Id = "TASK-768",
                Title = "Shelter childcare support",
                Location = "Riverton School",
                Skills = ["Childcare"],
                Priority = "Medium",
                Status = "Open",
                CreatedAt = DateTime.UtcNow.AddHours(-6)
            },
            new MatchingTask
            {
                Id = "TASK-755",
                Title = "Water purification unit setup",
                Location = "West Warehouse",
                Skills = ["Logistics", "Technical Support"],
                Priority = "High",
                Status = "Open",
                CreatedAt = DateTime.UtcNow.AddHours(-7)
            }
        ]);

        _candidatePool.AddRange([
            new MatchingCandidate
            {
                Id = "VOL-1048",
                Name = "Elena Rossi",
                Initials = "ER",
                Skills = ["First Aid", "Logistics", "Driving"],
                Distance = "0.8 km",
                Availability = "Available now",
                Score = 96,
                Verified = true,
                Experience = "Experienced in evacuation support and on-site patient intake.",
                CompletedTasks = 8,
                Checks =
                [
                    "Required skills verified",
                    "Available during full task window",
                    "Within recommended travel range (0.8 km)",
                    "Completed 8 similar tasks with positive feedback",
                    "No conflicting assignments"
                ]
            },
            new MatchingCandidate
            {
                Id = "VOL-1172",
                Name = "Musa Bello",
                Initials = "MB",
                Skills = ["Driving", "Rescue", "Logistics"],
                Distance = "1.4 km",
                Availability = "Available 14:00",
                Score = 93,
                Verified = true,
                Experience = "Experienced rescue and commercial driving volunteer.",
                CompletedTasks = 12,
                Checks =
                [
                    "Driving and rescue skills certified and verified",
                    "Availability confirmed from 14:00 onwards",
                    "Within recommended travel range (1.4 km)",
                    "Completed 12 similar tasks successfully",
                    "No conflicting emergency assignments"
                ]
            },
            new MatchingCandidate
            {
                Id = "VOL-0934",
                Name = "Lin Wei",
                Initials = "LW",
                Skills = ["Translation", "Medical", "First Aid"],
                Distance = "2.1 km",
                Availability = "Available now",
                Score = 87,
                Verified = true,
                Experience = "Medical triage assistant and multilingual communication support.",
                CompletedTasks = 6,
                Checks =
                [
                    "Medical triage & First Aid credentials verified",
                    "Available during complete task window",
                    "Within reachable response zone (2.1 km)",
                    "Completed 6 similar tasks in field clinics",
                    "No conflicting assignments"
                ]
            },
            new MatchingCandidate
            {
                Id = "VOL-1288",
                Name = "Sara Khan",
                Initials = "SK",
                Skills = ["Shelter Support", "Childcare", "Admin"],
                Distance = "1.1 km",
                Availability = "Pending verification",
                Score = 72,
                Verified = false,
                Experience = "Shelter coordination and community childcare support experience.",
                CompletedTasks = 4,
                Checks =
                [
                    "Skills submitted for background review",
                    "Availability requires final coordinator confirmation",
                    "Within recommended travel range (1.1 km)",
                    "Completed 4 community support tasks"
                ],
                Warning = "Volunteer verification is pending. Approval is not recommended until ID and safety screening are completed."
            },
            new MatchingCandidate
            {
                Id = "VOL-1340",
                Name = "Daniel Okafor",
                Initials = "DO",
                Skills = ["Logistics", "Technical Support", "Driving"],
                Distance = "1.9 km",
                Availability = "Available now",
                Score = 89,
                Verified = true,
                Experience = "Heavy equipment logistics and warehouse field distribution.",
                CompletedTasks = 15,
                Checks =
                [
                    "Logistics and equipment certification verified",
                    "Available immediately for emergency deployment",
                    "Within recommended distance (1.9 km)",
                    "Completed 15 emergency distribution operations",
                    "Clean safety tracking record"
                ]
            }
        ]);
    }

    public Task<List<MatchingTask>> GetTasksAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(_tasks.ToList());
        }
    }

    public Task<MatchingTask?> GetTaskByIdAsync(string taskId)
    {
        lock (_lock)
        {
            var task = _tasks.FirstOrDefault(t => t.Id.Equals(taskId, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(task);
        }
    }

    public Task<List<MatchingCandidate>> GetCandidatesForTaskAsync(string taskId, string? search = null)
    {
        lock (_lock)
        {
            var task = _tasks.FirstOrDefault(t => t.Id.Equals(taskId, StringComparison.OrdinalIgnoreCase));
            var candidates = _candidatePool.Select(c => CloneCandidateWithTaskScore(c, task)).ToList();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLowerInvariant();
                candidates = candidates.Where(c =>
                    c.Name.ToLowerInvariant().Contains(s) ||
                    c.Id.ToLowerInvariant().Contains(s) ||
                    string.Join(" ", c.Skills).ToLowerInvariant().Contains(s) ||
                    c.Availability.ToLowerInvariant().Contains(s)).ToList();
            }

            return Task.FromResult(candidates.OrderByDescending(c => c.Score).ToList());
        }
    }

    public Task<MatchingSummaryDto> GetSummaryAsync()
    {
        lock (_lock)
        {
            var summary = new MatchingSummaryDto
            {
                TotalOpenTasks = _tasks.Count(t => t.Status.Equals("Open", StringComparison.OrdinalIgnoreCase)),
                TotalVerifiedCandidates = _candidatePool.Count(c => c.Verified),
                ModelVersion = "Model v2.4",
                DecisionsCount = _decisions.Count
            };
            return Task.FromResult(summary);
        }
    }

    public Task<MatchingDecisionRecord> RecordDecisionAsync(RecordDecisionRequest request)
    {
        lock (_lock)
        {
            var task = _tasks.FirstOrDefault(t => t.Id.Equals(request.TaskId, StringComparison.OrdinalIgnoreCase));
            var candidate = _candidatePool.FirstOrDefault(c => c.Id.Equals(request.CandidateId, StringComparison.OrdinalIgnoreCase));

            var record = new MatchingDecisionRecord
            {
                Id = $"DEC-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
                TaskId = request.TaskId,
                TaskTitle = task?.Title ?? request.TaskId,
                CandidateId = request.CandidateId,
                CandidateName = candidate?.Name ?? (!string.IsNullOrWhiteSpace(request.CandidateName) ? request.CandidateName : request.CandidateId),
                Decision = request.Decision,
                Rationale = request.Rationale,
                Date = DateTime.UtcNow.ToString("dd MMM yyyy, HH:mm UTC"),
                CreatedAt = DateTime.UtcNow
            };

            _decisions.Insert(0, record);

            // If approved, update task status if desired
            if (request.Decision.Equals("Approved", StringComparison.OrdinalIgnoreCase) && task != null)
            {
                task.Status = "Assigned";
            }

            return Task.FromResult(record);
        }
    }

    public Task<List<MatchingDecisionRecord>> GetDecisionHistoryAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(_decisions.OrderByDescending(d => d.CreatedAt).ToList());
        }
    }

    public Task<bool> DeleteDecisionAsync(string id)
    {
        lock (_lock)
        {
            var item = _decisions.FirstOrDefault(d => d.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (item != null)
            {
                _decisions.Remove(item);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
    }

    private static MatchingCandidate CloneCandidateWithTaskScore(MatchingCandidate candidate, MatchingTask? task)
    {
        var clone = new MatchingCandidate
        {
            Id = candidate.Id,
            Name = candidate.Name,
            Initials = candidate.Initials,
            Skills = [.. candidate.Skills],
            Distance = candidate.Distance,
            Availability = candidate.Availability,
            Verified = candidate.Verified,
            Experience = candidate.Experience,
            CompletedTasks = candidate.CompletedTasks,
            Checks = [.. candidate.Checks],
            Warning = candidate.Warning
        };

        if (task == null)
        {
            clone.Score = candidate.Score;
            return clone;
        }

        // Calculate dynamic AI suitability score
        int baseScore = 60;

        // Skill match
        var matchingSkills = candidate.Skills.Intersect(task.Skills, StringComparer.OrdinalIgnoreCase).Count();
        if (matchingSkills > 0)
        {
            baseScore += matchingSkills * 15;
        }

        // Availability boost
        if (candidate.Availability.Equals("Available now", StringComparison.OrdinalIgnoreCase))
        {
            baseScore += 10;
        }
        else if (candidate.Availability.Equals("Available 14:00", StringComparison.OrdinalIgnoreCase))
        {
            baseScore += 5;
        }

        // Experience bonus
        if (candidate.CompletedTasks >= 10) baseScore += 8;
        else if (candidate.CompletedTasks >= 5) baseScore += 5;

        // Unverified penalty
        if (!candidate.Verified)
        {
            baseScore -= 18;
        }

        clone.Score = Math.Clamp(baseScore, 45, 99);
        return clone;
    }
}
