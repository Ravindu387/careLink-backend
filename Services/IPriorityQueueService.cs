using CareLink.Backend.Models;

namespace CareLink.Backend.Services;

public interface IPriorityQueueService
{
    Task<PriorityQueueStatsDto> GetStatsAsync();
    Task<PaginatedResult<PriorityQueueItem>> GetCasesAsync(PriorityQueueQueryDto query);
    Task<PriorityQueueItem?> GetByIdAsync(string id);
    Task<int> ClaimCasesAsync(ClaimPriorityCasesRequest request);
    Task<PriorityQueueItem?> UpdateCaseAsync(string id, UpdatePriorityCaseRequest request);
    Task<byte[]> ExportCsvAsync(PriorityQueueQueryDto query);
}
