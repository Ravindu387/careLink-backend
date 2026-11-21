using CareLink.Backend.Models;

namespace CareLink.Backend.Services;

public interface IAIVolunteerMatchingService
{
    Task<List<MatchingTask>> GetTasksAsync();
    Task<MatchingTask?> GetTaskByIdAsync(string taskId);
    Task<List<MatchingCandidate>> GetCandidatesForTaskAsync(string taskId, string? search = null);
    Task<MatchingSummaryDto> GetSummaryAsync();
    Task<MatchingDecisionRecord> RecordDecisionAsync(RecordDecisionRequest request);
    Task<List<MatchingDecisionRecord>> GetDecisionHistoryAsync();
    Task<bool> DeleteDecisionAsync(string id);
}
